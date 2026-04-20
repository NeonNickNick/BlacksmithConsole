using System.Text.Json.Serialization;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.FrontendBackendInterface;


namespace Blacksmith.AI.Strategies
{
    public class GeneralStrategyParams
    {
        [JsonConstructor]
        public GeneralStrategyParams()
        {

        }
        public GeneralStrategyParams(GeneralStrategyParams other)
        {
            TemperatureCoefficient = other.TemperatureCoefficient;
            WinScore = other.WinScore;
            LoseScore = other.LoseScore;
            PlayerResourceEnemyHPRatio = other.PlayerResourceEnemyHPRatio;
            EnemyResourcePlayerHPRatio = other.EnemyResourcePlayerHPRatio;
            EarlyIronWeight = other.EarlyIronWeight;
            EarlyExcessIronWeight = other.EarlyExcessIronWeight;
            EarlySpaceWeight = other.EarlySpaceWeight;
            EarlyTimeWeight = other.EarlyTimeWeight;
            EarlyDefaultWeight = other.EarlyDefaultWeight;
            EarlyIronOverstockPenalty = other.EarlyIronOverstockPenalty;
            MidIronWeight = other.MidIronWeight;
            MidSpaceWeight = other.MidSpaceWeight;
            MidTimeWeight = other.MidTimeWeight;
            MidDefaultWeight = other.MidDefaultWeight;
            LateIronWeight = other.LateIronWeight;
            LateSpaceWeight = other.LateSpaceWeight;
            LateTimeWeight = other.LateTimeWeight;
            LateDefaultWeight = other.LateDefaultWeight;
            HaveProfessionBonus = other.HaveProfessionBonus;
            IronDeficitPenaltyWhenEnemyHasProfession = other.IronDeficitPenaltyWhenEnemyHasProfession;
            IronDeficitPenaltyWhenBothNoProfession = other.IronDeficitPenaltyWhenBothNoProfession;
            IronDeficitThreshold = other.IronDeficitThreshold;
            EarlyUnnecessaryAttackPenaltyMultiplier = other.EarlyUnnecessaryAttackPenaltyMultiplier;
            MidAdvantageAttackBonusMultiplier = other.MidAdvantageAttackBonusMultiplier;
            MidUnnecessaryAttackPenaltyMultiplier = other.MidUnnecessaryAttackPenaltyMultiplier;
            WithProfessionDamageBonusMultiplier = other.WithProfessionDamageBonusMultiplier;
            WithProfessionHpDiffBonusMultiplier = other.WithProfessionHpDiffBonusMultiplier;
            HpAdvantageThreshold = other.HpAdvantageThreshold;
            EarlyRoundBonusPerRound = other.EarlyRoundBonusPerRound;
            LateRoundPenaltyPerRound = other.LateRoundPenaltyPerRound;
        }

