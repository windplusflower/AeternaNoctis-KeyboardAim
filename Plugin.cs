using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace AeternaKeyboardAim
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cn.codex.aeternanoctis.keyboardaim";
        public const string PluginName = "Aeterna Noctis Keyboard Aim";
        public const string PluginVersion = "1.1.0";

        internal static ConfigEntry<bool> EnableWasdFallback;
        internal static ConfigEntry<KeyCode> FallbackUpKey;
        internal static ConfigEntry<KeyCode> FallbackDownKey;
        internal static ConfigEntry<KeyCode> FallbackLeftKey;
        internal static ConfigEntry<KeyCode> FallbackRightKey;
        internal static ManualLogSource Log;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            EnableWasdFallback = Config.Bind(
                "Keyboard Aim",
                "EnableFallbackKeys",
                true,
                "Read the fallback keys directly if the game suppresses its movement action axes while aiming.");

            FallbackUpKey = Config.Bind("Fallback Keys", "Up", KeyCode.W, "Fallback key for aiming up.");
            FallbackDownKey = Config.Bind("Fallback Keys", "Down", KeyCode.S, "Fallback key for aiming down.");
            FallbackLeftKey = Config.Bind("Fallback Keys", "Left", KeyCode.A, "Fallback key for aiming left.");
            FallbackRightKey = Config.Bind("Fallback Keys", "Right", KeyCode.D, "Fallback key for aiming right.");

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(BowAimCallPatch));
            _harmony.PatchAll(typeof(AimResetPatch));
            _harmony.PatchAll(typeof(ShootDirectionPatch));
            _harmony.PatchAll(typeof(BowControllerUpdatePatch));

            Logger.LogInfo(
                "Keyboard bow aiming enabled with Ori 1 keyboard Bash response. "
                + "It follows the game's Horizontal/Vertical movement bindings.");
        }

        private void OnDestroy()
        {
            // Aeterna Noctis destroys BepInEx_Manager during its frame-0 bootstrap
            // scene transition. All runtime behavior is implemented by static
            // Harmony patches, so removing them here would disable the mod before
            // the first playable frame. Harmony state is process-scoped and is
            // discarded automatically when the game exits.
            Logger.LogWarning(
                "Plugin host was destroyed during game bootstrap; "
                + "keeping keyboard-aim Harmony patches active.");
            _harmony = null;
        }

        private void LateUpdate()
        {
            StandaloneAimController.UpdateFromPluginLateUpdate();
        }
    }

    internal static class AimDirectionPatch
    {
        private const int HorizontalActionId = 0;
        private const int VerticalActionId = 1;
        private const float OriInputThreshold = 0.2f;
        private const float OriKeyboardAcceleration = 2000f;
        private const float OriVisualLerpFactor = 0.5f;
        private const float OriFixedStepSeconds = 1f / 60f;

        private static bool _keyboardEngaged;
        private static bool _loggedFirstKeyboardInput;
        private static bool _loggedPatchReached;
        private static bool _keyboardClockwise;
        private static float _keyboardSpeed;
        private static float _keyboardAngleDegrees;
        private static float _displayAngleDegrees;
        private static float _fixedStepAccumulator;
        private static Vector2 _currentDirection = Vector2.right;
        private static int _lastSimulationFrame = -1;

        internal static Vector2 GetAimDirection(MouseAimingHUD mouseAimingHud)
        {
            Vector2 result = mouseAimingHud.AimDirection;
            UseMovementBindingsForBowAim(ref result, "mouse/keyboard");
            return result;
        }

        internal static Vector2 GetControllerAimDirection(
            Player player,
            int horizontalActionId,
            int verticalActionId)
        {
            Vector2 result = player.GetAxis2DRaw(horizontalActionId, verticalActionId);
            UseMovementBindingsForBowAim(ref result, "controller");
            return result;
        }

        private static void UseMovementBindingsForBowAim(ref Vector2 __result, string runtimeRoute)
        {
            if (!ReInput.isReady)
            {
                return;
            }

            if (!_loggedPatchReached)
            {
                Plugin.Log?.LogInfo($"Bow aim runtime route active via {runtimeRoute} branch.");
                _loggedPatchReached = true;
            }

            Player player = ReInput.players.GetPlayer(0);
            if (player == null)
            {
                return;
            }

            string inputSource;
            Vector2 requestedDirection = ReadRequestedDirection(player, out inputSource);

            bool hasDirectionInput = requestedDirection.magnitude > OriInputThreshold;

            if (!_keyboardEngaged)
            {
                if (!hasDirectionInput)
                {
                    return;
                }

                InitializeOriAimState(GetInitialDirection(__result));
                _keyboardEngaged = true;

                if (!_loggedFirstKeyboardInput)
                {
                    Plugin.Log?.LogInfo(
                        $"Keyboard aim input detected via {inputSource}: Horizontal={requestedDirection.x:0.###}, Vertical={requestedDirection.y:0.###}");
                    _loggedFirstKeyboardInput = true;
                }
            }

            if (_lastSimulationFrame != Time.frameCount)
            {
                _lastSimulationFrame = Time.frameCount;
                AdvanceOriAim(requestedDirection, hasDirectionInput);
            }

            __result = _currentDirection;
        }

        private static void InitializeOriAimState(Vector2 initialDirection)
        {
            _currentDirection = initialDirection.normalized;
            _keyboardAngleDegrees = DirectionToAngle(_currentDirection);
            _displayAngleDegrees = _keyboardAngleDegrees;
            _keyboardSpeed = 0f;
            _keyboardClockwise = false;
            _fixedStepAccumulator = 0f;
            _lastSimulationFrame = -1;
        }

        private static void AdvanceOriAim(Vector2 requestedDirection, bool hasDirectionInput)
        {
            // Ori DE evaluates keyboard Bash aiming from FixedUpdate. Its normal
            // physics setting is 60 Hz; unscaled time preserves that real-time
            // response while Aeterna Noctis slows gameplay to 20% during aiming.
            _fixedStepAccumulator += Time.unscaledDeltaTime;

            while (_fixedStepAccumulator >= OriFixedStepSeconds)
            {
                StepOriAim(requestedDirection, hasDirectionInput);
                _fixedStepAccumulator -= OriFixedStepSeconds;
            }
        }

        private static void StepOriAim(Vector2 requestedDirection, bool hasDirectionInput)
        {
            if (hasDirectionInput)
            {
                float targetAngle = DirectionToAngle(requestedDirection);
                float angleDelta = Mathf.DeltaAngle(_keyboardAngleDegrees, targetAngle);
                float previousDirectionSign = _keyboardClockwise ? 1f : -1f;

                if (Mathf.Sign(angleDelta) != previousDirectionSign)
                {
                    _keyboardClockwise = Mathf.Sign(angleDelta) > 0f;
                    _keyboardSpeed = 0f;
                }

                _keyboardSpeed += Mathf.Min(
                    Mathf.Abs(angleDelta),
                    OriFixedStepSeconds * OriKeyboardAcceleration);
                _keyboardAngleDegrees = Mathf.MoveTowardsAngle(
                    _keyboardAngleDegrees,
                    targetAngle,
                    _keyboardSpeed * OriFixedStepSeconds);
            }
            else
            {
                _keyboardSpeed = 0f;
            }

            _displayAngleDegrees = Mathf.LerpAngle(
                _displayAngleDegrees,
                _keyboardAngleDegrees,
                OriVisualLerpFactor);

            float angleRadians = _displayAngleDegrees * Mathf.Deg2Rad;
            _currentDirection = new Vector2(
                Mathf.Cos(angleRadians),
                Mathf.Sin(angleRadians));
        }

        private static float DirectionToAngle(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        private static Vector2 ReadRequestedDirection(Player player, out string source)
        {
            Vector2 rawBindings = ReadRawMovementBindings(player);
            if (rawBindings.sqrMagnitude > 0.0001f)
            {
                source = "raw movement binding";
                return rawBindings;
            }

            if (Plugin.EnableWasdFallback.Value)
            {
                Vector2 fallback = new Vector2(
                    (IsKeyHeld(Plugin.FallbackRightKey.Value) ? 1f : 0f)
                        - (IsKeyHeld(Plugin.FallbackLeftKey.Value) ? 1f : 0f),
                    (IsKeyHeld(Plugin.FallbackUpKey.Value) ? 1f : 0f)
                        - (IsKeyHeld(Plugin.FallbackDownKey.Value) ? 1f : 0f));

                if (fallback.sqrMagnitude > 0.0001f)
                {
                    source = "fallback key";
                    return fallback;
                }
            }

            source = "Rewired movement axis";
            return new Vector2(
                player.GetAxisRaw(HorizontalActionId),
                player.GetAxisRaw(VerticalActionId));
        }

        private static Vector2 ReadRawMovementBindings(Player player)
        {
            return new Vector2(
                ReadRawAction(player, HorizontalActionId),
                ReadRawAction(player, VerticalActionId));
        }

        private static float ReadRawAction(Player player, int actionId)
        {
            Keyboard keyboard = ReInput.controllers.Keyboard;
            if (keyboard == null)
            {
                return 0f;
            }

            float value = 0f;
            foreach (ControllerMap map in player.controllers.maps.GetAllMaps())
            {
                if (map == null || map.controllerType != ControllerType.Keyboard)
                {
                    continue;
                }

                foreach (ActionElementMap binding in map.ElementMapsWithAction(actionId, false))
                {
                    if (binding == null
                        || !binding.enabled
                        || binding.keyCode == KeyCode.None
                        || binding.hasModifiers
                        || !keyboard.GetKey(binding.keyCode))
                    {
                        continue;
                    }

                    value += binding.axisContribution == Pole.Negative ? -1f : 1f;
                }
            }

            return Mathf.Clamp(value, -1f, 1f);
        }

        private static bool IsKeyHeld(KeyCode key)
        {
            return key != KeyCode.None
                && ReInput.controllers.Keyboard != null
                && ReInput.controllers.Keyboard.GetKey(key);
        }

        private static Vector2 GetInitialDirection(Vector2 originalMouseDirection)
        {
            if (originalMouseDirection.sqrMagnitude > 0.0001f)
            {
                return originalMouseDirection.normalized;
            }

            if (KingVariables._Character != null && !KingVariables._Character.IsFacingRight)
            {
                return Vector2.left;
            }

            return Vector2.right;
        }

        internal static void Reset()
        {
            _keyboardEngaged = false;
            _keyboardClockwise = false;
            _keyboardSpeed = 0f;
            _keyboardAngleDegrees = 0f;
            _displayAngleDegrees = 0f;
            _fixedStepAccumulator = 0f;
            _currentDirection = Vector2.right;
            _lastSimulationFrame = -1;
        }

        internal static bool TryUpdateStandaloneDirection(
            Vector2 originalDirection,
            out Vector2 direction)
        {
            UseMovementBindingsForBowAim(ref originalDirection, "standalone LateUpdate");
            direction = originalDirection;
            return _keyboardEngaged;
        }

        internal static bool TryGetLatchedDirection(out Vector2 direction)
        {
            direction = _currentDirection;
            return _keyboardEngaged;
        }
    }

    [HarmonyPatch(typeof(BowController), "AimAndShoot")]
    internal static class BowAimCallPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RouteAimDirectionThroughKeyboardPatch(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo originalGetter = AccessTools.PropertyGetter(
                typeof(MouseAimingHUD),
                nameof(MouseAimingHUD.AimDirection));
            MethodInfo replacement = AccessTools.Method(
                typeof(AimDirectionPatch),
                nameof(AimDirectionPatch.GetAimDirection));
            MethodInfo controllerGetter = AccessTools.Method(
                typeof(Player),
                nameof(Player.GetAxis2DRaw),
                new[] { typeof(int), typeof(int) });
            MethodInfo controllerReplacement = AccessTools.Method(
                typeof(AimDirectionPatch),
                nameof(AimDirectionPatch.GetControllerAimDirection));

            int mouseReplacementCount = 0;
            int controllerReplacementCount = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(originalGetter))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = replacement;
                    mouseReplacementCount++;
                    yield return instruction;
                    continue;
                }

                if (instruction.Calls(controllerGetter))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = controllerReplacement;
                    controllerReplacementCount++;
                    yield return instruction;
                    continue;
                }

                yield return instruction;
            }

            if (mouseReplacementCount != 1 || controllerReplacementCount != 1)
            {
                Plugin.Log?.LogError(
                    "Expected one mouse and one controller bow-aim source; "
                    + $"routed mouse={mouseReplacementCount}, controller={controllerReplacementCount}.");
            }
            else
            {
                Plugin.Log?.LogInfo("BowController mouse and controller aim sources routed successfully.");
            }
        }
    }

    [HarmonyPatch(typeof(MouseAimingHUD), nameof(MouseAimingHUD.TurnOff))]
    internal static class AimResetPatch
    {
        [HarmonyPostfix]
        private static void ResetAfterAimingEnds()
        {
            BowController bowController = KingVariables._bowController;
            if (bowController == null || !bowController.IsAiming)
            {
                AimDirectionPatch.Reset();
            }
        }
    }

    internal static class StandaloneAimController
    {
        private static readonly FieldInfo AimField = AccessTools.Field(typeof(BowController), "_aim");
        private static readonly FieldInfo ShootPointField = AccessTools.Field(typeof(BowController), "shootPoint");
        private static readonly FieldInfo CrossFatherField = AccessTools.Field(typeof(BowController), "crossFather");
        private static readonly FieldInfo ArrowRotationField = AccessTools.Field(typeof(BowController), "_arrowRotation");
        private static readonly FieldInfo RotationShootPointZField = AccessTools.Field(typeof(BowController), "_rotationShootPointZ");

        private static bool _wasAiming;
        private static bool _loggedBowControllerFound;
        private static bool _loggedPluginLoop;
        private static bool _loggedMissingBowController;
        private static BowController _cachedBowController;
        private static int _nextLookupFrame;
        private static int _lastProcessedFrame = -1;

        internal static void UpdateFromPluginLateUpdate()
        {
            if (!_loggedPluginLoop)
            {
                Plugin.Log?.LogInfo("Standalone keyboard-aim LateUpdate loop is running.");
                _loggedPluginLoop = true;
            }

            BowController bowController = ResolveActiveBowController();
            if (bowController == null)
            {
                if (!_loggedMissingBowController && Time.frameCount >= 300)
                {
                    Plugin.Log?.LogWarning("No active BowController found after 300 frames.");
                    _loggedMissingBowController = true;
                }

                return;
            }

            UpdateAfterBowController(bowController, "plugin LateUpdate fallback");
        }

        internal static void UpdateAfterBowController(
            BowController bowController,
            string attachmentSource)
        {
            if (bowController == null || _lastProcessedFrame == Time.frameCount)
            {
                return;
            }

            _lastProcessedFrame = Time.frameCount;
            _cachedBowController = bowController;

            if (!_loggedBowControllerFound)
            {
                Plugin.Log?.LogInfo(
                    $"Standalone keyboard-aim controller attached via {attachmentSource}.");
                _loggedBowControllerFound = true;
            }

            if (!bowController.IsAiming)
            {
                EndAimIfNeeded();
                return;
            }

            _wasAiming = true;
            Vector2 originalDirection = (Vector2)AimField.GetValue(bowController);
            if (AimDirectionPatch.TryUpdateStandaloneDirection(originalDirection, out Vector2 direction))
            {
                ApplyDirection(bowController, direction);
            }
        }

        private static BowController ResolveActiveBowController()
        {
            if (_cachedBowController != null)
            {
                return _cachedBowController;
            }

            if (KingVariables._bowController != null)
            {
                _cachedBowController = KingVariables._bowController;
                return _cachedBowController;
            }

            if (Time.frameCount < _nextLookupFrame)
            {
                return null;
            }

            _nextLookupFrame = Time.frameCount + 60;
            _cachedBowController = UnityEngine.Object.FindObjectOfType<BowController>();
            return _cachedBowController;
        }

        internal static void ApplyLatchedDirection(BowController bowController)
        {
            if (bowController != null
                && AimDirectionPatch.TryGetLatchedDirection(out Vector2 direction))
            {
                ApplyDirection(bowController, direction);
            }
        }

        private static void ApplyDirection(BowController bowController, Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            direction.Normalize();
            Vector2 scaledDirection = direction * 0.5f;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            AimField.SetValue(bowController, scaledDirection);
            ArrowRotationField.SetValue(bowController, rotation);
            RotationShootPointZField.SetValue(bowController, angle);

            Transform shootPoint = (Transform)ShootPointField.GetValue(bowController);
            if (shootPoint != null)
            {
                Vector3 localPosition = shootPoint.localPosition;
                shootPoint.localPosition = new Vector3(
                    scaledDirection.x,
                    scaledDirection.y,
                    localPosition.z);
            }

            GameObject crossFather = (GameObject)CrossFatherField.GetValue(bowController);
            if (crossFather != null)
            {
                crossFather.transform.rotation = rotation;
            }
        }

        private static void EndAimIfNeeded()
        {
            if (_wasAiming)
            {
                AimDirectionPatch.Reset();
                _wasAiming = false;
            }
        }
    }

    [HarmonyPatch(typeof(BowController), nameof(BowController.Shoot))]
    internal static class ShootDirectionPatch
    {
        [HarmonyPrefix]
        private static void LockKeyboardDirectionBeforeShot(BowController __instance)
        {
            StandaloneAimController.ApplyLatchedDirection(__instance);
        }
    }

    [HarmonyPatch(typeof(BowController), "Update")]
    internal static class BowControllerUpdatePatch
    {
        [HarmonyPostfix]
        private static void ApplyKeyboardAimAfterBowUpdate(BowController __instance)
        {
            StandaloneAimController.UpdateAfterBowController(
                __instance,
                "BowController.Update postfix");
        }
    }
}
