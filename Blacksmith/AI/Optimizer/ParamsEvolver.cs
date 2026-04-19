using Blacksmith.AI.Strategies;
namespace Blacksmith.AI.Optimizer
{
    public class ParamsEvolver
    {
        private readonly Func<GeneralStrategyParams, GeneralStrategyParams, bool> _compete;
        private readonly Random _rand = new Random();

        public int PopulationSize { get; set; } = 4;
        public int EliteCount { get; set; } = 2;
        public double MutationRate { get; set; } = 0.2;
        public double MutationScale { get; set; } = 0.15;
        public int TournamentSize { get; set; } = 3;

        public ParamsEvolver(Func<GeneralStrategyParams, GeneralStrategyParams, bool> compete)
        {
            _compete = compete;
        }

        public GeneralStrategyParams Train(int generations)
        {
            var population = InitPopulation();

            for (int gen = 0; gen < generations; gen++)
            {
                Console.WriteLine($"Generation{gen}");
                var scored = EvaluatePopulation(population);

                var next = new List<GeneralStrategyParams>();

                // ✅ 精英保留
                var elites = scored
                    .OrderByDescending(x => x.score)
                    .Take(EliteCount)
                    .Select(x => x.param)
                    .ToList();

                next.AddRange(elites);

                // ✅ 生成新个体
                while (next.Count < PopulationSize)
                {
                    var p1 = TournamentSelect(scored);
                    var p2 = TournamentSelect(scored);

                    var child = Crossover(p1, p2);

                    if (_rand.NextDouble() < MutationRate)
                        child = Mutate(child);

                    next.Add(child);
                }

                population = next;

                Console.WriteLine($"Gen {gen} best score: {scored.Max(x => x.score)}");
            }

            return EvaluatePopulation(population)
                .OrderByDescending(x => x.score)
                .First().param;
        }

        // =========================
        // 评估
        // =========================
        private List<(GeneralStrategyParams param, int score)> EvaluatePopulation(List<GeneralStrategyParams> pop)
        {
            var result = new List<(GeneralStrategyParams, int)>();

            foreach (var p in pop)
            {
                int score = 0;

                // 每个个体打 K 场
                for (int i = 0; i < 10; i++)
                {
                    var opponent = pop[_rand.Next(pop.Count)];

                    if (_compete(p, opponent))
                        score++;
                }

                result.Add((p, score));
            }

            return result;
        }

        // =========================
        // 锦标赛选择
        // =========================
        private GeneralStrategyParams TournamentSelect(List<(GeneralStrategyParams param, int score)> pop)
        {
            var candidates = new List<(GeneralStrategyParams, int)>();

            for (int i = 0; i < TournamentSize; i++)
                candidates.Add(pop[_rand.Next(pop.Count)]);

            return candidates.OrderByDescending(x => x.Item2).First().Item1;
        }

        // =========================
        // 交叉（均匀交叉）
        // =========================
        private GeneralStrategyParams Crossover(GeneralStrategyParams a, GeneralStrategyParams b)
        {
            return new GeneralStrategyParams
            {
                TemperatureCoefficient = Pick(a.TemperatureCoefficient, b.TemperatureCoefficient),

                EarlyIronWeight = Pick(a.EarlyIronWeight, b.EarlyIronWeight),
                EarlySpaceWeight = Pick(a.EarlySpaceWeight, b.EarlySpaceWeight),
                EarlyTimeWeight = Pick(a.EarlyTimeWeight, b.EarlyTimeWeight),
                EarlyMagicWeight = Pick(a.EarlyMagicWeight, b.EarlyMagicWeight),

                MidIronWeight = Pick(a.MidIronWeight, b.MidIronWeight),
                MidSpaceWeight = Pick(a.MidSpaceWeight, b.MidSpaceWeight),
                MidTimeWeight = Pick(a.MidTimeWeight, b.MidTimeWeight),
                MidMagicWeight = Pick(a.MidMagicWeight, b.MidMagicWeight),

                LateIronWeight = Pick(a.LateIronWeight, b.LateIronWeight),
                LateSpaceWeight = Pick(a.LateSpaceWeight, b.LateSpaceWeight),
                LateTimeWeight = Pick(a.LateTimeWeight, b.LateTimeWeight),
                LateMagicWeight = Pick(a.LateMagicWeight, b.LateMagicWeight),

                HaveProfessionBonus = Pick(a.HaveProfessionBonus, b.HaveProfessionBonus),
                EnemyHasProfessionPenalty = Pick(a.EnemyHasProfessionPenalty, b.EnemyHasProfessionPenalty),
            };
        }

        private double Pick(double a, double b)
        {
            return _rand.NextDouble() < 0.5 ? a : b;
        }

        // =========================
        // 变异（乘法噪声）
        // =========================
        private GeneralStrategyParams Mutate(GeneralStrategyParams p)
        {
            double Mut(double v)
            {
                double noise = (_rand.NextDouble() * 2 - 1);
                return v * (1 + noise * MutationScale);
            }

            return new GeneralStrategyParams
            {
                TemperatureCoefficient = Mut(p.TemperatureCoefficient),

                EarlyIronWeight = Mut(p.EarlyIronWeight),
                EarlySpaceWeight = Mut(p.EarlySpaceWeight),
                EarlyTimeWeight = Mut(p.EarlyTimeWeight),
                EarlyMagicWeight = Mut(p.EarlyMagicWeight),

                MidIronWeight = Mut(p.MidIronWeight),
                MidSpaceWeight = Mut(p.MidSpaceWeight),
                MidTimeWeight = Mut(p.MidTimeWeight),
                MidMagicWeight = Mut(p.MidMagicWeight),

                LateIronWeight = Mut(p.LateIronWeight),
                LateSpaceWeight = Mut(p.LateSpaceWeight),
                LateTimeWeight = Mut(p.LateTimeWeight),
                LateMagicWeight = Mut(p.LateMagicWeight),

                HaveProfessionBonus = Mut(p.HaveProfessionBonus),
                EnemyHasProfessionPenalty = Mut(p.EnemyHasProfessionPenalty),
            };
        }

        // =========================
        // 初始化
        // =========================
        private List<GeneralStrategyParams> InitPopulation()
        {
            var list = new List<GeneralStrategyParams>();

            for (int i = 0; i < PopulationSize; i++)
            {
                var p = new GeneralStrategyParams();
                p = Mutate(p); // 从默认值扰动
                list.Add(p);
            }

            return list;
        }
    }
}