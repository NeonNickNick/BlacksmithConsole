using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Judgement;

namespace Blacksmith.AI
{
    public interface IAIStrategy
    {
        string Name { get; }
        (string skillName, int param) ChooseSkill(
            ActorSet self,
            ActorSet opponent);
    }
}
