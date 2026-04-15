# Mod基础指南
本文档面向拿到本项目编译后DLL或源码并希望通过内部提供API添加新职业，并且不关心底层实现的开发者。说明如何实现一个Mod，如何使用项目提供的DSL以及一些重要的 API 与约定。

目前Mod支持的功能具体包括：添加新职业、给内置职业添加新技能或覆盖其技能（无法修改被动技能，如驱动器时之盾），给Mod职业添加新技能或覆盖其技能（无法修改被动技能）。

## 总体流程
- 创建一个.NET 8类库项目，添加对游戏主程序导出的程序集Blacksmith.dll的引用。

- 在项目中按照下面的步骤编写若干类。

- 将编译产物放到游戏可执行文件所在目录，程序启动时会自动扫描并加载Mod。

## APIs
### 关于 ISkillContext
这是一个技能上下文接口，验证玩家资源数量等均从此进入。在现有实现中，ISkillContext 提供：
- sc.Self: 当前使用技能的一方（ActorSet实例）
- sc.Param: 技能的参数（若技能需要参数，如恢复2）

（Mod中可以直接按此约定访问 sc.Self / sc.Param；如果需要其它信息，请查看或引用项目中 ISkillContext 的真实定义）

### 关于Body和ActorSet
玩家核心类为Body类。它包装在ActorSet类中备用（为幻书预留），当前可以只考虑直接访问，即sc.Self.Focus，即可获得玩家Body类
可以通过Body类来查询玩家信息，重要入口如下：
- ActorSet Body.Community 玩家所属的一方
- int Body.Health.HP 生命值
- int Body.Health.MHP 最大生命值
- void Body.LoseHP(int)
- void Body.LoseMHP(int)
- void Body.GainHP(int)
- void Body.GainMHP(int)

- bool Body.Resource.Check(ResourceType, float, bool = false)//该bool参数用于控制是否只检查普通资源，例如只检查普通铁。即第一个参数类型为金铁只要bool参数设置正确就能正确工作
- float Body.Resource.QueryCommon(ResourceType)//查询普通资源数量。传入金铁也能正确工作，但是不建议
- float Body.Resource.QueryGold(ResourceType)//查询金资源数量。传入铁也能正常工作。对于其他资源，不存在金类型，不建议调用这个方法，因为必定返回0f

- void Body.Skill.AddPackage(ISkillPackage)//无需关心这个接口，直接new一个职业类进去即可添加职业
- void AddSkill(string packageName, string skillName)//使这个技能变得可用。请务必检查拼写正确且技能名全小写，包名与实际保持一致。传错什么都不会发生
- void RemoveSkill(string packageName, string skillName)//使这个技能禁用。请务必检查拼写正确且技能名全小写，包名与实际保持一致。传错什么都不会发生

### DSL简介
提供了一套链式DSL屏蔽底层实现，具有易懂的参数，不需要特别注释，可以用类似自然语言的方式编写技能。建议使用的编写格式为类似Python的缩进形式

总共有两种语句，第一种即最常见的攻击、防御、资源获取等。第二种为第一种中的特定语句所专用，例如吸血，只能跟在攻击的后面。

建议在开始编写前先加上
```csharp
using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
using DSL = DSLforSkillLogic;
/*伪代码：
  Pen pen = ...
  return DSL.Create(sc.Self, pen);
*/
```
建议一个技能只有这两句。return DSL.Create(sc.Self, pen)是返回的标准形式。接下来主要看前面。
pen是一个组合了若干句子的委托，后面直接根据技能效果依次写语句即可：
```csharp
/*伪代码
Pen pen = sf => sf

.WriteAttack(power, AttackType, APFactor = 1, delayRounds = 0)//攻击
.WriteDefense(power, DefenseBase defense, delayRounds = 0)//防御，注意这里需要手动构造防御类传进去。可以参考内置包
.WriteResource(power, ResourceType, delayRounds = 0)//资源获取
.WriteEffect(EffectType, List<EffectTag>, EffectTargetType, power, duration, action)//效果（建议谨慎使用）
.WriteRecovery(power)//恢复
.WriteFree(action) //自由语句，提供了与底层实现交互的方式
.UseResource(need, ResourceType, ifCommonOnly = false)//使用资源
.BloodSuck(percent)//百分比吸血
.LinkJudgeRule("ruleKey") //链接规则变动，不建议使用。打铁作为一个规则对称的游戏，有一些技能必须通过使规则不在对称才能实现。目前该功能尚不完善，唯一可用的方式使LinkJudgeRule("reflection")，效果即技能“转移”。

*/
```
接下来是一个详细的例子。
## 编写Mod实例
以保持一致性。

