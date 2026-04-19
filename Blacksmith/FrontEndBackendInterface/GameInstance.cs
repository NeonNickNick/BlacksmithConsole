using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Judgement;

namespace Blacksmith.FrontendBackendInterface
{
    public class DefaultSkillContext : ISkillContext
    {
        public string SkillName { get; }
        public ActorSet Self { get; }
        public int Param { get; }
        public DefaultSkillContext(string skillName, ActorSet self, int param)
        {
            SkillName = skillName;
            Self = self;
            Param = param;
        }
    }
    public class GameInstance
    {
        public ActorSet Player { get; private set; }
        public ActorSet Enemy { get; private set; }
        public Judger Judger { get; private set; }
        public GameHistory History { get; private set; }
        public GameInstance()
        {
            Player = new();
            Enemy = new();
            Judger = new(Player, Enemy);
            History = new();
        }
        public GameInstance DeepCopy()
        {
            GameInstance res = new();
            foreach(var pair in History.SkillHistory)
            {
                res.Declare(pair.Item1.SkillName, pair.Item1.Param, pair.Item2.SkillName, pair.Item2.Param);
            }
            return res;
        }
        public SkillDeclareResult TryDeclare(string skillName, int param)
        {
            DefaultSkillContext context = new(skillName, Player, param);
            return Player.Focus.Skill.TryDeclare(skillName, context);
        }
        public SkillDeclareResult ETryDeclare(string skillName, int param)
        {
            DefaultSkillContext context = new(skillName, Enemy, param);
            return Enemy.Focus.Skill.TryDeclare(skillName, context);
        }
       
        public void Declare(string skillName, int param, string esn, int ep)
        {
            var playerContext = new DefaultSkillContext(skillName, Player, param);
            var enemyContext = new DefaultSkillContext(esn, Enemy, ep);

            History.SkillHistory.Add((playerContext, enemyContext));

            var psfs = Player.Focus.Skill.GetPassiveSkill(playerContext);
            psfs.Add(Player.Focus.Skill.Declare(skillName, playerContext));

            var esfs = Enemy.Focus.Skill.GetPassiveSkill(enemyContext);
            esfs.Add(Enemy.Focus.Skill.Declare(esn, enemyContext));

            Judger.Judge(psfs, esfs);
        }
    }
}
