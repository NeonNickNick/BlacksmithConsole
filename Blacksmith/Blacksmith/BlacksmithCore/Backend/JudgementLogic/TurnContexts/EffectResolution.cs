using BlacksmithCore.Backend.JudgementLogic.Actor;
using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.JudgementLogic.Judgement;
using BlacksmithCore.Backend.JudgementLogic.Judgement.Core;

namespace BlacksmithCore.Backend.JudgementLogic.TurnContexts
{
    public enum EffectStage
    {
        OnSuccessfullyAdded
    }
    public class EffectResolution : IResolution
    {
        public int DelayRounds { get; set; } = 0;
        public readonly EffectType.BEValue Type;
        public EffectTargetType.BEValue TargetType { get; set; }
        public float Power { get; set; }
        public Action<ActorSet> Execute { get; set; } = null!;
        public EffectResolution(EffectType.BEValue type, EffectTargetType.BEValue targetType, float power)
        {
            Type = type;
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