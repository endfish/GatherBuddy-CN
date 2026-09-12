# 测试与游戏内验收

## 自动化检查

```powershell
dotnet run --project tests/GatherBuddy.CN.Tests
```

检查中文资源 JSON、重复键、缺失翻译、占位符、格式化、ImGui 标签、枚举显示、消息模板精确迁移，以及预设导入导出的版本、枚举与自定义内容。

若提供本机游戏的 `game/sqpack` 目录，会追加国服数据初始化、中文名称与搜索、缺失记录、钓鱼消息及钓场索引检查：

```powershell
dotnet run --project tests/GatherBuddy.CN.Tests -- '你的游戏目录/game/sqpack'
```

可选的独立 ImGui 检查需要将当前 Dalamud 对应的 `cimgui.dll` 复制到测试程序输出目录，然后运行：

```powershell
dotnet run --project tests/GatherBuddy.CN.Tests -- --native-ui
```

它在独立进程中新建 ImGui 上下文，检查原生标签 ID 及中文列菜单的绘制，不连接游戏，不代表游戏内布局验收。

首轮验证环境：Dalamud API 15、国服客户端 `2026.09.01.0000.0000`。本地数据初始化包含 1,592 个采集物、2,758 种鱼类、573 个钓场、21 条海钓航线。抛竿、钓场发现及以小钓大文案分别对照 LogMessage 1110、1115、1121，未知钓场名称对照 PlaceName 950；获得力、鉴别力使用 BaseParam 72、73。

鱼名来自国服 Item 表。新版本标题参考国服官方 [7.3](https://actff1.web.sdo.com/project/20240927dawntrail/patch73/index.html)、[7.4](https://actff1.web.sdo.com/project/20240927dawntrail/patch74/index.html)、[7.5](https://actff1.web.sdo.com/project/20240927dawntrail/patch75/index.html) 专题。

## 游戏内待验收

以下项目尚未通过实际角色操作确认。请在停用原版后使用本地测试包验收，并记录客户端版本、复现步骤及日志。

- [ ] 首次加载、停用后重载、退出游戏后重新加载，配置和窗口状态正常。
- [ ] 采集物、鱼类、鱼饵及地点的中文精确、包含、模糊搜索。
- [ ] 地图标记、地点右键菜单、以太之光传送和装备套装切换。
- [ ] 天气预报、天气变化条件、闹钟触发及聊天消息模板。
- [ ] 采集分组的新建、复制、导入、导出、删除和默认预设恢复。
- [ ] 采集窗口和自定义地点的编辑、筛选、快捷键与拖放。
- [ ] 普通抛竿、咬钩、各类提钩、以小钓大、未知钓场与新钓场发现。
- [ ] 海钓航线、幻海流、昼夜条件、钓鱼计时与刺鱼辅助。
- [ ] 钓鱼记录、统计图、文字报告、JSON／TSV 导出与记录迁移。
- [ ] 中文字体、长提示、多档 UI 缩放、窗口宽度及表头截断。
- [ ] 表格右键菜单：列宽、列顺序、列隐藏及最后一列保护。
- [ ] 现有 IPC 调用方和原版配置兼容，自定义模板与预设名称未被覆盖。

上述实测完成后，再决定正式发布和接入分发仓库。
