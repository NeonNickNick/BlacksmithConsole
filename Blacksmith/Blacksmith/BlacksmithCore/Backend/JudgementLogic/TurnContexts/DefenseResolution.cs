using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.JudgementLogic.Judgement;
using BlacksmithCore.Backend.JudgementLogic.Judgement.Core;

namespace BlacksmithCore.Backend.JudgementLogic.TurnContexts
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