# Blacksmith Framework

Blacksmith 是一个围绕《打铁》规则构建的可扩展对战框架。程序启动后加载运行目录中的插件 DLL，初始化内置 AI，然后在 `http://localhost:5000` 提供本地 Web 对战服务。

## 项目结构

| 项目 | 说明 |
|---|---|
| `ClapInfra` | 最底层的基础设施库。提供可扩展枚举框架 `ClapEnum<T>`、技能包反射配对机制 `ClapSkillPackage`、通用实体组件模板 `ClapBody` 和 决议缓冲区模型`ClapTurnContext`。不依赖任何 Blacksmith 项目。 |
| `BlacksmithCore` | 基于 `ClapInfra` 构建的核心引擎。包含领域模型、技能 DSL、判定引擎、动态规则、AI 策略、插件加载器。 |
| `BlacksmithClient` | 唯一的运行入口。ASP.NET Core 本地站点，托管 `wwwroot` 静态前端，暴露 `/api/*` 最小 API，通过 `WebGameSession` 组装会话与快照。 |
| `ModExamples` | 示例 Mod 源码。演示扩展枚举 + 新职业 + Common 修改器的组合写法。 |

源代码位于 `Blacksmith/` 目录下，解决方案文件为 `Blacksmith/Blacksmith.sln`。所有项目目标框架为 `net8.0`。

## 运行方式

```powershell
cd .\Blacksmith
dotnet run --project .\BlacksmithClient\BlacksmithClient.csproj
```

程序在 `http://localhost:5000` 启动并自动打开浏览器。

## 对战模式

| 模式 | 说明 |
|---|---|
| **Manual** | 双方技能均由前端手动输入，适合调试规则。 |
| **BloodSigil** | 使用 `BloodSigilStrategy`，基于规则的启发式 AI。 |
| **General** | 使用 `GeneralStrategy`，基于 MCTS 搜索的通用 AI。可通过 `data.json` 读取评分参数。 |

## 内置职业

当前核心库内置的主职业包：

- **Common** — 通用技能。
- **Cannon** — 钢炮。高物理伤害，穿甲弹可打断对手并穿透非真实防御。
- **Driver** — 驱动器。被动每回合获得真实伤减，依赖时空资源转换和爆发攻击。
- **Warlock** — 术士。魔法职业，可制造多回合延迟攻击、禁言对手时空获取，有炼金子职业。
- **BloodSigil** — 鲜血印记。以生命值为代价换取高伤害与吸血，转职时 +3 MHP/+3 HP 并移除部分基础攻击。
- **Lancer** — 战矛。纹章系统职业，命中可附加火/冰/光/暗四种纹章效果，蓄力后爆发魔法伤害。

`ModExamples/HolyBook.cs` 提供了圣书职业的部分示例实现。

## 文档导航

- [规则说明](./Documents/规则/RuleCN.md)
- [项目架构](./Documents/项目架构.md)
- [Mod 基础指南](./Documents/Mod基础指南/引言.md)
- [Mod 进阶指南](./Documents/Mod进阶指南/引言.md)

## 扩展机制概览

Blacksmith 的扩展体系是"启动期装配型插件架构"：

1. 程序启动时 `PluginLoader` 扫描运行目录全部 `.dll`
2. 反射发现 `BlacksmithEnum<T>` 子类、`[IsBlacksmithEnumModifier]` 静态类、`MainProfession` / `ProfessionModifier` 子类
3. 注册扩展枚举成员、职业名和职业修改器
4. 调用 `ClapEnum.CloseFactory()` 关闭枚举工厂

之后进入运行期，所有装配结果直接可用。不支持运行中热插拔。

核心扩展点：
- **可扩展枚举**：`ResourceType`、`DefenseType`、`AttackType`、`EffectType`、`EffectTargetType`、`DynamicJudgeRuleName`、`JudgeStage`
- **技能包**：`MainProfession`（主职业）和 `ProfessionModifier`（挂到已有职业上的技能补丁）
- **防御类型**：继承 `DefenseBase` 实现新的防御行为
- **AI 策略**：实现 `IAIStrategy` 接口
