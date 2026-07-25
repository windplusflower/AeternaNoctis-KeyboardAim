# Aeterna Noctis Keyboard Aim

[English](README.md)

这是一个面向《Aeterna Noctis》的 BepInEx 5 辅助功能 Mod，为弓箭时停瞄准补充
完整的键盘方向控制。

进入弓箭的时停瞄准后，可以使用当前移动键平滑旋转瞄准方向。1.1.0 版按
《奥日与黑暗森林：终极版》普通 60 Hz 物理模式的键盘猛击公式实现：方向键给出
目标方向，短按微调，长按平滑收敛，到达目标角后停止。

> 这是非官方社区 Mod，与 Aeternum Game Studios 无关。

## 功能

- 使用游戏当前设置的水平、垂直移动键。
- 游戏在瞄准时屏蔽移动动作的情况下，直接读取备用 `W`、`A`、`S`、`D`。
- 复现《奥日》键盘猛击的加速转向、反向重置速度和箭头角度插值。
- 方向键指定目标角，而不是控制固定角速度；不会瞬间吸附。
- 短按只转动一小段，长按会逐渐对准所按的方向。
- 松开移动键后保持当前角度。
- 每次拉弓期间，只有按下键盘方向后才由键盘接管，不改变原有鼠标和手柄操作。

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
   `AeternaNoctis-KeyboardAim-v1.1.0-FullPack.zip`。
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
2. 下载 `AeternaNoctis-KeyboardAim-v1.1.0-ModOnly.zip`。
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
| `EnableFallbackKeys` | `true` | 游戏屏蔽移动动作时，直接读取备用按键。 |
| `Up`、`Down`、`Left`、`Right` | `W`、`S`、`A`、`D` | 直接读取的备用方向键。 |

为了保持与《奥日》一致，转向曲线和输入阈值从 1.1.0 起使用原作常数，不再允许
单独调速。旧配置文件里的 `RotationSpeedDegreesPerSecond` 和 `InputDeadZone`
可能仍然存在，但 1.1.0 不再读取它们。

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
