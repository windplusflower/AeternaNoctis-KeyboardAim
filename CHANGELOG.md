# Changelog

## 1.1.0

- Replaced fixed-speed rotation with the Ori and the Blind Forest: Definitive
  Edition keyboard Bash aiming response.
- Matched the normal 60 Hz update cadence, 0.2 digital-input threshold,
  2000-degree acceleration term, direction-reversal speed reset, and 0.5
  displayed-angle interpolation.
- Movement keys now select a target direction: taps nudge the aim, holding
  converges on that direction, and releasing preserves the selected angle.
- Removed the rotation-speed and dead-zone options so stale settings cannot
  change the Ori response. Older config entries are ignored.

## 1.0.8

- Set keyboard aiming to a fixed 60 degrees per second.
- A base 1.5-second slow-motion window now allows exactly 90 degrees of
  continuous rotation.
- Bullet-time extension abilities no longer change the rotation speed.
- Prevented duplicate per-frame rotation when more than one compatibility path
  reaches the aiming code.

## 1.0.7

- Added aim-window-based speed calculation.
- Prevented duplicate rotation updates in the same frame.

## 1.0.6

- Kept Harmony patches active when the game's bootstrap scene destroys the
  original BepInEx plugin host object.

## 1.0.5

- Added a direct `BowController.Update` compatibility path.
- Applied the latched keyboard direction again immediately before firing.

## 1.0.3

- Routed both the mouse/keyboard and controller bow-aim branches through the
  keyboard aiming handler.
