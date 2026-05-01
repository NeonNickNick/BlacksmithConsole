# DSL与规则链接
[返回](./引言.md)

本篇专门解释一个关键方法：

```csharp
SourceFile.LinkJudgeRuleDynamic(ruleKey, mutations)
```

如果说普通 DSL 是"写这个技能本身做什么"，那么 `LinkJudgeRuleDynamic` 就是在说：

这个技能除了当前效果外，还要把一些动态规则挂到判定系统里。

## 这个方法做了什么

在当前实现里，`LinkJudgeRuleDynamic` 会把一组动态规则挂到 `SourceFile` 的内部缓存上：

```csharp
_mutationsOnCompile[ruleKey] = mutations;
```

真正注册发生在 `Compile(judger)` 阶段：

```csharp
judger.JudgeRuleManager.RegistJudgeRuleDynamic(pair.Key, pair.Value);
judger.JudgeRuleManager.AddJudgeRule(_owner, pair.Key);
```

也就是说完整过程是：

- 技能书写阶段：先把"将来要注册什么规则"记下来
- 编译阶段：把模板注册到池里
- 同时按当前施法者专门化，并加入本回合判定链

## 典型调用格式

```csharp
Pen pen = sf => sf
    .UseResource(1, ResourceType.Instance.Iron())
    .LinkJudgeRuleDynamic(
        DynamicJudgeRuleName.Instance.Charge(),
        new List<Mutation>
        {
            // 在这里填写 Mutation
        });

return DSL.Create(sc.Self, pen);
```

## `Mutation` 应该怎么写

最典型的写法是：

```csharp
new Mutation(
    judgeRule: (player, enemy) =>
    {
        // 这里写某个阶段要插入的逻辑
    },
    stage: JudgeStage.Instance.OnAttackCanceling(),
    ruleType: RuleType.Modifier,
    modifierOrder: ModifierOrder.Before,
    remainingRounds: 1,
    delayRounds: 0)
```

字段含义：

- `judgeRule`：该规则实际执行的内容，签名为 `Action<Community, Community>`
- `stage`：挂到哪个判定阶段
- `ruleType`：是覆盖还是修饰
- `modifierOrder`：在核心规则前还是后执行
- `remainingRounds`：规则持续多久
- `delayRounds`：多久以后开始生效

## 规则里可以做什么

在 `judgeRule` 中，你通常会操作：

- `player.Focus.Get<TurnContext>().Get<AttackResolution>()` 等 Resolution 列表
- `enemy.Focus.Get<TurnContext>().Get<EffectResolution>()` 等
- `player.Focus.Get<Health>()` 等组件
- 以及通过 DSL 再临时生成一个技能片段并执行

例如：

```csharp
new Mutation(
    (player, enemy) =>
    {
        if (enemy.Focus.Get<TurnContext>().Get<AttackResolution>()
            .Find(a => a.DelayRounds == 0) == null)
        {
            return;
        }

        DSL.Create(player, sf => sf
            .WriteAttack(10, AttackType.Instance.Magical()))
            .Compile()
            .Execute(player);
    },
    JudgeStage.Instance.OnAttackCanceling(),
    RuleType.Modifier,
    ModifierOrder.Before)
```

这段规则的意思是：

- 当进入 `OnAttackCanceling` 阶段
- 如果对手当前回合有即时攻击
- 我方就立刻插入一段额外攻击

## 一个完整而实用的模式

下面给出一个"下回合开始时检查状态，并在特定阶段触发反击"的结构模板：

```csharp
.LinkJudgeRuleDynamic(
    DynamicJudgeRuleName.Instance.Charge(),
    new List<Mutation>
    {
        new Mutation(
            (player, enemy) =>
            {
                // 触发规则：本阶段立刻生效
            },
            JudgeStage.Instance.OnAttackCanceling(),
            RuleType.Modifier,
            ModifierOrder.Before),

        new Mutation(
            (player, enemy) =>
            {
                // 下回合开始时的清理/重置逻辑
            },
            JudgeStage.Instance.OnBegin(),
            RuleType.Modifier,
            ModifierOrder.Before,
            delayRounds: 1)
    })
```

这是当前项目里最常见的高级技能写法之一。

## 为什么不是直接在技能里写死

因为有些效果不是"当前技能一放就立刻结算完"，而是：

- 等到某个阶段才判断
- 依赖对手这一回合有没有做某件事
- 要持续到下一回合
- 要插在默认规则之前或之后

这种效果如果直接写在普通 DSL 里，会很快失控；放到 `Mutation` 里则非常自然。

## `Lancer.Charge()` 的阅读方式

如果你想快速理解真实项目里的高级写法，建议直接读：

- `Blacksmith/BlacksmithCore/Backend/SkillPackages/BuitinProfessions/Lancer.cs`

阅读顺序建议是：

1. 先看 `ChargeCheck`
2. 再看 `Charge`
3. 最后看 `AttackCanceling_Modifier_Before`

## 编写时的建议

1. `judgeRule` 里尽量只写阶段相关逻辑，不要把整套技能重新实现一遍。
2. 如果你只是要制造一个即时攻击，推荐在规则里再调用一次简短的 DSL。
3. 如果规则依赖技能类的字段状态，务必控制好字段重置时机。
4. 优先先写最小可运行版本，再补持续回合和延迟回合。
