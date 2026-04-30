# 职业Mod
[返回](./引言.md)

本文档面向希望添加新职业、扩展已有职业技能，或者编写职业获得方式的开发者。

当前项目支持：

- 添加新的主职业
- 给内置职业添加或覆盖技能
- 给 Mod 职业添加或覆盖技能
- 通过修改 `Common` 职业，为玩家提供“获得新职业”的入口技能

如果你需要理解动态规则、阶段判定和 `Mutation`，请阅读 [Mod进阶指南](../Mod进阶指南/引言.md)。

## 总体流程

1. 创建一个 `.NET 8` 类库项目。
2. 引用 `BlacksmithCore.csproj`，或者引用编译后的 `BlacksmithCore.dll`。
3. 编写一个 `MainProfession` 或 `ProfessionModifier`。
4. 把编译产物放到游戏可执行文件所在目录。
5. 启动游戏，程序会自动扫描并加载该 DLL。

## 核心类型

### `ISkillContext`

技能检查函数和技能实现函数都使用 `ISkillContext`：

```csharp
public interface ISkillContext
{
    string SkillName { get; }
    ActorSet Self { get; }
    int Param { get; }
}
```

你最常用的是：

- `sc.Self`：当前施放技能的一方
- `sc.Self.Focus`：当前施放者的主体 `Body`
- `sc.Param`：技能参数

### `ActorSet` 与 `Body`

当前游戏主要使用 `ActorSet.Focus` 作为主角色。常见访问方式：

```csharp
var self = sc.Self;
var body = sc.Self.Focus;
```

常用信息包括：

- `body.Health.HP`
- `body.Health.MHP`
- `body.Resource.Check(...)`
- `body.Resource.Query(...)`（查询某种资源）
- `body.Resource.QueryAll(...)`（查询某种资源的普通 + Gold 总和）
- `body.Skill.AddPackage(...)`
- `body.Skill.AddSkill(...)`
- `body.Skill.RemoveSkill(...)`

注意：

- 生命值修改是通过 `body.Health.GainHP(...)` / `LoseHP(...)` 等完成的。
- 不是 `Body.LoseHP(...)`，而是 `Body.Health.LoseHP(...)`。

## 技能是如何被识别的

职业包和修改包都继承自 `SkillPackageBase`（其底层反射配对来自 `ClapInfra.ClapProfession.ClapSkillPackage`）。系统会通过反射自动收集：

- 名为 `XxxCheck` 的私有实例方法，签名必须是 `bool (ISkillContext)`
- 名为 `Xxx` 的私有实例方法，签名必须是 `DSL.SourceFile (ISkillContext)`

两者配对后，技能名会被自动转换为小写，例如：

- `HolyBookCheck` + `HolyBook` -> 技能名 `holybook`
- `SpaceAttackCheck` + `SpaceAttack` -> 技能名 `spaceattack`

因此：

- 技能方法必须是实例方法
- 必须使用 `private`
- 技能方法名与检查方法名必须严格对应

## DSL 基础用法

推荐先写上这两个别名：

```csharp
using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
using DSL = DSLforSkillLogic;
```

标准写法通常是：

```csharp
private DSL.SourceFile SomeSkill(ISkillContext sc)
{
    Pen pen = sf => sf
        .UseResource(1, ResourceType.Instance.Iron())
        .WriteAttack(3, AttackType.Instance.Physical());

    return DSL.Create(sc.Self, pen);
}
```

常用 DSL 语句：

- `WriteAttack(power, attackType, APFactor = 1, delayRounds = 0)`
- `WriteDefense(power, defense, delayRounds = 0)`
- `WriteResource(power, resourceType, delayRounds = 0)`
- `WriteRecovery(power)`
- `WriteEffect(...)`
- `WriteFree(action)`
- `UseResource(need, resourceType, ifCommonOnly = false)`
- `WithBloodSuck(percent)`
- `WithInterupt()`

说明：

- `WithBloodSuck` 和 `WithInterupt` 都是接在最近一条攻击后面的攻击修辞，只修饰前一条攻击。
- `AttackType`、`ResourceType` 等都应通过 `Instance` 访问，例如 `AttackType.Instance.Physical()`。

## 示例一：编写一个新职业

下面是一个最小主职业示例：

