# DSL 与规则链接

本章详细介绍如何通过 DSL 将动态判定规则（Dynamic Judge Rules）链接到技能中：LinkJudgeRuleDynamic 的作用、调用链、以及如何在技能中编写可注册到 DynamicJudgeRulePool 的 Mutation 模板。最后给出多个实际例子（包含工程内 Lancer 的 Charge 技能示例解析）。

注意：本章假设你已熟悉基础 DSL 的语句（WriteAttack、WriteDefense、WriteFree 等），以及 SourceFile 的基本使用方式。

## LinkJudgeRuleDynamic 的职责

DSL 中的 SourceFile 提供了一个方法：

SourceFile.LinkJudgeRuleDynamic(DynamicJudgeRuleName.BEValue ruleKey, List<Mutation> mutations)

它执行两件事：

1. 把 ruleKey 记录到 SourceFile._mutationsOnCompile 列表，以便当 SourceFile 被 Compile 并交给 Judger 编译时，Judger 能在 Compile 阶段调用 JudgeRuleManager.AddJudgeRule，最终把这些动态规则加入 JudgeRuleManager。简单说：把 "我在这个技能里需要这些动态规则" 的意图记下来。

2. 把 mutations 注册到 DynamicJudgeRulePool（DynamicJudgeRulePool.RegistDynamic），将这些传入的 Mutation 模板转换为内部的 MutationPrototype 并保存到全局池中。

RegistDynamic 的实现会把每条 Mutation 的 JudgeRule（Action<ActorSet, ActorSet>）包装为一个 prototype（Action<ActorSet source, ActorSet player, ActorSet enemy>），以便在 Query 时专化（Specialize）成真正绑定到某个施法者的 Mutation（Action<ActorSet, ActorSet>）。

因此 LinkJudgeRuleDynamic 实际上完成了 "模板注册"（push templates into pool）与 "技能声明"（在编译时告诉 Judger: 我要把对应规则加入裁判管理器）的工作。

## 从 DSL 调用到实际执行的完整调用链

1. 在技能实现中调用 SourceFile.LinkJudgeRuleDynamic(key, mutations)。
2. LinkJudgeRuleDynamic 把 key 记录到 _mutationsOnCompile，并把 mutations 注册到全局 DynamicJudgeRulePool。
3. 当技能编译为 Intent（通常由 DSLforSkillLogic.SourceFile.Compile 调用并传入 Judger 实例时），Compile 会把 SourceFile._mutationsOnCompile 中的每个 key 调用 judger.JudgeRuleManager.AddJudgeRule(_owner, key)。
4. JudgeRuleManager.AddJudgeRule 会调用 DynamicJudgeRulePool.Query(_owner, key)，Query 会根据 _owner 专化模板并返回一份真实的 Mutation 列表。
5. AddJudgeRule 把这些 Mutation 转换为 StageRuleContainer.RuleUnit 并插入到相应阶段的容器（Override / ModifiersBefore / ModifiersAfter）。
6. 在 Judger 真正执行回合判定时，会构建每个阶段的执行流（StageRuleContainer.Build），并在合适的回合执行这些动态规则。

## 如何编写 Mutation 模板（在技能内）

Mutation 构造函数签名为：

Mutation(
    Action<ActorSet, ActorSet> judgeRule,
    JudgeStage stage,
    RuleType ruleType,
    ModifierOrder modifierOrder,
    int remainingRounds = 1,
    int delayRounds = 0)

在 LinkJudgeRuleDynamic 中传入的 judgeRule 是一个 Action<ActorSet, ActorSet>，注意：这是 "模板形式"，在 DynamicJudgeRulePool.RegistDynamic 内部会被包装为三参数原型（source, player, enemy）。

最佳实践：在 judgeRule 中不要直接依赖技能类的成员变量（除非你清楚模板被包装后如何调用），而应通过传入的 ActorSet 参数读取或写入角色状态。模板中可以写：

var m = new Mutation((player, enemy) => {
    // 使用 player / enemy 来操作 TurnContext 或写入 Resolution
    player.Focus.TurnContext.AttackResolutions.Add(...)
}, JudgeStage.OnAttackCanceling, RuleType.Modifier, ModifierOrder.Before, remainingRounds: 1, delayRounds: 0);

当此模板被 RegistDynamic 后，DynamicJudgeRulePool 内部会把它封装为一个接受 source (施法者), player, enemy 三参数的 prototype。专化时（Query(source, name)）会把 source "记住" 并返回一个 Mutation，其中 JudgeRule 是一个只接受 (player, enemy) 的 Action，内部使用 IfElseUtil(source, player, enemy, prototypeJudgeRule) 来映射哪些参数代表实际的发起方。

## 示例：Lancer 的 Charge（解析）

Lancer.cs 中的 Charge 方法使用了 LinkJudgeRuleDynamic 来实现复杂的 Charge 行为：

