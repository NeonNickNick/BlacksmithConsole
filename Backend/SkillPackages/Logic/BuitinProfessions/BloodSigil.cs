using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class BloodSigil : SkillPackageBase
    {
        public override string Name => "bloodsigil";
        public BloodSigil()
        {
            InitializeSkills();
        }
        private static bool BloodBladeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Health.HP > 4;
        }
        private static DSL.SourceFile BloodBlade(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    source.Focus.Health.LoseHP(4);
                })
                .WriteAttack(6, AttackType.Magic)
                    .BloodSuck(0.75f);
            return DSL.Create(sc.Self, pen);
        }
        /*
        private static bool BloodLustCheck(ISkillContext sc)
        {

        }
        private static DSL.SourceFile BloodLust(ISkillContext sc)
        {

        }
        private static bool BloodRecoveryCheck(ISkillContext sc)
        {

        }
        private static DSL.SourceFile BloodRecovery(ISkillContext sc)
        {

        }
        private static bool BloodShieldCheck(ISkillContext sc)
        {

        }
        private static DSL.SourceFile BloodShield(ISkillContext sc)
        {

        }
        private static bool BloodRageCheck(ISkillContext sc)
        {

        }
        private static DSL.SourceFile BloodRage(ISkillContext sc)
        {

        }*/
    }
}
