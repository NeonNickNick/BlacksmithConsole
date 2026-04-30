using BlacksmithCore.Backend.JudgementLogic.Judgement;
using BlacksmithCore.Driver;

namespace BlacksmithCore.AI
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
