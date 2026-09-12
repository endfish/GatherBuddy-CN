# 维护与上游同步

`main` 是唯一长期维护分支。`origin` 指向个人仓库，`upstream` 指向 Ottermandias/GatherBuddy，仅用于抓取和比较。中文分支的问题与功能在本仓库维护，不向上游自动创建 PR。

```powershell
git remote add upstream https://github.com/Ottermandias/GatherBuddy.git
git remote set-url --push upstream DISABLED
git fetch upstream
git rev-list --left-right --count main...upstream/main
git log --oneline main..upstream/main
git diff main...upstream/main -- GatherBuddy GatherBuddy.GameData .gitmodules
```

完成比较后，从 `main` 建立临时 `sync/upstream-版本或日期` 分支进行合并，检查子模块指针，再执行 `git submodule update --init --recursive`。不要直接把 OtterGui 更新到其最新提交。

上游同步、兼容修复、本地化基础设施、中文资源、自有功能与版本元数据分别提交。完成检查后合回 `main`，测试通过再推送个人仓库。自有功能从 `feature/*` 开发，通过独立模块与小范围接入点集成。

## 本地化约定

- `GatherBuddy.GameData/Localization/Localize.cs` 由插件和 GameData 共用，默认加载内嵌 `zh-CN.json`；英文源文案作为缺失翻译的回退。
- 固定文本使用 `Localize.Text`，整句插值使用 `Localize.Format`。不要对动态拼接结果或用户内容做翻译查询。
- ImGui 控件使用 `Localize.Label`／`FormatLabel`：保留已有显式 `###` ID，其他可见标签以英文源文案派生稳定 ID。纯隐藏 `##` 标签直接保留。表格等持久化标识不翻译。
- 枚举只在显示时使用 `Localize.Display`，有歧义的名称使用 `enum.类型.成员` 资源键。配置键、枚举成员及数值、IPC 通道和导入导出格式不改名。
- 在 `zh-CN.json` 中保留所有占位符、必要换行和空格；`##`／`###` 留在代码中。新增语句必须补充中文资源。
- 内置预设说明可翻译；已有预设名称、用户备注、导入文本和自定义消息模板不覆盖。
- 术语优先对照国服数据表；钓鱼消息正则必须有真实客户端消息依据和测试样例。
- 控件适配集中在 `GatherBuddy/Gui/CnWidgets`。升级 OtterGui 或 ImGui 后，复查复制的控件及原生表格菜单适配。

扫描器同时检查字段缓存和界面调用。允许保留的命令、文件名、格式、品牌、底层异常和协议标识逐项记录在 `tests/GatherBuddy.CN.Tests/localization-allowlist.json`；新增例外必须写明原因。解析器、旧记录迁移、日志、TSV 导出和生成源码的调试函数属于协议或诊断用途，按测试中的边界保留原文。

## 版本与交付

当前基线：上游 `3.8.11.1` / `1e39592f55e57774f287bfcea87dbd651a5cf2d8`，中文正式版 `3.8.11.3`。
每次升级需递增中文版本，并更新项目中的 `UpstreamVersion`／`UpstreamCommit` 元数据及说明。

推送前完成 Debug、Release、语言及兼容测试，核对 ZIP 的 `GatherBuddy.dll`、`GatherBuddy.json` 和依赖。保持 `InternalName` 与所有 `GatherBuddy.*` IPC（包括上游既有拼写）不变。正式发布由维护者明确要求后执行：从 main 构建 Release，创建版本标签与 GitHub Release，并同步 DalamudPlugins 的 `GatherBuddy-CN/latest.zip` 和 `repo.json` 条目；普通源码维护不自动触发发布。

仓库不得包含个人配置、客户端数据、凭据、机器路径、临时调研材料或本机协作提示词。
