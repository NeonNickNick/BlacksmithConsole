using BlacksmithCore.AI.Strategies;
namespace BlacksmithCore.AI.Optimizer
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

                    var child = p1.GetCrossWith(_rand, p2);

                    if (_rand.NextDouble() < MutationRate)
                        child = child.GetMutation(_rand, MutationScale);

                    next.Add(child);
                }

                population = next;

                Console.WriteLine($"Gen {gen} best score: {scored.Max(x => x.score)}");
            }

            return EvaluatePopulation(population)
                .OrderByDescending(x => x.score)
                .First().param;
        }
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
        private GeneralStrategyParams TournamentSelect(List<(GeneralStrategyParams param, int score)> pop)
        {
            var candidates = new List<(GeneralStrategyParams, int)>();

            for (int i = 0; i < TournamentSize; i++)
                candidates.Add(pop[_rand.Next(pop.Count)]);

            return candidates.OrderByDescending(x => x.Item2).First().Item1;
        }
        private List<GeneralStrategyParams> InitPopulation()
        {
            var list = new List<GeneralStrategyParams>();

            for (int i = 0; i < PopulationSize; i++)
            {
                var p = new GeneralStrategyParams();
                p = p.GetMutation(_rand, MutationScale); // 从默认值扰动
                list.Add(p);
            }

            return list;
        }
    }
}