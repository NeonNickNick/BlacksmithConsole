# Mod 基础指南

本文档面向拿到本项目编译后 DLL（或源码）并希望通过内部提供API添加新职业，并且不关心底层实现的开发者。说明如何实现一个职业插件（Class Library），如何使用项目提供的 DSL（用于生成技能逻辑），以及一些重要的 API 与约定。

## 总体流程

- 创建一个 .NET 类库项目（目标 .NET 8），添加对游戏主程序导出的程序集的引用（通常需要引用 Blacksmith 的主程序集或包含公共类型的 DLL）。

- 在插件中，每个扩展职业仅需创建继承自 Blacksmith.Backend.SkillPackages.Core.SkillPackageBase 的类并重写抽象方法。

- 必须写无参数构造函数并且必须在构造函数中调用父类方法 InitializeSkills()，并添加若干成对的私有静态方法来定义技能（Check + 生成器）。

- 将编译产物（插件 DLL）放到游戏可执行文件所在目录或程序约定的插件目录；程序启动时会自动扫描并加载插件。

## 技能（非常重要）
强烈建议在文件中使用
```csharp
using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
using DSL = DSLforSkillLogic;
```
以保持语义清晰。

其次必须写无参数构造函数并且必须在构造函数中调用父类方法 
```csharp
SkillPackageBase.InitializeSkills()
```
- 对于每个技能，请添加一对私有静态方法
```csharp
static bool YouSkillNameCheck(ISkillContext sc)
```
  - 返回 true 表示当前上下文允许使用此技能（例如检测资源足够、冷却等）。
```csharp
static DSLforSkillLogic.SourceFile YouSkillName(ISkillContext sc)
```
  - 方法内部构建并返回 DSL.SourceFile 描述技能的行为。

InitializeSkills 会通过反射把满足以上签名和命名规则的一对方法注册为技能。这就是为什么调用它非常重要。

## 关于 ISkillContext

在现有实现中，ISkillContext 至少提供：
- sc.Self: 当前使用技能的一方（ActorSet）
- sc.Param: 技能的参数（若技能需要参数，如多段攻击的次数）

（插件中可以直接按此约定访问 sc.Self / sc.Param；如果需要其它信息，请查看或引用项目中 ISkillContext 的真实定义）

## DSL 快速参考（在插件中使用 DSLforSkillLogic）

### 创建 DSL.SourceFile 的标准方式：
- return DSL.Create(sc.Self, pen);
- 其中 pen 是一个组合了若干句子的委托，例如：
```csharp
.WriteAttack(power, AttackType, APFactor = 1, delayRounds = 0)//攻击
.WriteDefense(power, DefenseBase defense, delayRounds = 0)//防御
.WriteResource(power, ResourceType, delayRounds = 0)//资源获取
.WriteEffect(EffectType, List<EffectTag>, EffectTargetType, power, duration, action)//效果（建议谨慎使用）
.WriteRecovery(power)//恢复
.WriteFree(action) //自由语句
.UseResource(need, ResourceType, ifCommonOnly = false)//使用资源
.LinkJudgeRule("ruleKey") //链接规则变动
```

### 额外的 DSL 用法示例：
- 修辞（rhetoric）机制允许在上一句基础上添加附加逻辑（例如在攻击产生的 AttackResolution 上附加 OnEnd 的吸血）：参考已有实现中的 BloodSuck。

### Judge 规则（可链接规则）

- 有些技能需要在判定阶段加入额外规则（例如转移、反射等）。在 DSL 中使用 LinkJudgeRule("key")，在技能被编译并且传入 Judger 时，JudgeRuleManager 会调用 AddJudgeRule(_owner, key)。
- 项目内部通过 JudgeRulePool 等实现具体的规则（例如内置了 "reflect"）。插件可以复用已有的 ruleKey 或与作者约定新 key 并在运行时代码中把规则注册到 Judger。

## 插件示例（最小骨架）

这是一个最小的职业插件示例：
```csharp
namespace Blacksmith.ProfessionPlugin{
  using DSL = Blacksmith.Backend.SkillPackages.Logic.DSLforSkillLogic;
  using Pen = Func<DSL.SourceFile, DSL.SourceFile>;
  public class MyProfession : SkillPackageBase
  {
      public override string Name => "myprofession";

      public MyProfession()
      {
          InitializeSkills(); // 必须
      }

      public override DSL.SourceFile PassiveSkill(ISkillContext sc)
      {
          return new DSL.SourceFile(sc.Self);
      }

      private static bool bloomCheck(ISkillContext sc)
      {
          // 检查资源 / 条件
          return true;
      }

      private static DSL.SourceFile bloom(ISkillContext sc)
      {
          Pen pen = sf => sf
              .UseResource(1, ResourceType.Iron)
              .WriteAttack(3, AttackType.Physical);//链式调用
          return DSL.Create(sc.Self, pen);
      }
  }
}
```
## 打包与发布

- 在插件项目的 csproj 中设置 TargetFramework 为 net8.0。
- 引用本项目编译产物（或把需要的公共接口项目也作为 NuGet / 项目引用），以便访问 SkillPackageBase、DSLforSkillLogic、ISkillContext 等类型。
- 将生成的 DLL 放到主程序可扫描的目录（例如可执行文件同目录），程序在启动时会自动加载并注册。
- 确保职业 Name 唯一，否则 ProfessionRegistry.Regist 会打印冲突信息并拒绝重复添加。

## 调试建议

- 在插件中把核心逻辑写成纯静态方法（Check 与 生成器均为私有静态），调用 InitializeSkills 后游戏将自动可见。
- 使用控制台输出（Console.WriteLine）在插件加载和技能执行时打印信息，方便追踪。

## 附：常见类型参考（只作为快速查阅）

- 常用枚举/类型（出现在 DSL 与技能实现中）：
  - ResourceType（Iron / Magic / GoldIron / ...）
  - AttackType（Physical / Real / ...）
  - EffectType、EffectTag、EffectTargetType
  - DefenseBase（以及具体实现 RealReduction、CommonReduction 等）
- 若需更精确契约，请在插件项目中引用并查看 Blacksmith 源码中的这些类型定义。

## 结束语

这个文档提供了将职业以插件形式扩展到游戏中的快速说明。插件只要满足命名与签名约定，并通过 InitializeSkills 注册技能，即可被主程序发现并注册为新职业。有关更深层的改动，例如编写一些全新的需要接触到底层才能实现的技能请见文档[Mod 进阶指南](./ModAdvanced.md)
