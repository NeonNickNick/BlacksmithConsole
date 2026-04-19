using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.FrontendBackendInterface;


namespace Blacksmith.AI.Strategies
{
    public class GeneralStrategy : IAIStrategy
    {
        public string Name => "General";

        private GameInstance _main = null!;
        private Random _random = new Random();

        // MCTS 参数
        private const int MaxIterations = 5000;
        private const int RolloutDepth = 20;
        private const double UctConstant = 1.414;

        public void Init(GameInstance gameInstance)
        {
            _main = gameInstance;
        }

        public (string skillName, int param) ChooseSkill(ActorSet self, ActorSet opponent)
        {
            var rootState = _main.DeepCopy();

            var rootActions = GetAllAvailable(rootState.Enemy, rootState);
            if (rootActions.Count == 0)
                return ("", 0);

            var root = new MCTSNode(rootState, null, rootActions);

            for (int i = 0; i < MaxIterations; i++)
            {
                var node = root;

                // 1. Selection
                while (node.UntriedActions.Count == 0 && node.Children.Count > 0)
                {
                    node = Select(node);
                }

                // 2. Expansion
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

                    var child = new MCTSNode(nextState, node, nextActions);
                    child.Action = action;

                    // ⭐ 提前注入一点局面价值（降低随机性影响）
                    child.Wins += Evaluate(nextState) * 0.2;

                    node.Children.Add(child);
                    node = child;
                }

                // 3. Simulation（仍然走几步，但最终用Evaluate）
                var simState = node.State.DeepCopy();

                for (int d = 0; d < RolloutDepth; d++)
                {
                    if (IsTerminal(simState))
                        break;

                    var p = RandomAction(simState.Player, simState);
                    var e = RandomAction(simState.Enemy, simState);

                    simState.Declare(p.Item1, p.Item2, e.Item1, e.Item2);
                }

                double result = Evaluate(simState);

                // 4. Backpropagation
                while (node != null)
                {
                    node.Visits++;
                    node.Wins += result;
                    node = node.Parent!;
                }
            }

