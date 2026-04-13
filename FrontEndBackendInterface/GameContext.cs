using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.SkillPackages.Logic;

namespace Blacksmith.FrontendBackendInterface
{
    public interface ISkillChoose
    {
        public SkillDeclareResult TryDeclare(string skillName, int param);
        public void Declare(string skillName, int param);
        //以下为临时
        public SkillDeclareResult ETryDeclare(string skillName, int param);
        public void Declare(string skillName, int param, string esn, int ep);
    }
    public class GameContext
    {
        public ISkillChoose SkillChoose { get; set; }
        public ActorSet Player { get; set; }
        public ActorSet Enemy { get; set; }
    }
}