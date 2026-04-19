using System.Text.Json;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.FrontendBackendInterface;


namespace Blacksmith.AI.Strategies
{
    public class GeneralStrategyParams
    {
        public double TemperatureCoefficient { get; set; } = 0.03; // 原 0.03 * round

        // 终局奖励/惩罚
        public double TerminalWinScore { get; set; } = 1e9;
        public double TerminalLoseScore { get; set; } = -1e9;

        // ========== 早期资源权重 ==========
        public double EarlyIronWeight { get; set; } = 1200;
        public double EarlyExcessIronWeight { get; set; } = 1200;     // 超过4铁时的额外奖励
        public double EarlySpaceWeight { get; set; } = 4000;
        public double EarlyTimeWeight { get; set; } = 3500;
        public double EarlyMagicWeight { get; set; } = 2000;
        public double EarlyIronOverstockPenalty { get; set; } = 80;   // 超过5铁的惩罚

        // ========== 中期资源权重 ==========
        public double MidIronWeight { get; set; } = 60;
        public double MidSpaceWeight { get; set; } = 200;
        public double MidTimeWeight { get; set; } = 180;
        public double MidMagicWeight { get; set; } = 120;

        // ========== 后期资源权重 ==========
        public double LateIronWeight { get; set; } = 20;
        public double LateSpaceWeight { get; set; } = 50;
        public double LateTimeWeight { get; set; } = 50;
        public double LateMagicWeight { get; set; } = 30;

        // ========== 职业相关 ==========
        public double HaveProfessionBonus { get; set; } = 500;
        public double EnemyHasProfessionPenalty { get; set; } = -800;
        public double IronDeficitPenaltyWhenEnemyHasProfession { get; set; } = -300;
        public double IronDeficitPenaltyWhenBothNoProfession { get; set; } = -1000;
        public double IronDeficitThreshold { get; set; } = 2;         // 铁差阈值

        // ========== 攻击策略权重 ==========
        public double EarlyUnnecessaryAttackPenaltyMultiplier { get; set; } = 30;    // (100 - HP) * 30
        public double MidAdvantageAttackBonusMultiplier { get; set; } = 2;           // hpDiff * 2
        public double MidUnnecessaryAttackPenaltyMultiplier { get; set; } = 10;      // (100 - HP) * 10
        public double WithProfessionDamageBonusMultiplier { get; set; } = 20;        // (100 - HP) * 20
        public double WithProfessionHpDiffBonusMultiplier { get; set; } = 5;         // hpDiff * 5
        public double HpAdvantageThreshold { get; set; } = 20;                       // hpDiff > 20 才算优势

        // ========== 回合节奏 ==========
        public double EarlyRoundBonusPerRound { get; set; } = 1;
        public double LateRoundPenaltyPerRound { get; set; } = 4;
    }
    public class GeneralStrategy : IAIStrategy
    {
        private readonly GeneralStrategyParams _params;
        private GameInstance _main = null!;
        private static ThreadLocal<Random> _random = new(() => new Random(Guid.NewGuid().GetHashCode()));

        public string Name => "General";

        public GeneralStrategy(GeneralStrategyParams? parameters = null)
        {
            _params = parameters ?? new GeneralStrategyParams();
            if (parameters == null)
            {
                Console.WriteLine($"未找到参数文件，使用默认参数");
            }
            else
            {
                Console.WriteLine($"已加载参数文件");
            }
        }

        public void Init(GameInstance gameInstance)
        {
            _main = gameInstance;
        }