```csharp
using BlacksmithCore.Backend.Backend.SkillPackages.Logic;
using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.SkillPackages.Core;
using BlacksmithCore.Backend.SkillPackages.Logic;

namespace Example.Mod;

using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
using DSL = DSLforSkillLogic;

public class MyProfession : MainProfession
{
    private bool JokeCheck(ISkillContext sc)
    {
        return sc.Self.Focus.Health.HP > 5
            && sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 1);
    }

    private DSL.SourceFile Joke(ISkillContext sc)
    {
        Pen pen = sf => sf
            .UseResource(1, ResourceType.Instance.Iron())
            .WriteFree(source =>
            {
                source.Focus.Health.LoseHP(1);
                source.Focus.Health.LoseMHP(1);
            })
            .WriteRecovery(1)
            .WriteAttack(3, AttackType.Instance.Physical())
            .WriteAttack(3, AttackType.Instance.Magical())
                .WithBloodSuck(0.5f);

        return DSL.Create(sc.Self, pen);
    }
}
```

要点：

- 新职业继承 `MainProfession`
- 职业名默认就是类名 `MyProfession`
- 被动技能可选，重写 `PassiveSkill(ISkillContext sc)` 即可

## 示例二：让 `Common` 提供一个“转职技能”

如果你只写了 `MainProfession`，游戏并不会自动给玩家一个获得该职业的技能。常见做法是给 `Common` 写一个修改器：

```csharp
using BlacksmithCore.Backend.Backend.SkillPackages.Logic;
using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.SkillPackages.Core;
using BlacksmithCore.Backend.SkillPackages.Logic;
using BlacksmithCore.Backend.SkillPackages.Logic.BuitinProfessions;
using BlacksmithCore.Infra.Attributes;

namespace Example.Mod;

using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
using DSL = DSLforSkillLogic;

[IsProfessionModifier(nameof(Common))]
public class CommonModifier : ProfessionModifier
{
    private bool MyProfessionCheck(ISkillContext sc)
    {
        return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 2);
    }

    private DSL.SourceFile MyProfession(ISkillContext sc)
    {
        sc.Self.Focus.Skill.AddPackage(new MyProfession());

        Pen pen = sf => sf
            .UseResource(2, ResourceType.Instance.Iron())
            .WriteFree(source =>
            {
                Common.ExcludeAllProfessions(source);
            });

        return DSL.Create(sc.Self, pen);
    }
}
```

这里有两个关键点：

- `[IsProfessionModifier(nameof(Common))]` 表示这个修改器要挂到 `Common` 包上。
- `Common.ExcludeAllProfessions(source)` 用于保证同一局里只保留一个主职业入口。

## 修改已有职业技能

如果你想给现有职业增加技能或覆盖技能：

- 继承 `ProfessionModifier`
- 用 `[IsProfessionModifier("目标职业名")]` 指向目标职业
- 在修改器里写新的 `SkillCheck` / `Skill` 方法

当前加载逻辑会把修改器中的技能追加到目标职业包；如果技能名重复，对应的检查器和生成器会被覆盖。

## 关于被动技能

- `MainProfession` 可以重写 `PassiveSkill`。
- `ProfessionModifier` 的主要职责是给目标包补技能，不应把它当成独立职业来设计。
- 如果你想写一个职业自己的被动，请优先写在 `MainProfession` 中。

## 与当前代码对齐的注意事项

1. `AttackType` 里法术攻击名是 `Magical()`，不是 `Magic()`。
2. `Body` 上没有直接的 `LoseHP` / `GainHP`；应通过 `body.Health` 调用。
3. 资源和攻击类型应使用 `Instance` 获取，例如 `ResourceType.Instance.Iron()`。
4. 技能名最终会转成全小写，手动调用 `AddSkill/RemoveSkill` 时也要使用全小写技能名。
5. `Common` 是一个真实职业包，修改 `Common` 是提供转职入口最常见的方式。

## 参考示例

仓库中的示例 Mod 位于：

- `Blacksmith/ModExamples/HolyBook.cs`
- `Blacksmith/ModExamples/CommonModifier.cs`
- `Blacksmith/ModExamples/EnumExtension.cs`

它展示了：

- 如何新增职业
- 如何为 `Common` 添加转职入口
- 如何扩展资源与防御枚举

## 温馨提示

1. 多个 Mod 如果声明了同名技能，后写入字典的实现会覆盖前者；实际覆盖顺序取决于 DLL 扫描顺序。
2. 如果两个 Mod 定义了同名职业，程序会抛出异常。
3. 职业 Mod 往往会同时依赖枚举扩展与 DSL，建议先读完基础指南，再进入进阶指南。
