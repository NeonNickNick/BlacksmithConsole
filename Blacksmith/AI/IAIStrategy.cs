using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.FrontendBackendInterface;

namespace Blacksmith.AI
{
    public interface IAIStrategy
    {
        string Name { get; }
        public void Init(GameInstance gameInstance);
        (string skillName, int param) ChooseSkill(
            ActorSet self,
            ActorSet opponent);
    }
}
