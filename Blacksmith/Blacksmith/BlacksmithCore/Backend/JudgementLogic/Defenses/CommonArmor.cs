using BlacksmithCore.Backend.JudgementLogic.Actor;
using BlacksmithCore.Backend.JudgementLogic.Core;
namespace BlacksmithCore.Backend.JudgementLogic.Defenses
{
    public class CommonArmor : DefenseBase
    {
        public override DefenseType.BEValue Type { get; set; } = DefenseType.Instance.CommonArmor();
        public override int Power { get; set; } = 0;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;

        public override void Merge(DefenseBase addition)
        {
            //不会被调用
        }
        public override void Update()
        {
            if (Power <= 0)
            {
                IsDead = true;
            }
        }
        public override (int, int) Work(Body source, Body owner, int attack, AttackType.BEValue type)
        {
            var res = ((int)MathF.Max(0, attack - Power), (int)MathF.Min(attack, Power));
            Power = (int)MathF.Max(0, Power - attack);
            return res;
        }
    }
}