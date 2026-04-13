using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.FrontendBackendInterface;
using Blacksmith.Program;

namespace Blacksmith.FrontEndBackendInterface
{
    public class BackendEntry : ISkillChoose
    {
        public ActorSet Player { get; private set; }
        public ActorSet Enemy { get; private set; }
        public Judger Judger { get; private set; }
        public BackendEntry()
        {
            Player = new();
            Enemy = new();
            Judger = new(Player, Enemy);
        }
        public SkillDeclareResult TryDeclare(string skillName, int param)
        {
            DefaultSkillContext context = new(Player, param);
            return Player.Focus.Skill.TryDeclare(skillName, context);
        }
        public SkillDeclareResult ETryDeclare(string skillName, int param)
        {
            DefaultSkillContext context = new(Enemy, param);
            return Enemy.Focus.Skill.TryDeclare(skillName, context);
        }
        private int temp = 1;
        public void Declare(string skillName, int param)
        {
            var playerContext = new DefaultSkillContext(Player, param);
            var enemyContext = new DefaultSkillContext(Enemy, 0);
            var psfs = Player.Focus.Skill.GetPassiveSkill(playerContext);
            psfs.Add(Player.Focus.Skill.Declare(skillName, playerContext));

            var esfs = Enemy.Focus.Skill.GetPassiveSkill(enemyContext);

            if (temp % 3 != 0)
            {
                esfs.Add(Enemy.Focus.Skill.Declare("iron", enemyContext));
            }
            else
            {
                esfs.Add(Enemy.Focus.Skill.Declare("drill", enemyContext));
            }
            temp++;
            Judger.Judge(psfs, esfs);
        }
        public void Declare(string skillName, int param, string esn, int ep)
        {
            var playerContext = new DefaultSkillContext(Player, param);
            var enemyContext = new DefaultSkillContext(Enemy, ep);

            var psfs = Player.Focus.Skill.GetPassiveSkill(playerContext);
            psfs.Add(Player.Focus.Skill.Declare(skillName, playerContext));

            var esfs = Enemy.Focus.Skill.GetPassiveSkill(enemyContext);
            esfs.Add(Enemy.Focus.Skill.Declare(esn, enemyContext));

            Judger.Judge(psfs, esfs);
        }
    }
}
