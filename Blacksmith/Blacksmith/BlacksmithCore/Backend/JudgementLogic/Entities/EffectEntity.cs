using BlacksmithCore.Backend.JudgementLogic.Actor;
using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.JudgementLogic.Judgement.Core;

namespace BlacksmithCore.Backend.JudgementLogic.Entities
{
    public class EffectEntity
    {
        public readonly EffectType.BEValue Type;
        public int DelayTimes { get; set; } = 0;
        public int RemainingTimes { get; set; }
        public IResolution Resolution { get; set; }
        public Action<Body> Execute { get; set; } = null!;
        public EffectEntity(EffectType.BEValue type, int remainingTimes, IResolution resolution)
        {
            Type = type;
            RemainingTimes = remainingTimes;
            Resolution = resolution;
        }
    }
}