            return SampleFromTopK(root.Children, root.State.History.SkillHistory.Count);
        }
        private (string, int) SampleFromTopK(List<MCTSNode> children, int round)
        {
            int k = Math.Min(3, children.Count); // Top-K，可调
            double temperature = Math.Max(0, 0.03 * round);            // 温度，越低越贪心

            // 按 Q 值排序（比 Visits 更稳定）
            var topK = children
                .OrderByDescending(c => c.Wins / (c.Visits + 1e-6))
                .Take(k)
                .ToList();

            // Softmax
            double maxScore = topK.Max(c => c.Wins / (c.Visits + 1e-6));

            List<double> weights = new();
            double sum = 0;

            foreach (var c in topK)
            {
                double q = c.Wins / (c.Visits + 1e-6);

                // 数值稳定 + 温度
                double w = Math.Exp((q - maxScore) / temperature);
                weights.Add(w);
                sum += w;
            }

            // 采样
            double r = _random.NextDouble() * sum;
            double acc = 0;

            for (int i = 0; i < topK.Count; i++)
            {
                acc += weights[i];
                if (r <= acc)
                {
                    return topK[i].Action!.Value;
                }
            }

            return topK.Last().Action!.Value;
        }
        // =========================
        // MCTS 节点
        // =========================

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
                    UctConstant * Math.Sqrt(Math.Log(node.Visits + 1) / (child.Visits + 1e-6));
                return uct;
            }).First();
        }

        // =========================
        // 评估函数（核心）
        // =========================

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

            // ===== 终局 =====
            if (enemyHP <= 0) return -1e9;
            if (playerHP <= 0) return 1e9;

            double score = 0;

            // ===== 阶段划分 =====
            bool early = round < 7;
            bool mid = round >= 7 && round < 15;
            bool late = round >= 15;

            // =========================
            // 1️⃣ 资源系统（核心）
            // =========================

            double resourceScore = 0;

            if (early)
            {
                // 强资源导向
                resourceScore += enemyIron * 1200 + Math.Max(0, enemyIron - 4) * 1200;
                resourceScore += enemySpace * 4000;
                resourceScore += enemyTime * 3500;
                resourceScore += enemyMagic * 2000;

                // ❗ 防止囤积过量（边际递减）
                resourceScore -= Math.Max(0, enemyIron - 5) * 80;
            }
            else if (mid)
            {
                // 资源仍重要，但降低权重
                resourceScore += enemyIron * 60;
                resourceScore += enemySpace * 200;
                resourceScore += enemyTime * 180;
                resourceScore += enemyMagic * 120;
            }
            else // late
            {
                // 后期资源价值很低（鼓励用掉）
                resourceScore += enemyIron * 20;
                resourceScore += enemySpace * 50;
                resourceScore += enemyTime * 50;
                resourceScore += enemyMagic * 30;
            }

            score += resourceScore;

            // =========================
            // 2️⃣ 职业系统（关键战略点）
            // =========================

            if (haveProfession)
            {
                score += 500; // 巨大优势
            }

            if (playerHaveProfession && !haveProfession)
            {
                score -= 800; // 强惩罚

                // 如果资源还不够反制 → 更糟
                if (enemyIron - playerIron < 2)
                {
                    score -= 300;
                }
            }
            if (!playerHaveProfession && !haveProfession)
            {
                if (enemyIron - playerIron < 0)
                {
                    score -= 1000;
                }
            }

            // =========================
            // 3️⃣ 攻击策略（严格约束）
            // =========================

            double hpDiff = enemyHP - playerHP;

            if (!haveProfession)
            {
                // ❗ 未拿职业：禁止无意义攻击
                if (early)
                {
                    // 早期：打人是负收益
                    score -= (100 - playerHP) * 30;
                }
                else if (mid)
                {
                    // 中期：只有明显优势才允许攻击
                    if (hpDiff > 20)
                    {
                        score += hpDiff * 2;
                    }
                    else
                    {
                        score -= (100 - playerHP) * 10;
                    }
                }
            }
            else
            {
                // 有职业：全面进攻
                score += (100 - playerHP) * 20;
                score += hpDiff * 5;
            }

            // =========================
            // 4️⃣ 回合节奏（防止拖延）
            // =========================

            if (early)
            {
                score += round * 1; // 轻微
            }
            else if (mid)
            {
                
            }
            else
            {
                score -= round * 4; // 后期必须结束
            }

            return score;
        }


        private bool IsTerminal(GameInstance state)
        {
            return state.Enemy.Focus.Health.HP <= 0 ||
                   state.Player.Focus.Health.HP <= 0;
        }

        // =========================
        // 动作生成（无污染版）
        // =========================

        private (string, int) RandomAction(ActorSet actor, GameInstance instance)
        {
            var actions = GetAllAvailable(actor, instance);
            if (actions.Count == 0)
                return ("", 0);

            return actions[_random.Next(actions.Count)];
        }

        private List<(string, int)> GetAllAvailable(ActorSet actor, GameInstance instance)
        {
            List<(string, int)> res = new();

            var names = actor.Focus.Skill.GetAvailableSkillNames();

            foreach (var name in names)
            {
                var useless = new List<string>()
                {
                    "stick",
                    "drill",
                    "recovery",
                    "shield",
                    "thornshield",
                    "mute"
                };
                if (useless.Contains(name))
                {
                    continue;
                }
                for (int i = 0; i <= 5; i++)
                {
                    if(name != "magicattack" && name != "spaceattack" && i > 0)
                    {
                        break;
                    }
                    SkillDeclareResult r;

                    if (actor == instance.Player)
                        r = instance.TryDeclare(name, i);
                    else
                        r = instance.ETryDeclare(name, i);

                    if (r == SkillDeclareResult.Success)
                    {
                        res.Add((name, i));
                    }
                    else if (i > 0)
                    {
                        break;
                    }
                }
            }

            return res;
        }
    }
}