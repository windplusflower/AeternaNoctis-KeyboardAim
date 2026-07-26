# Aeterna Noctis Keyboard Aim

[简体中文](README.zh-CN.md)

A BepInEx 5 accessibility and quality-of-life mod that adds smooth, fully
keyboard-controlled bow aiming to **Aeterna Noctis**.

While the bow's slow-motion aiming is active, use your movement bindings to
rotate the aim in any direction. Version 1.2.0 implements the keyboard Bash
response from Ori and the Blind Forest: Definitive Edition's normal 60 Hz
physics mode: keys select a target direction, taps nudge the arrow, and a held
direction converges smoothly before stopping on the target angle.

Arrows with dedicated mechanisms also get matching aim assist: light arrows
target light switches, dark arrows target dark platforms, true-sight arrows
target true-sight platforms, and frost arrows target frost platforms. Enemies
are never candidates. The default acquisition cone is 30 degrees, the lock is
held up to 45 degrees, the maximum range is 32 world units, and platforms block
target acquisition.

> This is an unofficial community mod and is not affiliated with Aeternum Game
> Studios.

## Features

- Uses the game's current horizontal and vertical movement bindings.
- Includes direct `W`, `A`, `S`, `D` fallback keys for cases where the game
  suppresses movement actions during bow aiming.
- Reproduces Ori's keyboard Bash acceleration, reversal reset, and displayed
  arrow-angle interpolation.
- Treats the pressed direction as a target angle rather than a fixed angular
  velocity, without snapping instantly.
- A tap moves the aim a short distance; holding converges on the pressed
  direction.
- Keeps the selected angle when the movement keys are released.
- Leaves mouse and controller aiming unchanged until a keyboard direction is
  pressed for the current shot.
- Searches only for dedicated mechanisms matching the currently selected arrow;
  enemies are never candidates.
- Blood arrows have no dedicated mechanism, and teleport arrows use ordinary
  terrain as destinations, so neither auto-locks.

## Compatibility

- Windows x64
- Steam version of Aeterna Noctis
- Tested with Aeterna Noctis Ultimate Edition v3.3.001
- Built for BepInEx 5.4.23.5

Other game versions may work, but have not been verified.

## Download

Open the [latest GitHub Release](https://github.com/windplusflower/AeternaNoctis-KeyboardAim/releases/latest)
and choose one of these files:

- **FullPack** — recommended for most players. Includes the official Windows
  x64 build of BepInEx 5.4.23.5, so BepInEx does not need to be installed
  separately.
- **ModOnly** — for players who already have BepInEx 5 installed.

## Installation

### FullPack — recommended

1. Exit the game.
2. Download `AeternaNoctis-KeyboardAim-v1.2.0-FullPack.zip` from the latest
   Release.
3. In Steam, right-click **Aeterna Noctis** and select
   **Manage > Browse local files**.
4. Extract the contents of the ZIP directly into the folder containing
   `Aeterna Noctis.exe`.
5. Start the game.

After extraction, this file should exist:

```text
Aeterna Noctis\BepInEx\plugins\AeternaKeyboardAim.dll
```

The first launch after installing BepInEx may take a little longer while it
creates its folders and configuration files.

### ModOnly

1. Make sure BepInEx 5 is already installed and has been launched at least once.
2. Download `AeternaNoctis-KeyboardAim-v1.2.0-ModOnly.zip`.
3. Extract it into the game folder, or copy `AeternaKeyboardAim.dll` to:

```text
Aeterna Noctis\BepInEx\plugins\
```

## Controls

1. Hold your normal bow button to enter slow-motion aiming.
2. Hold or tap your movement keys to rotate the aiming direction.
3. Release the movement keys to keep the current angle.
4. Release the bow button to fire.

The mod follows remapped movement controls. Its direct fallback keys default to
`W`, `A`, `S`, and `D` and can be changed in the configuration file.

## Configuration

After the first launch, edit:

```text
BepInEx\config\cn.codex.aeternanoctis.keyboardaim.cfg
```

Available settings:

| Setting | Default | Description |
| --- | ---: | --- |
| `EnableFallbackKeys` | `true` | Reads the fallback keys directly if the game suppresses its movement actions. |
| `Up`, `Down`, `Left`, `Right` | `W`, `S`, `A`, `D` | Direct fallback keys. |
| `Enabled` | `true` | Enables arrow-specific mechanism aim assist. |
| `MaxAngleDegrees` | `30` | Maximum angle between the aim and a matching mechanism. |
| `MaxDistance` | `32` | Maximum world-space assist distance. |

### The in-game "Auto Aim" setting

The setting is stored as `HaveBowCrosshair`. It only toggles the minion's
visible aiming laser/guide; it does not change the firing direction or search
for targets.

The game's actual target selection comes from the `CrosshairBob` item.
`BowController.TryAutoAim()` searches for the nearest visible living enemy when
the current aim vector is zero. This mod replaces that enemy search during
keyboard aiming, so even with the item equipped only a dedicated mechanism
matching the current arrow can be selected.

To preserve Ori's response, version 1.1.0 uses the original curve and input
threshold constants rather than exposing a speed control. Existing
`RotationSpeedDegreesPerSecond` and `InputDeadZone` entries may remain in an
older configuration file, but 1.1.0 no longer reads them.

## Uninstallation

Delete:

```text
BepInEx\plugins\AeternaKeyboardAim.dll
```

Optionally delete the generated configuration file:

```text
BepInEx\config\cn.codex.aeternanoctis.keyboardaim.cfg
```

If BepInEx is also used by other mods, do not remove the entire `BepInEx`
folder.

## Troubleshooting

- Confirm that the ZIP was extracted beside `Aeterna Noctis.exe`, not into an
  extra subfolder.
- Open `BepInEx\LogOutput.log` and search for
  `Aeterna Noctis Keyboard Aim`.
- Make sure the Windows x64 build of BepInEx 5 is being used.
- When reporting a problem, attach `BepInEx\LogOutput.log` and state the game
  version.

## Building from source

The project references local game assemblies, which are not included in this
repository. Install BepInEx 5 into the game folder, then build with:

```powershell
dotnet build -c Release -p:GameDir="D:\Path\To\Aeterna Noctis"
```

Alternatively, set the `AETERNA_NOCTIS_GAME_DIR` environment variable before
building.

See [CHANGELOG.md](CHANGELOG.md) for version history and
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) for the BepInEx redistribution
notice.
