# Blacksmith Framework

Blacksmith 是一个围绕《打铁》规则构建的可扩展对战框架。当前仓库已经收敛为一个本地 Web 宿主：程序启动后会加载运行目录中的插件 DLL，初始化内置 AI，然后在 `http://localhost:5000` 提供静态前端和最小 API。

## 当前仓库结构

- `Blacksmith/BlacksmithClient`
  当前唯一宿主项目。负责启动 ASP.NET Core 本地站点、暴露 `/api/*` 接口、托管 `wwwroot` 前端资源，并包含 `WebGameSession` 会话门面。
- `Blacksmith/BlacksmithCore`
  游戏核心库。包含判定引擎、技能 DSL、职业包系统、AI 策略、插件加载器。领域对象通过 `GetView()` 方法向 Web 层输出只读快照数据。
- `Blacksmith/ModExamples`
  示例 Mod 源码目录，展示“扩展枚举 + 新职业 + Common 修改器”的组合写法。
- `Documents`
  中文规则、Mod 指南和项目架构文档。

## 运行方式

1. 进入解决方案目录：

   ```powershell
   cd .\Blacksmith
   ```

2. 启动本地宿主：

   ```powershell
   dotnet run --project .\BlacksmithClient\BlacksmithClient.csproj
   ```

3. 程序会在 `http://localhost:5000` 启动，并尝试自动打开浏览器。

## 当前模式

- `Manual`
  双方技能都由前端手动输入，适合调试规则。
- `BloodSigil`
  使用 `BloodSigilStrategy`。
- `General`
  使用 `GeneralStrategy`，若存在 `data.json` 会读取其中的参数。

## 内置内容概览

当前核心库内置的职业包主要包括：

- `Common`
- `Warlock`
- `Cannon`
- `Driver`
- `BloodSigil`
- `Lancer`

`holybook` 相关内容目前位于 `Blacksmith/ModExamples`，它是示例 Mod，而不是 `BlacksmithCore` 的内置职业。

## 文档导航

- [规则说明](./Documents/规则/RuleCN.md)
- [Mod 基础指南](./Documents/Mod基础指南/引言.md)
- [Mod 进阶指南](./Documents/Mod进阶指南/引言.md)
- [项目架构](./Documents/项目架构.md)

## 现状说明

- 插件加载入口在 `BlacksmithClient/Program.cs`，会先扫描运行目录中的 `.dll`，再注册扩展枚举和职业包。
- 当前前后端不是分离的两个可执行项目，而是 `BlacksmithClient` 单独承载前端静态资源和后端 API。
- 所有项目目标框架均为 `net8.0`。
- 前端快照由各领域对象的 `GetView()` 方法生成，`WebGameSession` 直接组装为 API 响应，不再通过独立的 `WebGameReader` 类。
- `Blacksmith/ModExamples` 目录已经并入当前解决方案；如果你把它当成自己的起点，建议优先参考文档中的”当前命名空间”写法，而不是直接照抄历史代码。
