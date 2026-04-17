using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;

namespace Blacksmith.Backend.JudgementLogic.TurnContexts
{
    public class ResourceResolution : IResolution
    {
        public int DelayRounds { get; set; } = 0;
        public ResourceType.BEValue Type { get; set; }
        public float Power { get; set; }
        public Action<ActorSet> Execute { get; set; } = null!;
        public ResourceResolution() { }
        public ResourceResolution(ResourceType.BEValue type, float power, Action<ActorSet> execute)
        {
            Type = type;
            Power = power;
            Execute = execute;
        }
    }
}
