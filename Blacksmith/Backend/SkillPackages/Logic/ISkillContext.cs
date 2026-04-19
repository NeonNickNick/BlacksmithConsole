using Blacksmith.Backend.JudgementLogic.Judgement;

namespace Blacksmith.Backend.Backend.SkillPackages.Logic
{
    public interface ISkillContext
    {
        public string SkillName { get; }
        public ActorSet Self { get; }
        public int Param { get; }
    }
}
