using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.TurnContexts;
using Blacksmith.Backend.SkillPackages.Logic;
namespace Blacksmith.Backend.JudgementLogic.Defenses
{
    using DSL = DSLforSkillLogic;
    public class ThornReduction : DefenseBase
    {
        public override DefenseType Type { get; set; } = DefenseType.ThornReduction;
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
        public override int Work(Body source, Body owner, int attack, AttackType type)
        {
            int res = (int)MathF.Max(0, attack - Power);
            if(type == AttackType.Physical)
            {
                DSL.Create(sf => sf.WriteAttack(res / 2, AttackType.Magic, delayRounds: 1)).Compile().Execute(owner.Community);
            }
            return res;
        }
    }
}