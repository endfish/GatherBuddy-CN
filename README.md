# GatherBuddy-CN

面向国服的 GatherBuddy 简体中文分支，由 Endfish 独立维护，持续同步 [Ottermandias/GatherBuddy](https://github.com/Ottermandias/GatherBuddy)。

当前测试版本 **3.8.11.2**，基于上游 **3.8.11.1 / [1e39592](https://github.com/Ottermandias/GatherBuddy/commit/1e39592f55e57774f287bfcea87dbd651a5cf2d8)**，使用 .NET 10、Dalamud API 15。

## 功能

- 查询采集物、鱼类、鱼饵、采集点及钓场，支持中文精确、包含和模糊搜索。
- 查看采集时间、天气、天气变化和海钓航线，配置闹钟及采集分组。
- 标记地图位置、传送至以太之光、切换采集职业装备套装。
- 提供钓鱼计时浮窗、刺鱼辅助、钓鱼记录与统计。
- 汉化界面、提示、菜单、筛选器、命令帮助及默认消息模板；游戏名称读取本机客户端数据。

本轮保持上游功能范围。底层日志、命令参数、协议标识和用户自定义内容保持原样。

## 安装本地测试版

首轮只提供源码与本地测试包，完成游戏内验收后再接入分发仓库。

1. 在插件列表中停用原版 GatherBuddy。
2. 将测试 ZIP 的所有文件解压至一个独立目录，保留 DLL 之间的相对位置。
3. 打开 `/xlsettings` → `Experimental`，将解压目录中的 `GatherBuddy.dll` 添加为开发插件。
4. 在 `/xlplugins` 的开发插件列表中启用 GatherBuddy-CN，输入 `/gatherbuddy` 打开界面。

显示名为 GatherBuddy-CN，`InternalName`、程序集及配置身份仍为 `GatherBuddy`。因此**中文版与原版不能同时启用**，两者会使用同一套既有配置。测试前可备份 GatherBuddy 配置和记录。

第一次使用时，在设置中填写游戏内实际的园艺工、采矿工、捕鱼人装备套装名称。既有英文默认套装名和预设标识保留，以兼容已有命令与配置。

## 常用命令

| 命令 | 用途 |
| --- | --- |
| `/gatherbuddy` | 打开主界面 |
| `/gather 火之碎晶` | 查询物品并按设置执行地图标记、传送及换装 |
| `/gatherbtn 物品名` | 只查询园艺采集点 |
| `/gathermin 物品名` | 只查询采矿采集点 |
| `/gatherfish 鱼名` | 查询钓场 |
| `/gathergroup` | 显示采集分组命令帮助 |
| `/gbc` | 显示快捷开关帮助 |

`alarm`、`next` 等参数沿用上游拼写。设置中的消息模板保留 `{Item}`、`{Input}`、`{Alarm}`、`{Offset}`、`{DelayString}`、`{Location}` 标记。
升级时仅把与上游默认值完全相同的消息模板转换为中文；空模板、自定义模板、预设名称与备注不覆盖。

## 构建与验收

需要 .NET 10 SDK 和国服 Dalamud API 15 开发文件。首次克隆后初始化子模块：

```powershell
git submodule update --init --recursive
$env:DALAMUD_HOME = Join-Path $env:APPDATA 'XIVLauncherCN\addon\Hooks\dev'
dotnet build GatherBuddy/GatherBuddy.csproj -c Debug
dotnet build GatherBuddy/GatherBuddy.csproj -c Release
dotnet run --project tests/GatherBuddy.CN.Tests
```

构建结果位于 `GatherBuddy/bin/Debug` 或 `GatherBuddy/bin/Release`。中文资源内嵌于 `GatherBuddy.GameData.dll`，无需单独复制语言文件。

开发与同步说明见 [CONTRIBUTING.md](CONTRIBUTING.md)，自动化检查及游戏内验收清单见 [TESTING.md](TESTING.md)。

## 来源与许可证

保留上游作者 Ottermandias 的署名、历史及 [Apache-2.0 许可证](LICENSE)。OtterGui 子模块固定在上游记录的提交；插件内的控件适配注明来源及基线，不要求额外的依赖 fork。第三方声明见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。
