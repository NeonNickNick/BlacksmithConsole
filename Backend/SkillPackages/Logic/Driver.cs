using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public class Driver : SkillPackageBase
    {
        public override string Name => "driver";
        public Driver()
        {
            InitializeSkills();
        }
        public override DSL.SourceFile PassiveSkill(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(1, new RealReduction())
                .WriteDefense((int)MathF.Min(5, sc.Self.Focus.Resource.QueryCommon(ResourceType.Time) * 2), new RealReduction());
            return DSL.Create(sc.Self, pen);
        }
        private static bool SpaceAttackCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Space, 1);
        }
        private static DSL.SourceFile SpaceAttack(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Space)
                .WriteAttack(11, AttackType.Physical);
            return DSL.Create(sc.Self, pen);
        }

        private static bool Space2TimeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Space, 1);
        }
        private static DSL.SourceFile Space2Time(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Space)
                .WriteResource(1, ResourceType.Time)
                .WriteDefense(3, new RealReduction());
            return DSL.Create(sc.Self, pen);
        }

        private static bool Time2SpaceCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Time, 1);
        }
        private static DSL.SourceFile Time2Space(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Time)
                .WriteResource(1, ResourceType.Space)
                .WriteDefense(3, new RealReduction());
            return DSL.Create(sc.Self, pen);
        }

        private static bool SpaceBarrierCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Param <= 5 && sc.Self.Focus.Resource.Check(ResourceType.Iron, sc.Param);
        }
        private static DSL.SourceFile SpaceBarrier(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param, ResourceType.Iron)
                .WriteDefense(5.5f * sc.Param - 0.5f * sc.Param * sc.Param, new RealReduction());
            return DSL.Create(sc.Self, pen);
        }
    }
}