using BlacksmithCore.AI;
using BlacksmithCore.Backend.JudgementLogic.Actor;
using BlacksmithCore.FrontendBackendInterface;

namespace BlacksmithClient.Frontend
{
    public class WebGameSession
    {
        private readonly List<IAIStrategy> _strategies;
        private GameInstance? _game;
        private IAIStrategy? _activeAI;
        private int _mode;
        private bool _started;
        private string _modeName = string.Empty;
        private bool _isManual;

        public WebGameSession(List<IAIStrategy> strategies)
        {
            _strategies = strategies;
        }

        public List<object> GetStrategies()
        {
            var list = new List<object>();
            for (int i = 0; i < _strategies.Count; i++)
            {
                list.Add(new { id = i, name = _strategies[i].Name + " (AI)" });
            }
            list.Add(new { id = _strategies.Count, name = "Manual" });
            return list;
        }

        public object StartGame(int mode)
        {
            var starter = new BackendStarter();
            _game = starter.StartBackend();
            _mode = mode;
            _started = true;

            _isManual = mode >= _strategies.Count;
            if (_isManual)
            {
                _activeAI = null;
                _modeName = "Manual";
            }
            else
            {
                _activeAI = _strategies[mode];
                _activeAI.Init(_game);
                _modeName = _activeAI.Name;
            }

            return BuildSnapshot();
        }

        public DeclareResult DeclareTurn(string skillName, int param, string esn, int ep)
        {
            if (_game == null || !_started)
                return new DeclareResult { Ok = false, Message = "Game not started.", Snapshot = GetSnapshot() };

            var playerResult = _game.TryDeclare(skillName, param);
            if (playerResult != SkillDeclareResult.Success)
                return new DeclareResult { Ok = false, Message = $"Player skill '{skillName}' {playerResult}.", Snapshot = GetSnapshot() };

            string enemySkillName;
            int enemyParam;

            if (!_isManual && _activeAI != null)
            {
                (enemySkillName, enemyParam) = _activeAI.ChooseSkill(_game.Enemy, _game.Player);
            }
            else
            {
                var enemyResult = _game.ETryDeclare(esn, ep);
                if (enemyResult != SkillDeclareResult.Success)
                    return new DeclareResult { Ok = false, Message = $"Enemy skill '{esn}' {enemyResult}.", Snapshot = GetSnapshot() };

                enemySkillName = esn;
                enemyParam = ep;
            }

            _game.Declare(skillName, param, enemySkillName, enemyParam);

            var snapshot = BuildSnapshot();
            return new DeclareResult { Ok = true, Message = "Turn resolved.", Snapshot = snapshot };
        }

        public object GetSnapshot()
        {
            return BuildSnapshot();
        }

        private object BuildSnapshot()
        {
            if (_game == null || !_started)
            {
                return new
                {
                    player = (object?)null,
                    enemy = (object?)null,
                    turns = Array.Empty<object>(),
                    started = false,
                    manualMode = true,
                    modeName = "Not started",
                    result = "Awaiting game"
                };
            }

            var pv = _game.Player.Focus.GetView();
            var ev = _game.Enemy.Focus.GetView();

            return new
            {
                player = BuildActor(pv, _game.Player.Focus.Skill.GetAvailableSkillNames()),
                enemy = BuildActor(ev, _game.Enemy.Focus.Skill.GetAvailableSkillNames()),
                turns = _game.History.SkillHistory.Select((pair, i) => new
                {
                    index = i + 1,
                    result = "Continue",
                    playerSkill = pair.Item1.SkillName,
                    playerParam = pair.Item1.Param,
                    enemySkill = pair.Item2.SkillName,
                    enemyParam = pair.Item2.Param
                }).ToList(),
                started = _started,
                manualMode = _isManual,
                modeName = _modeName,
                result = DetermineResult(pv, ev)
            };
        }

        private static object BuildActor(BodyView view, List<string> availableSkills)
        {
            return new
            {
                professions = view.ProfessionNames,
                hp = view.HP,
                maxHP = view.MHP,
                defenses = view.DefenseView.Select(d => new { name = d.name, power = d.power }).ToList(),
                resources = view.ResourcesView.Select(r => new { name = r.name, quantity = r.quantity }).ToList(),
                futureAttacks = view.FutureAttackView.Select(f => new
                {
                    name = f.name,
                    delayRounds = f.delayRounds,
                    power = f.power
                }).ToList(),
                futureDefenses = view.FutureDefenseView.Select(f => new
                {
                    name = f.name,
                    delayRounds = f.delayRounds,
                    power = f.power
                }).ToList(),
                availableSkills = availableSkills
            };
        }

        private static string DetermineResult(BodyView player, BodyView enemy)
        {
            bool playerDead = player.HP <= 0;
            bool enemyDead = enemy.HP <= 0;

            if (playerDead && enemyDead) return "draw";
            if (playerDead) return "lose";
            if (enemyDead) return "win";
            return "next";
        }
    }

    public class DeclareResult
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Snapshot { get; set; }
    }
}
