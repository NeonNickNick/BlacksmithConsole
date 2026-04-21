using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Infra.ExtensibleEnum;

namespace ModExamples.HolyBookMod.Defense
{
    public class GreyHP : DefenseBase
    {
        public override BlacksmithEnum<DefenseType>.BEValue Type { get; set; } = DefenseType.Instance.GreyHP();
        public override int Power { get; set; } = 0;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;
        public override void Merge(DefenseBase addition)
        {
            //不会执行
        }
        public override void Update()
        {
            if(Power <= 0)
            {
                IsDead = true;
            }
        }
        public override (int, int) Work(Body source, Body owner, int attack, BlacksmithEnum<AttackType>.BEValue type)
        {
            var res = ((int)MathF.Max(0, attack - Power), (int)MathF.Min(attack, Power));
            Power = (int)MathF.Max(0, Power - attack);
            return res;
        }
    }
}
