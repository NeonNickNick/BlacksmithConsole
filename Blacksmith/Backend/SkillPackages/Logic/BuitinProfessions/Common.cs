using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;
using Blacksmith.Backend.SkillPackages.Core;
using Blacksmith.Infra.ExtensibleProfession;

namespace Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public class Common : MainProfession
    {
        private static List<string> Professions => ProfessionRegistry.Professions;

        private bool IronCheck(ISkillContext sc) => true;
        private DSL.SourceFile Iron(ISkillContext sc)
        {
            Pen pen = sf => sf.WriteResource(1, ResourceType.Instance.Iron());
            return DSL.Create(sc.Self, pen);
        }
        
        private bool StickCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 0.5f);
        }
        private DSL.SourceFile Stick(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteAttack(1, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool DrillCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 1.5f);
        }
        private DSL.SourceFile Drill(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1.5f, ResourceType.Instance.Iron())
                .WriteAttack(3, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool SlashCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 2.5f);
        }
        private DSL.SourceFile Slash(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Instance.Iron())
                .WriteAttack(5, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
        
        private bool ShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), sc.Param * 0.5f);
        }
        private DSL.SourceFile Shield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param * 0.5f, ResourceType.Instance.Iron())
                .WriteDefense(2 + sc.Param, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }

        private bool ThornShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 1 + sc.Param * 0.5f);
        }
        private DSL.SourceFile ThornShield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param * 0.5f, ResourceType.Instance.Iron())
                .WriteDefense(4 + sc.Param, new ThornReduction());
            return DSL.Create(sc.Self, pen);
        }

        private bool RecoveryCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 1 + sc.Param);
        }
        private DSL.SourceFile Recovery(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param, ResourceType.Instance.Iron())
                .WriteRecovery(2 + 2 * sc.Param);
            return DSL.Create(sc.Self, pen);
        }
        
        private bool SpaceCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 3);
        }
        private DSL.SourceFile Space(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Space());
            return DSL.Create(sc.Self, pen);
        }

        private bool TimeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 3);
        }
        private DSL.SourceFile Time(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Time());
            return DSL.Create(sc.Self, pen);
        }

        private bool TearCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Space(), 1f);
        }
        private DSL.SourceFile Tear(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Space())
                .WriteAttack(8, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
        private bool ReflectCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Space(), 2f);
        }
        private DSL.SourceFile Reflect(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Space())
                .LinkJudgeRuleDynamic(
                    DynamicJudgeRuleName.Instance.Reflect(),
                    new()
                    {
                        new(
                            ReflectRule.EffectSwaping_Modifier_After,
                            JudgeStage.Instance.OnEffectSwaping(),
                            RuleType.Modifier,
                            ModifierOrder.After),
                        new(
                            ReflectRule.AttackSwaping_Modifier_After,
                            JudgeStage.Instance.OnAttackSwaping(),
                            RuleType.Modifier,
                            ModifierOrder.After)
                    });
            return DSL.Create(sc.Self, pen);
        }

        private bool WarlockCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 1f);
        }
        private DSL.SourceFile Warlock(ISkillContext sc)
        {
            sc.Self.Focus.Skill.AddPackage(new Warlock());
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }

        private bool CannonCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 4);
        }
        private DSL.SourceFile Cannon(ISkillContext sc)
        {
            sc.Self.Focus.Skill.AddPackage(new Cannon());
            Pen pen = sf => sf
                .UseResource(4, ResourceType.Instance.Iron())
                .WriteDefense(3, new CommonReduction())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }

        private bool DriverCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 3);
        }
        private DSL.SourceFile Driver(ISkillContext sc)
        {
            sc.Self.Focus.Skill.AddPackage(new Driver());
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }

        private bool BloodSigilCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 7);
        }
        private DSL.SourceFile BloodSigil(ISkillContext sc)
        {
            sc.Self.Focus.Skill.AddPackage(new BloodSigil());
            Pen pen = sf => sf
                .UseResource(7, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                    List<string> addition = new()
                    {
                        "stick",
                        "drill",
                        "slash",
                        "tear"
                    };
                    addition.ForEach(a => source.Focus.Skill.RemoveSkill(nameof(Common), a));
                    source.Focus.Health.GainMHP(3);
                    source.Focus.Health.GainHP(3);
                });
            return DSL.Create(sc.Self, pen);
        }
        private bool LancerCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 4);
        }
        private DSL.SourceFile Lancer(ISkillContext sc)
        {
            sc.Self.Focus.Skill.AddPackage(new Lancer());
            Pen pen = sf => sf
                .UseResource(4, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }
        public static void ExcludeAllProfessions(ActorSet source)
        {

            Professions.ForEach(p => source.Focus.Skill.RemoveSkill(nameof(Common), p.ToLower()));
        }
    }
    public static class ReflectRule
    {
        public static void EffectSwaping_Modifier_After(ActorSet player, ActorSet enemy)
        {
            var playerResolutions = player.Focus.TurnContext.EffectResolutions;

            var reflect = playerResolutions.Where(e => e.TargetType == EffectTargetType.Instance.Enemy() || e.DelayRounds == 0).ToList();

            playerResolutions.RemoveAll(e => reflect.Contains(e));

            reflect.ForEach(e => e.DelayRounds = 1);

            playerResolutions.AddRange(reflect);
        }
        public static void AttackSwaping_Modifier_After(ActorSet player, ActorSet enemy)
        {
            var playerResolutions = player.Focus.TurnContext.AttackResolutions;

            var reflect = playerResolutions.Where(a => a.DelayRounds == 0).ToList();

            playerResolutions.RemoveAll(a => reflect.Contains(a));

            reflect.ForEach(a =>
            {
                a.DelayRounds = 1;
                a.Source = player;
            });

            playerResolutions.AddRange(reflect);
        }
    }
}