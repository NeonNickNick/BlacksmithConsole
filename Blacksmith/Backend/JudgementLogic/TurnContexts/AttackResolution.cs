using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;

namespace Blacksmith.Backend.JudgementLogic.TurnContexts
{
    public enum AttackStage
    {
        OnEnd
    }
    public class AttackResolution : IResolution
    {
        public ActorSet? Source { get; set; }
        public int DelayRounds { get; set; } = 0;
        public AttackType.BEValue Type { get; set; }
        public float Power { get; set; }
        public Action<ActorSet> Execute { get; set; } = (a) => { };
        public int TotalDamage { get; set; } = 0;

        private readonly Dictionary<AttackStage, List<Action<ActorSet?, Body, AttackResolution>>> _stages = new();
        public AttackResolution() { }

        public void AddStage(AttackStage stage, Action<ActorSet?, Body, AttackResolution> action)
        {
            if (!_stages.TryGetValue(stage, out var list))
            {
                list = new();
                _stages[stage] = list;
            }
            list.Add(action);
        }
        public void RunStage(AttackStage stage, Body target)
        {
            if (_stages.TryGetValue(stage, out var list))
            {
                foreach (var a in list)
                    a(Source, target, this);
            }
        }
    }
}