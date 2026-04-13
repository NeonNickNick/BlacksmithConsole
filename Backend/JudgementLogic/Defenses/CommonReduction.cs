using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
namespace Blacksmith.Backend.JudgementLogic.Defenses
{
    public class CommonReduction : DefenseBase
    {
        public override DefenseType Type { get; set; } = DefenseType.CommonReduction;
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
            return (int)MathF.Max(0, attack - Power);
        }
    }
}