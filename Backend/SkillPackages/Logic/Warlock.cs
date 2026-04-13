using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class Warlock : SkillPackageBase
    {
        public override string Name => "warlock";
        public Warlock()
        {
            InitializeSkills();
        }
        private static bool MagicCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1);
        }
        private static DSL.SourceFile Magic(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Self, 1, ResourceType.Iron)
                .WriteResource(1, ResourceType.Magic);
            return DSL.Create(pen);
        }

        private static bool MagicAttackCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Self.Focus.Resource.Check(ResourceType.Magic, sc.Param);
        }
        private static DSL.SourceFile MagicAttack(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteAttack(2 * sc.Param, AttackType.Physical, delayRounds: 0)
                .WriteAttack(2 * sc.Param, AttackType.Physical, delayRounds: 1)
                .WriteAttack(2 * sc.Param, AttackType.Physical, delayRounds: 2);
            return DSL.Create(pen);
        }
    }
}