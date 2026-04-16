using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.SkillPackages.Core;
using Blacksmith.Mod;

namespace Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class Common : MainProfession
    {
        private static List<string> Professions => ProfessionRegistry.Professions;
        public override string Name => "common";
        
        private bool IronCheck(ISkillContext sc) => true;
        private DSL.SourceFile Iron(ISkillContext sc)
        {
            Pen pen = sf => sf.WriteResource(1, ResourceType.Iron);
            return DSL.Create(sc.Self, pen);
        }

        private bool StickCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 0.5f);
        }
        private DSL.SourceFile Stick(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Iron)
                .WriteAttack(1, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private bool DrillCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1.5f);
        }
        private DSL.SourceFile Drill(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1.5f, ResourceType.Iron)
                .WriteAttack(3, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private bool SlashCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 2.5f);
        }
        private DSL.SourceFile Slash(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Iron)
                .WriteAttack(5, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private bool ShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, sc.Param * 0.5f);
        }
        private DSL.SourceFile Shield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param * 0.5f, ResourceType.Iron)
                .WriteDefense(2 + sc.Param, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }

        private bool ThornShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1 + sc.Param * 0.5f);
        }
        private DSL.SourceFile ThornShield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param * 0.5f, ResourceType.Iron)
                .WriteDefense(4 + sc.Param, new ThornReduction());
            return DSL.Create(sc.Self, pen);
        }

        private bool RecoveryCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1 + sc.Param);
        }
        private DSL.SourceFile Recovery(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param, ResourceType.Iron)
                .WriteRecovery(2 + 2 * sc.Param);
            return DSL.Create(sc.Self, pen);
        }

        private bool SpaceCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 3);
        }
        private DSL.SourceFile Space(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Iron)
                .WriteResource(1, ResourceType.Space);
            return DSL.Create(sc.Self, pen);
        }

        private bool TimeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 3);
        }
        private DSL.SourceFile Time(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Iron)
                .WriteResource(1, ResourceType.Time);
            return DSL.Create(sc.Self, pen);
        }

        private bool TearCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Space, 1f);
        }
        private DSL.SourceFile Tear(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Space)
                .WriteAttack(8, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }
        private bool ReflectCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Space, 2f);
        }
        private DSL.SourceFile Reflect(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Space)
                .LinkJudgeRule("reflect");
            return DSL.Create(sc.Self, pen);
        }

        private bool WarlockCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1f);
        }
        private DSL.SourceFile Warlock(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Iron)
                .WriteFree(source => 
                { 
                    ExcludeAllProfessions(source);
                    source.Focus.Skill.AddPackage(new Warlock());
                });
            return DSL.Create(sc.Self, pen);
        }

        private bool CannonCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 4);
        }
        private DSL.SourceFile Cannon(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(4, ResourceType.Iron)
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                    source.Focus.Skill.AddPackage(new Cannon());
                });
            return DSL.Create(sc.Self, pen);
        }

        private bool DriverCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 3);
        }
        private DSL.SourceFile Driver(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Iron)
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                    source.Focus.Skill.AddPackage(new Driver());
                });
            return DSL.Create(sc.Self, pen);
        }

        private bool BloodSigilCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 7);
        }
        private DSL.SourceFile BloodSigil(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(7, ResourceType.Iron)
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
                    addition.ForEach(a => source.Focus.Skill.RemoveSkill("common", a));
                    source.Focus.Skill.AddPackage(new BloodSigil());
                    source.Focus.Health.GainMHP(3);
                    source.Focus.Health.GainHP(3);
                });
            return DSL.Create(sc.Self, pen);
        }
        public static void ExcludeAllProfessions(ActorSet source)
        {

            Professions.ForEach(p => source.Focus.Skill.RemoveSkill("common", p));
        }
        public static void ExcludeNone(ActorSet source, string self)
        {
            source.Focus.Skill.RemoveSkill("common", self);
        }
    }
}