        public GeneralStrategyParams GetMutation(Random rand, double MutationScale)
        {
            double Mut(double v)
            {
                double noise = (rand.NextDouble() * 2 - 1);
                return v * (1 + noise * MutationScale);
            }
            GeneralStrategyParams res = new(this);
            // 补充变异逻辑
            res.TemperatureCoefficient = Mut(TemperatureCoefficient);
            res.WinScore = Mut(WinScore);
            res.LoseScore = Mut(LoseScore);
            res.PlayerResourceEnemyHPRatio = Mut(PlayerResourceEnemyHPRatio);
            res.EnemyResourcePlayerHPRatio = Mut(EnemyResourcePlayerHPRatio);
            res.EarlyIronWeight = Mut(EarlyIronWeight);
            res.EarlyExcessIronWeight = Mut(EarlyExcessIronWeight);
            res.EarlySpaceWeight = Mut(EarlySpaceWeight);
            res.EarlyTimeWeight = Mut(EarlyTimeWeight);
            res.EarlyDefaultWeight = Mut(EarlyDefaultWeight);
            res.EarlyIronOverstockPenalty = Mut(EarlyIronOverstockPenalty);
            res.MidIronWeight = Mut(MidIronWeight);
            res.MidSpaceWeight = Mut(MidSpaceWeight);
            res.MidTimeWeight = Mut(MidTimeWeight);
            res.MidDefaultWeight = Mut(MidDefaultWeight);
            res.LateIronWeight = Mut(LateIronWeight);
            res.LateSpaceWeight = Mut(LateSpaceWeight);
            res.LateTimeWeight = Mut(LateTimeWeight);
            res.LateDefaultWeight = Mut(LateDefaultWeight);
            res.HaveProfessionBonus = Mut(HaveProfessionBonus);
            res.IronDeficitPenaltyWhenEnemyHasProfession = Mut(IronDeficitPenaltyWhenEnemyHasProfession);
            res.IronDeficitPenaltyWhenBothNoProfession = Mut(IronDeficitPenaltyWhenBothNoProfession);
            res.IronDeficitThreshold = Mut(IronDeficitThreshold);
            res.EarlyUnnecessaryAttackPenaltyMultiplier = Mut(EarlyUnnecessaryAttackPenaltyMultiplier);
            res.MidAdvantageAttackBonusMultiplier = Mut(MidAdvantageAttackBonusMultiplier);
            res.MidUnnecessaryAttackPenaltyMultiplier = Mut(MidUnnecessaryAttackPenaltyMultiplier);
            res.WithProfessionDamageBonusMultiplier = Mut(WithProfessionDamageBonusMultiplier);
            res.WithProfessionHpDiffBonusMultiplier = Mut(WithProfessionHpDiffBonusMultiplier);
            res.HpAdvantageThreshold = Mut(HpAdvantageThreshold);
            res.EarlyRoundBonusPerRound = Mut(EarlyRoundBonusPerRound);
            res.LateRoundPenaltyPerRound = Mut(LateRoundPenaltyPerRound);
            return res;
        }

