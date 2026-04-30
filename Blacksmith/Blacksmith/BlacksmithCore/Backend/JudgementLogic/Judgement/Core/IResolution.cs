namespace BlacksmithCore.Backend.JudgementLogic.Judgement.Core
{
    public interface IResolution
    {
        public int DelayRounds { get; set; }
        public float Power { get; set; }
        public Action<ActorSet> Execute { get; set; }
    }
}