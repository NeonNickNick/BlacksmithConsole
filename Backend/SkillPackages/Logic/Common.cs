using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class Common : SkillPackageBase
    {
        private static readonly List<string> _professions = new()
        {
            "warlock",
            "driver"
        };
        public override string Name => "common";
        public Common()
        {
            InitializeSkills();
        }
        public override DSL.SourceFile PassiveSkill(ISkillContext sc)
        {
            return new(sc.Self);
        }
        private static bool IronCheck(ISkillContext sc) => true;
        private static DSL.SourceFile Iron(ISkillContext sc)
        {
            Pen pen = sf => sf.WriteResource(1, ResourceType.Iron);
            return DSL.Create(sc.Self, pen);
        }

        private static bool StickCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 0.5f);
        }
        private static DSL.SourceFile Stick(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Iron)
                .WriteAttack(1, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private static bool DrillCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1.5f);
        }
        private static DSL.SourceFile Drill(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1.5f, ResourceType.Iron)
                .WriteAttack(3, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private static bool SlashCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 2.5f);
        }
        private static DSL.SourceFile Slash(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Iron)
                .WriteAttack(5, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private static bool ShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, sc.Param * 0.5f);
        }
        private static DSL.SourceFile Shield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param * 0.5f, ResourceType.Iron)
                .WriteDefense(2 + sc.Param, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }

        private static bool ThornShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1 + sc.Param * 0.5f);
        }
        private static DSL.SourceFile ThornShield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param * 0.5f, ResourceType.Iron)
                .WriteDefense(4 + sc.Param, new ThornReduction());
            return DSL.Create(sc.Self, pen);
        }

        private static bool RecoveryCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1 + sc.Param);
        }
        private static DSL.SourceFile Recovery(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param, ResourceType.Iron)
                .WriteRecovery(2 + 2 * sc.Param);
            return DSL.Create(sc.Self, pen);
        }

        private static bool SpaceCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 3);
        }
        private static DSL.SourceFile Space(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Iron)
                .WriteResource(1, ResourceType.Space);
            return DSL.Create(sc.Self, pen);
        }

        private static bool TimeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 3);
        }
        private static DSL.SourceFile Time(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Iron)
                .WriteResource(1, ResourceType.Time);
            return DSL.Create(sc.Self, pen);
        }

        private static bool TearCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Space, 1f);
        }
        private static DSL.SourceFile Tear(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Space)
                .WriteAttack(8, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }
        private static bool ReflectCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Space, 2f);
        }
        private static DSL.SourceFile Reflect(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Space)
                .LinkJudgeRule("reflect");
            return DSL.Create(sc.Self, pen);
        }

        private static bool WarlockCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1f);
        }
        private static DSL.SourceFile Warlock(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Iron)
                .WriteFree(source => 
                { 
                    _professions.ForEach(p => source.Focus.Skill.RemoveSkill("common", p));
                    source.Focus.Skill.AddPackage(new Warlock());
                });
            return DSL.Create(sc.Self, pen);
        }

        private static bool CannonCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 4);
        }
        private static DSL.SourceFile Cannon(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(4, ResourceType.Iron)
                .WriteFree(source =>
                {
                    _professions.ForEach(p => source.Focus.Skill.RemoveSkill("common", p));
                    source.Focus.Skill.AddPackage(new Cannon());
                });
            return DSL.Create(sc.Self, pen);
        }

        private static bool DriverCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 3);
        }
        private static DSL.SourceFile Driver(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Iron)
                .WriteFree(source =>
                {
                    _professions.ForEach(p => source.Focus.Skill.RemoveSkill("common", p));
                    source.Focus.Skill.AddPackage(new Driver());
                });
            return DSL.Create(sc.Self, pen);
        }
    }
}