        public GeneralStrategyParams GetCrossWith(Random rand, GeneralStrategyParams other)
        {
            double Pick(double a, double b)
            {
                return rand.NextDouble() < 0.5 ? a : b;
            }
            GeneralStrategyParams res = new();
            // 补充交叉逻辑
            res.TemperatureCoefficient = Pick(TemperatureCoefficient, other.TemperatureCoefficient);
            res.WinScore = Pick(WinScore, other.WinScore);
            res.LoseScore = Pick(LoseScore, other.LoseScore);
            res.PlayerResourceEnemyHPRatio = Pick(PlayerResourceEnemyHPRatio, other.PlayerResourceEnemyHPRatio);
            res.EnemyResourcePlayerHPRatio = Pick(EnemyResourcePlayerHPRatio, other.EnemyResourcePlayerHPRatio);
            res.EarlyIronWeight = Pick(EarlyIronWeight, other.EarlyIronWeight);
            res.EarlyExcessIronWeight = Pick(EarlyExcessIronWeight, other.EarlyExcessIronWeight);
            res.EarlySpaceWeight = Pick(EarlySpaceWeight, other.EarlySpaceWeight);
            res.EarlyTimeWeight = Pick(EarlyTimeWeight, other.EarlyTimeWeight);
            res.EarlyDefaultWeight = Pick(EarlyDefaultWeight, other.EarlyDefaultWeight);
            res.EarlyIronOverstockPenalty = Pick(EarlyIronOverstockPenalty, other.EarlyIronOverstockPenalty);
            res.MidIronWeight = Pick(MidIronWeight, other.MidIronWeight);
            res.MidSpaceWeight = Pick(MidSpaceWeight, other.MidSpaceWeight);
            res.MidTimeWeight = Pick(MidTimeWeight, other.MidTimeWeight);
            res.MidDefaultWeight = Pick(MidDefaultWeight, other.MidDefaultWeight);
            res.LateIronWeight = Pick(LateIronWeight, other.LateIronWeight);
            res.LateSpaceWeight = Pick(LateSpaceWeight, other.LateSpaceWeight);
            res.LateTimeWeight = Pick(LateTimeWeight, other.LateTimeWeight);
            res.LateDefaultWeight = Pick(LateDefaultWeight, other.LateDefaultWeight);
            res.HaveProfessionBonus = Pick(HaveProfessionBonus, other.HaveProfessionBonus);
            res.IronDeficitPenaltyWhenEnemyHasProfession = Pick(IronDeficitPenaltyWhenEnemyHasProfession, other.IronDeficitPenaltyWhenEnemyHasProfession);
            res.IronDeficitPenaltyWhenBothNoProfession = Pick(IronDeficitPenaltyWhenBothNoProfession, other.IronDeficitPenaltyWhenBothNoProfession);
            res.IronDeficitThreshold = Pick(IronDeficitThreshold, other.IronDeficitThreshold);
            res.EarlyUnnecessaryAttackPenaltyMultiplier = Pick(EarlyUnnecessaryAttackPenaltyMultiplier, other.EarlyUnnecessaryAttackPenaltyMultiplier);
            res.MidAdvantageAttackBonusMultiplier = Pick(MidAdvantageAttackBonusMultiplier, other.MidAdvantageAttackBonusMultiplier);
            res.MidUnnecessaryAttackPenaltyMultiplier = Pick(MidUnnecessaryAttackPenaltyMultiplier, other.MidUnnecessaryAttackPenaltyMultiplier);
            res.WithProfessionDamageBonusMultiplier = Pick(WithProfessionDamageBonusMultiplier, other.WithProfessionDamageBonusMultiplier);
            res.WithProfessionHpDiffBonusMultiplier = Pick(WithProfessionHpDiffBonusMultiplier, other.WithProfessionHpDiffBonusMultiplier);
            res.HpAdvantageThreshold = Pick(HpAdvantageThreshold, other.HpAdvantageThreshold);
            res.EarlyRoundBonusPerRound = Pick(EarlyRoundBonusPerRound, other.EarlyRoundBonusPerRound);
            res.LateRoundPenaltyPerRound = Pick(LateRoundPenaltyPerRound, other.LateRoundPenaltyPerRound);
            return res;
        }
        public double TemperatureCoefficient  = 0.03; // 原 0.03 * round

        // 终局奖励/惩罚
        public double WinScore  = 1e9;
        public double LoseScore  = -1e9;

        //交叉权重
        public double PlayerResourceEnemyHPRatio = 100;
        public double EnemyResourcePlayerHPRatio = 100;

        // ========== 早期资源权重 ==========
        public double EarlyIronWeight  = 1200;
        public double EarlyExcessIronWeight  = 1200;     // 超过4铁时的额外奖励
        public double EarlySpaceWeight  = 4000;
        public double EarlyTimeWeight  = 3500;
        public double EarlyDefaultWeight = 2000;
        public double EarlyIronOverstockPenalty  = 80;   // 超过7铁的惩罚

        // ========== 中期资源权重 ==========
        public double MidIronWeight  = 300;
        public double MidSpaceWeight  = 1000;
        public double MidTimeWeight  = 900;
        public double MidDefaultWeight = 600;

        // ========== 后期资源权重 ==========
        public double LateIronWeight  = 100;
        public double LateSpaceWeight  = 250;
        public double LateTimeWeight  = 250;
        public double LateDefaultWeight = 150;

        // ========== 职业相关 ==========
        public double HaveProfessionBonus  = 500;
        public double IronDeficitPenaltyWhenEnemyHasProfession  = 300;
        public double IronDeficitPenaltyWhenBothNoProfession  = 1000;
        public double IronDeficitThreshold  = 3;         // 铁差阈值

