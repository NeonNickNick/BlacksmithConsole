using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;

namespace Blacksmith.Backend.JudgementLogic.TurnContexts
{
    public enum EffectStage
    {
        OnSuccessfullyAdded
    }
    public class EffectResolution : IResolution
    {
        public int DelayRounds { get; set; } = 0;
        public readonly EffectType Type;
        public List<EffectTag> Tags { get; set; }
        public EffectTargetType TargetType { get; set; }
        public float Power { get; set; }
        public Action<ActorSet> Execute { get; set; }
        public EffectResolution(EffectType type, List<EffectTag> tags, EffectTargetType targetType, float power)
        {
            Type = type;
            Tags = tags;
            TargetType = targetType;
            Power = power;
        }
        private readonly Dictionary<EffectStage, List<Action<ActorSet, Body, EffectResolution>>> _stages = new();
        public void AddStage(EffectStage stage, Action<ActorSet, Body, EffectResolution> action)
        {
            if (!_stages.TryGetValue(stage, out var list))
            {
                list = new();
                _stages[stage] = list;
            }
            list.Add(action);
        }
        public void RunStage(EffectStage stage, ActorSet source, Body target)
        {
            if (_stages.TryGetValue(stage, out var list))
            {
                foreach (var a in list)
                    a(source, target, this);
            }
        }
    }
}