        public (string skillName, int param) ChooseSkill(ActorSet self, ActorSet opponent)
        {
            int threadCount = Math.Min(8, Environment.ProcessorCount);
            var tasks = new List<Task<List<MCTSNode>>>();

            for (int t = 0; t < threadCount; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var localGame = _main.DeepCopy();
                    return RunMCTS(localGame, 4000 / threadCount);
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // 合并 root children
            var merged = new Dictionary<(string, int), (double wins, int visits)>();
            foreach (var task in tasks)
            {
                foreach (var child in task.Result)
                {
                    var action = child.Action!.Value;
                    if (!merged.ContainsKey(action))
                        merged[action] = (0, 0);

                    var v = merged[action];
                    v.wins += child.Wins;
                    v.visits += child.Visits;
                    merged[action] = v;
                }
            }

            var finalChildren = merged.Select(kv => new MCTSNode(null!, null!, new List<(string, int)>())
            {
                Action = kv.Key,
                Wins = kv.Value.wins,
                Visits = kv.Value.visits
            }).ToList();

            return SampleFromTopK(finalChildren, _main.History.SkillHistory.Count);
        }

        private List<MCTSNode> RunMCTS(GameInstance rootState, int iterations)
        {
            var rootActions = GetAllAvailable(rootState.Enemy, rootState);
            var root = new MCTSNode(rootState, null, rootActions);

            for (int i = 0; i < iterations; i++)
            {
                var node = root;

                // Selection
                while (node.UntriedActions.Count == 0 && node.Children.Count > 0)
                {
                    node = Select(node);
                }

                // Expansion
                if (node.UntriedActions.Count > 0)
                {
                    var action = node.UntriedActions[0];
                    node.UntriedActions.RemoveAt(0);

                    var nextState = node.State.DeepCopy();
                    var playerAction = RandomAction(nextState.Player, nextState);
                    nextState.Declare(
                        playerAction.Item1, playerAction.Item2,
                        action.Item1, action.Item2
                    );

                    var nextActions = GetAllAvailable(nextState.Enemy, nextState);
                    var child = new MCTSNode(nextState, node, nextActions) { Action = action };
                    node.Children.Add(child);
                    node = child;
                }

                // Rollout
                var simState = node.State.DeepCopy();
                for (int d = 0; d < 20; d++)
                {
                    if (IsTerminal(simState))
                        break;

                    var p = RandomAction(simState.Player, simState);
                    var e = RandomAction(simState.Enemy, simState);
                    simState.Declare(p.Item1, p.Item2, e.Item1, e.Item2);
                }

                double result = Evaluate(simState);

                // Backprop
                while (node != null)
                {
                    node.Visits++;
                    node.Wins += result;
                    node = node.Parent!;
                }
            }

            return root.Children;
        }

        private (string, int) SampleFromTopK(List<MCTSNode> children, int round)
        {
            int k = Math.Min(2, children.Count);
            double temperature = Math.Max(0, _params.TemperatureCoefficient * round);

            var topK = children
                .OrderByDescending(c => c.Wins / (c.Visits + 1e-6))
                .Take(k)
                .ToList();

            double maxScore = topK.Max(c => c.Wins / (c.Visits + 1e-6));

            List<double> weights = new();
            double sum = 0;
            foreach (var c in topK)
            {
                double q = c.Wins / (c.Visits + 1e-6);
                double w = Math.Exp((q - maxScore) / temperature);
                weights.Add(w);
                sum += w;
            }

            double r = _random.Value!.NextDouble() * sum;
            double acc = 0;
            for (int i = 0; i < topK.Count; i++)
            {
                acc += weights[i];
                if (r <= acc)
                    return topK[i].Action!.Value;
            }
            return topK.Last().Action!.Value;
        }

        private class MCTSNode
        {
            public GameInstance State;
            public MCTSNode? Parent;
            public List<MCTSNode> Children = new();
            public (string skill, int param)? Action;
            public int Visits = 0;
            public double Wins = 0;
            public List<(string, int)> UntriedActions;

            public MCTSNode(GameInstance state, MCTSNode? parent, List<(string, int)> actions)
            {
                State = state;
                Parent = parent;
                UntriedActions = new List<(string, int)>(actions);
            }
        }

        private MCTSNode Select(MCTSNode node)
        {
            return node.Children.OrderByDescending(child =>
            {
                double mean = child.Wins / (child.Visits + 1e-6);
                double uct = mean +
                    MathF.Sqrt(2) * Math.Sqrt(Math.Log(node.Visits + 1) / (child.Visits + 1e-6));
                return uct;
            }).First();
        }

        private double Evaluate(GameInstance state)
        {
            var enemy = state.Enemy;
            var player = state.Player;

            double enemyHP = enemy.Focus.Health.HP;
            double playerHP = player.Focus.Health.HP;

            double enemyIron = enemy.Focus.Resource.QueryAll(ResourceType.Instance.Iron());
            double enemySpace = enemy.Focus.Resource.QueryAll(ResourceType.Instance.Space());
            double enemyTime = enemy.Focus.Resource.QueryAll(ResourceType.Instance.Time());
            double enemyMagic = enemy.Focus.Resource.QueryAll(ResourceType.Instance.Magic());

            double playerIron = player.Focus.Resource.QueryAll(ResourceType.Instance.Iron());

            bool haveProfession = enemy.Focus.Skill.HaveProfession;
            bool playerHaveProfession = player.Focus.Skill.HaveProfession;

            int round = state.History.SkillHistory.Count;

            // 终局
            if (enemyHP <= 0) return _params.TerminalLoseScore;
            if (playerHP <= 0) return _params.TerminalWinScore;

            double score = 0;

            // 阶段划分
            bool early = round < 8;
            bool mid = round >= 8 && round < 15;
            bool late = round >= 15;

            // 1️⃣ 资源系统
            double resourceScore = 0;
            if (early)
            {
                resourceScore += enemyIron * _params.EarlyIronWeight;
                resourceScore += Math.Max(0, enemyIron - 4) * _params.EarlyExcessIronWeight;
                resourceScore += enemySpace * _params.EarlySpaceWeight;
                resourceScore += enemyTime * _params.EarlyTimeWeight;
                resourceScore += enemyMagic * _params.EarlyMagicWeight;
                resourceScore -= Math.Max(0, enemyIron - 5) * _params.EarlyIronOverstockPenalty;
            }
            else if (mid)
            {
                resourceScore += enemyIron * _params.MidIronWeight;
                resourceScore += enemySpace * _params.MidSpaceWeight;
                resourceScore += enemyTime * _params.MidTimeWeight;
                resourceScore += enemyMagic * _params.MidMagicWeight;
            }
            else // late
            {
                resourceScore += enemyIron * _params.LateIronWeight;
                resourceScore += enemySpace * _params.LateSpaceWeight;
                resourceScore += enemyTime * _params.LateTimeWeight;
                resourceScore += enemyMagic * _params.LateMagicWeight;
            }
            score += resourceScore;

            // 2️⃣ 职业系统
            if (haveProfession)
                score += _params.HaveProfessionBonus;

            if (playerHaveProfession && !haveProfession)
            {
                score += _params.EnemyHasProfessionPenalty;
                if (enemyIron - playerIron < _params.IronDeficitThreshold)
                    score += _params.IronDeficitPenaltyWhenEnemyHasProfession;
            }

            if (!playerHaveProfession && !haveProfession)
            {
                if (enemyIron - playerIron < 0)
                    score += _params.IronDeficitPenaltyWhenBothNoProfession;
            }

            // 3️⃣ 攻击策略
            double hpDiff = enemyHP - playerHP;
            if (!haveProfession)
            {
                if (early)
                    score -= (100 - playerHP) * _params.EarlyUnnecessaryAttackPenaltyMultiplier;
                else if (mid)
                {
                    if (hpDiff > _params.HpAdvantageThreshold)
                        score += hpDiff * _params.MidAdvantageAttackBonusMultiplier;
                    else
                        score -= (100 - playerHP) * _params.MidUnnecessaryAttackPenaltyMultiplier;
                }
            }
            else
            {
                score += (100 - playerHP) * _params.WithProfessionDamageBonusMultiplier;
                score += hpDiff * _params.WithProfessionHpDiffBonusMultiplier;
            }

            // 4️⃣ 回合节奏
            if (early)
                score += round * _params.EarlyRoundBonusPerRound;
            else if (late)
                score -= round * _params.LateRoundPenaltyPerRound;

            return score;
        }

        private bool IsTerminal(GameInstance state)
        {
            return state.Enemy.Focus.Health.HP <= 0 ||
                    state.Player.Focus.Health.HP <= 0;
        }

        private (string, int) RandomAction(ActorSet actor, GameInstance instance)
        {
            var actions = GetAllAvailable(actor, instance);
            if (actions.Count == 0)
                return ("", 0);
            return actions[_random.Value!.Next(actions.Count)];
        }

        private List<(string, int)> GetAllAvailable(ActorSet actor, GameInstance instance)
        {
            List<(string, int)> res = new();
            var names = actor.Focus.Skill.GetAvailableSkillNames();

            foreach (var name in names)
            {
                var useless = new List<string>() { "stick", "drill", "recovery", "shield", "thornshield", "mute" };
                if (useless.Contains(name))
                    continue;

                for (int i = 0; i <= 5; i++)
                {
                    if (name != "magicattack" && name != "spaceattack" && i > 0)
                        break;

                    SkillDeclareResult r = (actor == instance.Player)
                        ? instance.TryDeclare(name, i)
                        : instance.ETryDeclare(name, i);

                    if (r == SkillDeclareResult.Success)
                        res.Add((name, i));
                    else if (i > 0)
                        break;
                }
            }
            return res;
        }
    }
}