        // ========== 攻击策略权重 ==========
        public double EarlyUnnecessaryAttackPenaltyMultiplier  = 30;    // (100 - HP) * 30
        public double MidAdvantageAttackBonusMultiplier  = 2;           // hpDiff * 2
        public double MidUnnecessaryAttackPenaltyMultiplier  = 10;      // (100 - HP) * 10
        public double WithProfessionDamageBonusMultiplier  = 20;        // (100 - HP) * 20
        public double WithProfessionHpDiffBonusMultiplier  = 5;         // hpDiff * 5
        public double HpAdvantageThreshold  = 20;                       // hpDiff > 20 才算优势

        // ========== 回合节奏 ==========
        public double EarlyRoundBonusPerRound  = 1;
        public double LateRoundPenaltyPerRound  = 40;
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
            double playerSpace = player.Focus.Resource.QueryAll(ResourceType.Instance.Space());
            double playerMagic = player.Focus.Resource.QueryAll(ResourceType.Instance.Magic());

            bool haveProfession = enemy.Focus.Skill.HaveProfession;
            bool playerHaveProfession = player.Focus.Skill.HaveProfession;

            int round = state.History.SkillHistory.Count;//已经过的回合数

            // 终局
            if (enemyHP <= 0) return _params.LoseScore;
            if (playerHP <= 0) return _params.WinScore;

            double score = 0;

            // 阶段划分
            bool early = round < 10;
            bool mid = round >= 10 && round < 15;
            bool late = round >= 15;

            //0 交叉关注
            //引入双方铁和生命值之间的关系
            score -= 10 * 
                ((playerIron + 3 * playerSpace + 2 * playerMagic) / (enemyHP + 1e-6 )) * 
                    _params.PlayerResourceEnemyHPRatio;
            
            score += 10 * 
                ((enemyIron + 3 * enemySpace + 2 * enemyMagic) / (playerHP + 1e-6)) *
                    _params.EnemyResourcePlayerHPRatio;

            // 1️⃣ 资源系统
            double resourceScore = 0;
            if (early)
            {
                resourceScore += enemyIron * _params.EarlyIronWeight;
                resourceScore += Math.Max(0, enemyIron - 4) * _params.EarlyExcessIronWeight;
                resourceScore += enemySpace * _params.EarlySpaceWeight;
                resourceScore += enemyTime * _params.EarlyTimeWeight;
                resourceScore += enemyMagic * _params.EarlyDefaultWeight;
                resourceScore -= Math.Max(0, enemyIron - 7) * _params.EarlyIronOverstockPenalty;
            }
            else if (mid)
            {
                resourceScore += enemyIron * _params.MidIronWeight;
                resourceScore += enemySpace * _params.MidSpaceWeight;
                resourceScore += enemyTime * _params.MidTimeWeight;
                resourceScore += enemyMagic * _params.MidDefaultWeight;
            }
            else // late
            {
                resourceScore += enemyIron * _params.LateIronWeight;
                resourceScore += enemySpace * _params.LateSpaceWeight;
                resourceScore += enemyTime * _params.LateTimeWeight;
                resourceScore += enemyMagic * _params.LateDefaultWeight;
            }
            score += resourceScore;

            // 2️⃣ 职业系统
            if (haveProfession)//有职业就奖励
                score += _params.HaveProfessionBonus;

            if (playerHaveProfession && !haveProfession)
            {
                if (round >= 8)
                {
                    //如果已经过了8回合还没有职业，给严厉惩罚
                    score -= 5e8;
                }
                if (enemyIron - playerIron < _params.IronDeficitThreshold)
                {
                    score -= _params.IronDeficitPenaltyWhenEnemyHasProfession;
                }//如果玩家已经选了职业但是铁差小于3（可变）
            }

            if (!playerHaveProfession && !haveProfession)
            {
                if (enemyIron - playerIron < 0)
                    score -= _params.IronDeficitPenaltyWhenBothNoProfession;
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