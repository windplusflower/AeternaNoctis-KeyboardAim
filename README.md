# Aeterna Noctis Keyboard Aim

[简体中文](README.zh-CN.md)

A BepInEx 5 accessibility and quality-of-life mod that adds smooth, fully
keyboard-controlled bow aiming to **Aeterna Noctis**.

While the bow's slow-motion aiming is active, use your movement bindings to
rotate the aim in any direction. The result feels similar to adjusting Bash
direction with a keyboard in Ori: movement keys rotate continuously instead of
snapping to only horizontal or eight fixed directions.

> This is an unofficial community mod and is not affiliated with Aeternum Game
> Studios.

## Features

- Uses the game's current horizontal and vertical movement bindings.
- Includes direct `W`, `A`, `S`, `D` fallback keys for cases where the game
  suppresses movement actions during bow aiming.
- Smoothly rotates toward the pressed direction instead of snapping.
- Keeps the selected angle when the movement keys are released.
- Leaves mouse and controller aiming unchanged until a keyboard direction is
  pressed for the current shot.
- Uses a fixed rotation speed of 60 degrees per second: 90 degrees during the
  base 1.5-second slow-motion window.
- Does not speed up or slow down when bullet-time extension abilities are
  acquired.

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
2. Download `AeternaNoctis-KeyboardAim-v1.0.8-FullPack.zip` from the latest
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
2. Download `AeternaNoctis-KeyboardAim-v1.0.8-ModOnly.zip`.
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
| `RotationSpeedDegreesPerSecond` | `60` | Fixed keyboard aiming rotation speed in degrees per second. |
| `InputDeadZone` | `0.25` | Minimum movement-input magnitude before keyboard aiming takes control. |
| `EnableFallbackKeys` | `true` | Reads the fallback keys directly if the game suppresses its movement actions. |
| `Up`, `Down`, `Left`, `Right` | `W`, `S`, `A`, `D` | Direct fallback keys. |

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
