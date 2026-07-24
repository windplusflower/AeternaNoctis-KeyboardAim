# Aeterna Noctis Keyboard Aim

[English](README.md)

这是一个面向《Aeterna Noctis》的 BepInEx 5 辅助功能 Mod，为弓箭时停瞄准补充
完整的键盘方向控制。

进入弓箭的时停瞄准后，可以使用当前移动键平滑旋转瞄准方向。操作方式类似《奥日》
猛击的键盘方向调整：方向会连续转动，不再只能水平左右或吸附到八个固定方向。

> 这是非官方社区 Mod，与 Aeternum Game Studios 无关。

## 功能

- 使用游戏当前设置的水平、垂直移动键。
- 游戏在瞄准时屏蔽移动动作的情况下，直接读取备用 `W`、`A`、`S`、`D`。
- 平滑转向，不会吸附到固定方向。
- 松开移动键后保持当前角度。
- 每次拉弓期间，只有按下键盘方向后才由键盘接管，不改变原有鼠标和手柄操作。
- 固定转速为每秒 60°：按原版无延长能力时的 1.5 秒瞄准窗口计算，全程可转 90°。
- 获得延长时停能力后，转速仍保持每秒 60°，不会自动变化。

## 兼容性

- Windows x64
- Steam 版《Aeterna Noctis》
- 已在 Aeterna Noctis Ultimate Edition v3.3.001 上测试
- 使用 BepInEx 5.4.23.5 构建

其他游戏版本可能也能使用，但尚未验证。

## 下载

打开 [最新 GitHub Release](https://github.com/windplusflower/AeternaNoctis-KeyboardAim/releases/latest)，
选择以下文件之一：

- **FullPack**：推荐普通玩家使用，已经包含官方 Windows x64 版
  BepInEx 5.4.23.5，不需要另外安装 BepInEx。
- **ModOnly**：适合已经安装 BepInEx 5 的玩家。

## 安装

### FullPack（推荐）

1. 退出游戏。
2. 从最新 Release 下载
   `AeternaNoctis-KeyboardAim-v1.0.8-FullPack.zip`。
3. 在 Steam 中右键《Aeterna Noctis》，选择
   **管理 > 浏览本地文件**。
4. 把 ZIP 内的全部内容直接解压到包含 `Aeterna Noctis.exe` 的游戏根目录。
5. 启动游戏。

解压后应当存在：

```text
Aeterna Noctis\BepInEx\plugins\AeternaKeyboardAim.dll
```

首次安装 BepInEx 后，第一次启动可能稍慢，因为它需要创建目录和配置文件。

### ModOnly

1. 确认 BepInEx 5 已经安装，并且至少启动过一次。
2. 下载 `AeternaNoctis-KeyboardAim-v1.0.8-ModOnly.zip`。
3. 将它解压到游戏目录；也可以单独把 `AeternaKeyboardAim.dll` 复制到：

```text
Aeterna Noctis\BepInEx\plugins\
```

## 操作方式

1. 像原版一样按住弓箭键，进入时停瞄准。
2. 按住或短按移动方向键，旋转瞄准方向。
3. 松开移动键会保持当前角度。
4. 松开弓箭键发射。

Mod 会跟随重新绑定后的移动键。直接读取的备用按键默认为 `W`、`A`、`S`、`D`，
可以在配置文件中修改。

## 配置

首次启动后会生成：

```text
BepInEx\config\cn.codex.aeternanoctis.keyboardaim.cfg
```

可用设置：

| 设置 | 默认值 | 说明 |
| --- | ---: | --- |
| `RotationSpeedDegreesPerSecond` | `60` | 键盘瞄准的固定转速，单位为度/秒。 |
| `InputDeadZone` | `0.25` | 键盘接管瞄准所需的最小移动输入强度。 |
| `EnableFallbackKeys` | `true` | 游戏屏蔽移动动作时，直接读取备用按键。 |
| `Up`、`Down`、`Left`、`Right` | `W`、`S`、`A`、`D` | 直接读取的备用方向键。 |

## 卸载

删除：

```text
BepInEx\plugins\AeternaKeyboardAim.dll
```

也可以删除生成的配置文件：

```text
BepInEx\config\cn.codex.aeternanoctis.keyboardaim.cfg
```

如果其他 Mod 也在使用 BepInEx，请不要删除整个 `BepInEx` 文件夹。

## 故障排查

- 确认 ZIP 解压到了 `Aeterna Noctis.exe` 所在目录，而不是多套了一层文件夹。
- 打开 `BepInEx\LogOutput.log`，搜索 `Aeterna Noctis Keyboard Aim`。
- 确认使用的是 Windows x64 版 BepInEx 5。
- 反馈问题时，请附上 `BepInEx\LogOutput.log` 并说明游戏版本。

## 从源码构建

项目需要引用本机的游戏程序集，这些文件不会上传到仓库。先在游戏目录安装
BepInEx 5，然后执行：

```powershell
dotnet build -c Release -p:GameDir="D:\Path\To\Aeterna Noctis"
```

也可以先设置环境变量 `AETERNA_NOCTIS_GAME_DIR`，再执行构建。

版本记录见 [CHANGELOG.md](CHANGELOG.md)，完整包内 BepInEx 的再发布说明见
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。
