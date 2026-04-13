using Blacksmith.Backend.JudgementLogic.Judgement.Core;
using Blacksmith.Backend.SkillPackages.Logic;

namespace Blacksmith.Backend.JudgementLogic.Judgement
{
    using DSL = DSLforSkillLogic;
    public class Judger
    {
        public ActorSet Player { get; }
        public ActorSet Enemy { get; }
        public JudgeRuleManager JudgeRuleManager { get; }

        private List<Intent> _playerIntents = new();
        private List<Intent> _enemyIntents = new();

        private Action _onJudge;
        public Judger(ActorSet player, ActorSet enemy)
        {
            Player = player;
            Enemy = enemy;
            JudgeRuleManager = new();
            _onJudge = () =>
            {
                TranslateIntentsToResolutions();
                JudgeRuleManager.GetRule()(Player, Enemy);
            };
        }
        private List<Intent> Compile(List<DSL.SourceFile> sourceFiles)
        {
            return sourceFiles.Select(s => s.Compile(this)).ToList();
        }
        public void Judge(List<DSL.SourceFile> playerSfs, List<DSL.SourceFile> enemySfs)
        {
            _playerIntents = Compile(playerSfs);
            _enemyIntents = Compile(enemySfs);
            _onJudge();
            JudgeRuleManager.Update();
        }

        public void TranslateIntentsToResolutions()
        {
            foreach (var temp in _playerIntents)
            {
                temp.Execute?.Invoke(Player);
            }
            foreach (var temp in _enemyIntents)
            {
                temp.Execute?.Invoke(Enemy);
            }
        }
    }
}
