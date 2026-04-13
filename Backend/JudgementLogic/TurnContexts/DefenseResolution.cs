using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;

namespace Blacksmith.Backend.JudgementLogic.TurnContexts
{
    public class DefenseResolution : IResolution
    {
        public int DelayRounds { get; set; } = 0;
        public DefenseBase Defense { get; set; }
        public float Power { get; set; }
        public Action<ActorSet> Execute { get; set; }
        public DefenseResolution() { }
        public DefenseResolution(DefenseBase defense, float power, Action<ActorSet> execute)
        {
            Defense = defense;
            Power = power;
            Execute = execute;
        }
    }
}