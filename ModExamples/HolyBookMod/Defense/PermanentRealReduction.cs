using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Infra.ExtensibleEnum;

namespace ModExamples.HolyBookMod.Defense
{
    public class PermanentRealReduction : DefenseBase
    {
        public override BlacksmithEnum<DefenseType>.BEValue Type { get; set; } = DefenseType.Instance.RealReduction();
        public override int Power { get; set; } = 0;
        public int Baseline { get; set; } = 1;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;
        public override void Merge(DefenseBase addition)
        {
            //不会执行
        }
        public override void Update()
        {
            if (Power <= 0)
            {
                IsDead = true;
            }
        }

        public override (int, int) Work(Body source, Body owner, int attack, BlacksmithEnum<AttackType>.BEValue type)
        {
            return ((int)MathF.Max(0, attack - Power), (int)MathF.Min(attack, Power));
        }
    }
}
