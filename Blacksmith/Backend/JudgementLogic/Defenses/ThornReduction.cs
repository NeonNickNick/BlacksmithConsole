using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.SkillPackages.Logic;
namespace Blacksmith.Backend.JudgementLogic.Defenses
{
    using DSL = DSLforSkillLogic;
    public class ThornReduction : DefenseBase
    {
        public override DefenseType.BEValue Type { get; set; } = DefenseType.Instance.ThornReduction();
        public override int Power { get; set; } = 0;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;

        public override void Merge(DefenseBase addition)
        {
            //不会被调用
        }
        public override void Update()
        {
            IsDead = true;
        }
        public override (int, int) Work(Body source, Body owner, int attack, AttackType.BEValue type)
        {
            int res = (int)MathF.Max(0, attack - Power);
            int absorbed = (int)MathF.Min(attack, Power);
            if (type == AttackType.Instance.Physical())
            {
                DSL.Create(owner.Community, sf => sf.WriteAttack((int)MathF.Ceiling(absorbed / 2f), AttackType.Instance.Magical(), delayRounds: 1)).Compile().Execute(owner.Community);
            }
            return (res, absorbed);
        }
    }
}