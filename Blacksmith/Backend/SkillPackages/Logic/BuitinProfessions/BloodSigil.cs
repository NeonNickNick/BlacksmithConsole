using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class BloodSigil : MainProfession
    {
        private float _increase = 1f;
        public override string Name => "bloodsigil";
        private int IncreaseAttack(int origin)
        {
            var res = (int)MathF.Ceiling(origin * _increase);
            _increase = 1f;
            return res;
        }
        private bool BloodBladeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Health.HP > 4;
        }
        private DSL.SourceFile BloodBlade(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    source.Focus.Health.LoseHP(4);
                })
                .WriteAttack(IncreaseAttack(6), AttackType.Magic)
                    .BloodSuck(0.66f);
            return DSL.Create(sc.Self, pen);
        }
        private bool BloodLustCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Health.HP > 2;
        }
        private DSL.SourceFile BloodLust(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    source.Focus.Health.LoseHP(2);
                    _increase = 1.5f;
                });
            return DSL.Create(sc.Self, pen);
        }
        private bool BloodRecoveryCheck(ISkillContext sc) => true;
        private DSL.SourceFile BloodRecovery(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteRecovery(1);
            return DSL.Create(sc.Self, pen);
        }
        private bool BloodShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Health.HP > 1;
        }
        private DSL.SourceFile BloodShield(ISkillContext sc)
        {
            int power = (int)MathF.Ceiling(0.4f * sc.Self.Focus.Health.HP);
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    source.Focus.Health.LoseHP(1);
                })
                .WriteDefense(power, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }
        private bool BloodRageCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Health.HP > 1 && sc.Self.Focus.Health.HP <= 5;
        }
        private DSL.SourceFile BloodRage(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    source.Focus.Health.LoseHP(1);
                })
                .WriteAttack(IncreaseAttack(5), AttackType.Magic)
                    .BloodSuck(1.5f);
            return DSL.Create(sc.Self, pen);
        }
    }
}
