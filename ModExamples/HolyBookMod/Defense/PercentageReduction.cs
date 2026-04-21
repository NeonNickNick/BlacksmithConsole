using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Infra.ExtensibleEnum;

namespace ModExamples.HolyBookMod.Defense
{
    public class PercentageReduction : DefenseBase
    {
        public override BlacksmithEnum<DefenseType>.BEValue Type { get; set; } = DefenseType.Instance.PercentageReduction();
        public override int Power { get; set; } = 0;
        public int Baseline { get; set; } = 1;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;
        public PercentageReduction(int baseline)
        {
            if(baseline <= 0)
            {
                throw new ArgumentException("百分比伤减的参考值必须大于0！");
            }
            Baseline = baseline;
        }

        public override void Merge(DefenseBase addition)
        {
            //不会执行
        }

        public override void Update()
        {
            IsDead = true;
        }

        public override (int, int) Work(Body source, Body owner, int attack, BlacksmithEnum<AttackType>.BEValue type)
        {
            float percent = 1.0f * Power / Baseline;
            return ((int)(attack * percent), attack - (int)(attack * percent));
        }
    }
}
