using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public class Cannon : MainProfession
    {
        public override DSL.SourceFile PassiveSkill(ISkillContext sc)
        {
            return new(sc.Self);
        }
        private bool StrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 1);
        }
        private DSL.SourceFile Strike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(4, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool DoubleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 2);
        }
        private DSL.SourceFile DoubleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Iron())
                .WriteAttack(8, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool TripleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 3);
        }
        private DSL.SourceFile TripleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteAttack(11, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool APShellCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 1);
        }
        private DSL.SourceFile APShell(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(2, AttackType.Instance.Physical(), 3)
                    .WithInterupt();
            return DSL.Create(sc.Self, pen);
        }

        private bool CannonBarrelCheck(ISkillContext sc) => true;
        private DSL.SourceFile CannonBarrel(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(2, new CommonReduction())
                .WriteAttack(1, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
    }
}