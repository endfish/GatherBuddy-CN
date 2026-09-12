# 第三方来源与声明

GatherBuddy 的原作者为 Ottermandias，使用 Apache License 2.0；完整许可证见仓库根目录的 `LICENSE`。

`GatherBuddy/Gui/CnWidgets` 中标注 “Adapted from Ottermandias/OtterGui” 的控件基于 OtterGui 提交 `79771ee5f3d463f02c63bebbedaa0aff49e59718`，使用 Apache License 2.0。本分支将其命名空间、显示文案与部分调用点调整为中文适配，保留原有算法及来源注释。

`CnWidgets/Table.cs` 通过 Dalamud 提供的 ImGui 绑定，局部接管表格菜单；不修改 ImGui 全局文本，也不修改 OtterGui 子模块。GatherBuddy-CN 的其余修改沿用 Apache License 2.0。
