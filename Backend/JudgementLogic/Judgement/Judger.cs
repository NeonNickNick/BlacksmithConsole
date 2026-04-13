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
        private Intent Compile(DSL.SourceFile sourceFiles)
        {
            return sourceFiles.Compile(this);
        }
        public void Judge(DSL.SourceFile playerSfs, DSL.SourceFile enemySfs)
        {
            _playerIntents = new() { Compile(playerSfs) };
            _enemyIntents = new() { Compile(enemySfs) };
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
