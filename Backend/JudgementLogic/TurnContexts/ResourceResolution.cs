using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;

namespace Blacksmith.Backend.JudgementLogic.TurnContexts
{
    public class ResourceResolution : IResolution
    {
        public int DelayRounds { get; set; } = 0;
        public ResourceType Type { get; set; }
        public float Power { get; set; }
        public Action<ActorSet> Execute { get; set; }
        public ResourceResolution() { }
        public ResourceResolution(ResourceType type, float power, Action<ActorSet> execute)
        {
            Type = type;
            Power = power;
            Execute = execute;
        }
    }
}
