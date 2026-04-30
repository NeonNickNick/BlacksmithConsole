using BlacksmithCore.Backend.JudgementLogic.Judgement;

namespace BlacksmithCore.Backend.SkillPackages
{
    public interface ISkillContext
    {
        public string SkillName { get; }
        public ActorSet Self { get; }
        public int Param { get; }
    }
}
