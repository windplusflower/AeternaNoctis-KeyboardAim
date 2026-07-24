# Changelog

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
