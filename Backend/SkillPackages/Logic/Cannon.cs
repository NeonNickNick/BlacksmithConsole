using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class Cannon : SkillPackageBase
    {
        public override string Name => "cannon";
        public Cannon()
        {
            InitializeSkills();
        }
        public override DSL.SourceFile PassiveSkill(ISkillContext sc)
        {
            return new(sc.Self);
        }
        private static bool StrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1);
        }
        private static DSL.SourceFile Strike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Iron)
                .WriteAttack(3, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private static bool DoubleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 2);
        }
        private static DSL.SourceFile DoubleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Iron)
                .WriteAttack(7, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private static bool TripleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 3);
        }
        private static DSL.SourceFile TripleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Iron)
                .WriteAttack(11, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private static bool APShellCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1);
        }
        private static DSL.SourceFile APShell(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Iron)
                .WriteAttack(2, AttackType.Physical, 3);
            return DSL.Create(sc.Self, pen);
        }

        private static bool CannonBarrelCheck(ISkillContext sc) => true;
        private static DSL.SourceFile CannonBarrel(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(2, new CommonReduction())
                .WriteAttack(1, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }
    }
}