- Charge 在使用时：
  - Consume 资源
  - 通过 WriteFree 增加 _chargeCount 并把 _chargeCost 设为 0
  - LinkJudgeRuleDynamic(DynamicJudgeRuleName.Instance.Charge(), new() { ... })：注册两个 Mutation 模板：
    1. 一个在 OnAttackCanceling 阶段的 Modifier Before，其 JudgeRule 指向类方法 AttackCanceling_Modifier_Before（该方法会在实际判定执行时被调用，从而实现被动触发的自动反击逻辑）；
    2. 一个在 OnBegin 阶段的 Modifier Before（delayRounds:1），其作用是在下一回合的开始检查 _chargeCount 与 _ifPassive 来决定是否重置 _chargeCount 和 _chargeCost（并通过闭包访问 _chargeCount 变量）。

- 链路回顾：
  - Charge 调用 LinkJudgeRuleDynamic，把两个模板注册到池并在 SourceFile._mutationsOnCompile 记下 Charge 的 key。
  - 当技能被 Compile 并传入 Judger 时，Compile 会调用 judger.JudgeRuleManager.AddJudgeRule(_owner, ChargeKey)。
  - AddJudgeRule 会让 DynamicJudgeRulePool 专化模板（内部会把 source 记住），并把两个 Mutation 按 Stage 放入 JudgeRuleManager 的容器。
  - 在实际的回合判定流程中，当 Judger 执行到 OnAttackCanceling 阶段时，如果此时满足 Delay/Remaining 条件，JudgeRuleManager 会在执行 CancelAttacks 之前先调用注册的 Before Modifier（即 AttackCanceling_Modifier_Before），而该方法会生成一次即时的攻击 Resolution 并把它注入到玩家的 TurnContext，从而实现 Charge 的被动“自动反击”效果。

下面给出一个简化伪代码示例以帮助理解：

```csharp
// 在技能中（简化）
var chargeTemplates = new List<Mutation>() {
    new Mutation((player, enemy) => AttackCancelingModifier(player, enemy), JudgeStage.OnAttackCanceling, RuleType.Modifier, ModifierOrder.Before),
    new Mutation((player, enemy) => {
        if (/* charge 未被消耗 */) resetCharge();
    }, JudgeStage.OnBegin, RuleType.Modifier, ModifierOrder.Before, delayRounds: 1)
};
sourceFile.LinkJudgeRuleDynamic(DynamicJudgeRuleName.Instance.Charge(), chargeTemplates);

// RegistDynamic 会把上面的模板包装为 prototype，并存入池
// Compile -> JudgeRuleManager.AddJudgeRule 会调用 pool.Query(source, Charge) 返回专化后的 Mutation
// JudgeRuleManager 将 Mutation 注入容器，在后续阶段执行
```

## 例子：自定义技能注册一个“延时反击”规则

需求：创建一个技能，使用后在下一回合的 OnAttackCanceling 阶段，如果敌方存在当回合攻击，则自动发动一次固定伤害的反击。

实现要点：
1. 在技能中使用 LinkJudgeRuleDynamic 注册一个 JudgeStage.OnAttackCanceling 的 Modifier Before，一条 JudgeStage.OnBegin 的延时检查用以清理或回滚状态（可选）。
2. 在 Mutation 的 judgeRule 中添加实际的逻辑，例如遍历 enemy.Focus.TurnContext.AttackResolutions 并判断其中是否存在 DelayRounds==0 的攻击分辨，如果存在则创建并注入你希望的反击 AttackResolution。

示例代码片段（DSL 内部的 WriteFree 中或直接在 LinkJudgeRuleDynamic 注册的 mutation 中使用）：

var m = new Mutation((player, enemy) => {
    if (enemy.Focus.TurnContext.AttackResolutions.Find(a => a.DelayRounds == 0) != null)
    {
        var res = new AttackResolution { Source = player, DelayRounds = 0, Type = AttackType.Instance.Magical(), Power = 10 };
        res.Execute = (Body target) => { target.Health.LoseHP((int)res.Power); };
        player.Focus.TurnContext.WriteResolution(res);
    }
}, JudgeStage.OnAttackCanceling, RuleType.Modifier, ModifierOrder.Before);

// 然后通过 LinkJudgeRuleDynamic 注册
sourceFile.LinkJudgeRuleDynamic(MyDynamicName, new() { m });

## 小结与安全建议

- LinkJudgeRuleDynamic 是把“模板规则”与“技能声明”连接起来的桥梁。模板在 pool 中保存，真正成为执行规则是在 Compile + AddJudgeRule 阶段由 pool 专化并注入 JudgeRuleManager。
- 在编写 Mutation 时要注意闭包与状态访问：如果 JudgeRule 依赖技能类的私有字段（如 Lancer 的 _chargeCount），模板在注册时被包装为 prototype 并在专化时使用 source 参数来构造真正的 Action；因此闭包仍然可以访问技能实例的状态（因为注册时是在技能实例方法执行期间），但要注意并发/生命周期问题。
- 尽量通过 ActorSet（player, enemy）与 TurnContext 操作来改变回合内行为，而不是直接在 JudgeRule 内大量修改全局状态。这样更符合系统设计并能避免难以追踪的副作用。

更多高级范例可参考项目内的 Lancer.cs、Driver.cs 等实现。