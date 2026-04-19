
using System.Text.Json;
using Blacksmith.AI.Optimizer;
using Blacksmith.AI.Strategies;
using Blacksmith.FrontendBackendInterface;

namespace Blacksmith.AI.NaturalSelection
{
    public static class Selector
    {
        public static void StartSelect()
        {
            var evolver = new ParamsEvolver(Compete);
            var best = evolver.Train(13);
            SaveToFile<GeneralStrategyParams>(best, "data.json");
            Console.WriteLine("训练完成");
        }
        public static void SaveToFile<T>(T data, string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }

        // 从 JSON 文件加载
        public static T LoadFromFile<T>(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
        private static bool Compete(GeneralStrategyParams a, GeneralStrategyParams b)
        {
            BackendStarter backendStarter = new();
            GameInstance gameInstance = backendStarter.StartBackend();
            var astrategy = new GeneralStrategy(a);
            astrategy.Init(gameInstance);
            var bstrategy = new GeneralStrategy(b);
            bstrategy.Init(gameInstance);

            while (End(gameInstance) == 0)
            {
                var atuple = astrategy.ChooseSkill(gameInstance.Player, gameInstance.Enemy);
                gameInstance.Swap();
                var btuple = bstrategy.ChooseSkill(gameInstance.Player, gameInstance.Enemy);
                gameInstance.Swap();
                gameInstance.Declare(btuple.Item1, btuple.Item2, atuple.Item1, atuple.Item2);
            }
            if (End(gameInstance) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static int End(GameInstance gameInstance)
        {
            if(gameInstance.Player.Focus.Health.HP <= 0)
            {
                return 1;
            }else if(gameInstance.Enemy.Focus.Health.HP <= 0)
            {
                return -1;
            }
            return 0;
        }
    }
}