主程序集提供了反射方法来自动获取技能名和技能方法，因此只需要按照约定实现抽象类，编写技能方法即可方便地添加新职业。

接下来假设来编写一个职业，装备它需要消耗1个铁，它只有一个技能“Joke”：生命值大于5时，消耗1个铁和1点HP和1点MHP，获取一个铁，恢复1点生命值，造成3点物理伤害和3点法术伤害。其中，法术伤害具有50%吸血效果。

```csharp
namespace Example.Mod{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class MyProfession : SkillPackageBase{//必须继承这个抽象类
        public string Name => "myprofession";//实现属性
        /*被动技能是可选的
        public override DSL.SourceFile PassiveSkill(ISkillContext sc){
            //此处即被动技能逻辑
        }
        */
        public MyProfession(){//必须无参数
            InitializeSkills();//重要，这个父类方法会反射得到技能表，如果不调用的话无法使用该职业技能
            //此处可添加若干逻辑，例如warlock包具有逻辑
            /*
            AvailableSkillNames.Remove("midastouch");
            */
        }
        //必须为实例方法
        private bool JokeCheck(ISkillContext sc){//方法名必须为$"{技能名}Check"，必须为bool(ISkillContext)。该方法作用在于检查技能是否能够使用
            return sc.Self.Focus.Health.HP > 5 && sc.Self.Focus.Resource.Check(ResourceType.Iron, 1);
        }
        //必须为实例方法
        private DSL.SourceFile Joke(ISkillContext sc){//方法名必须为$"技能名"，必须为DSL.SourceFile(ISkillContext)。该方法即技能逻辑，建议使用提供的DSL编写
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Iron)
                .WriteFree(source =>
                    {
                        source.Focus.Health.LoseHP(1);
                        source.Focus.Health.LoseMHP(1);
                    })
                .WriteRecovery(1)
                .WriteAttack(3, AttackType.Physical)
                .WriteAttack(3, AttackType.Magic)
                    .BloodSuck(0.5f);//注意必须跟在要吸血的攻击后面。这个专用语句不会影响前面的物理攻击
            return DSL.Create(sc.Self, pen);
        }
    }
}
```
在完成这个职业后还不够，我们还要让通用技能包知道原来可以添加这个新职业。因此必须给通用技能包写一个扩展技能

```csharp
namespace Example.Mod{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class CommonExtensionForMyProfession : SkillPackageBase{//必须继承这个抽象类
        public string Name => "common";//通用包名字。事实上，如果想给其它包写技能可以直接改成其他包的名字
        /*被动技能即使写了也无效
        public override DSL.SourceFile PassiveSkill(ISkillContext sc){
            //此处即被动技能逻辑
        }
        */
        public CommonExtensionForMyProfession(){//必须无参数
            Type = PackageType.Modifier;//重要，需要主动生命这是一个修改器而不是一个主包
            InitializeSkills();//重要，如果不调用主程序就不知道还有这个扩展
            //此处可添加若干逻辑，例如如果扩展技能也要维护一个内部状态
        }
        //必须为实例方法
        private bool MyProfessionCheck(ISkillContext sc){//方法名必须为$"{技能名}Check"，必须为bool(ISkillContext)
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1);
        }
        //必须为实例方法
        private DSL.SourceFile MyProfession(ISkillContext sc){//方法名必须为$"技能名"，必须为DSL.SourceFile(ISkillContext)
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Iron)
                .WriteFree(source => 
                { 
                    Professions.ForEach(p => source.Focus.Skill.RemoveSkill("common", p));//重要，因为每局游戏只能有一个职业。这条代码复制即可
                    source.Focus.Skill.AddPackage(new Warlock());//重要，否则根本没添加新职业技能。这条代码复制即可
                });
            return DSL.Create(sc.Self, pen);
        }
    }
}
```
技能方法和技能校验方法必须为实例方法。对于与技能包状态无关的工具方法建议设为静态，与状态有关的方法名需要避开“Check”与所有可能的技能名，建议给这种方法名加特殊前缀防止偶然重名。

接下来打包即可。

## 温馨提示
该方案不保证多个Mod能够兼容。如果两个Mod都修改了同一个技能，那么后被主程序加载的会生效。可以通过调整文件名来实现控制生效。有关更高级的Mod编写，例如自由语句，规则链接的完整功能请见文档[Mod 进阶指南](./ModAdvanced.md)
