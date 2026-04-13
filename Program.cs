using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.FrontEndBackendInterface;
using Blacksmith.FrontendBackendInterface;
namespace Blacksmith.Program
{
    public class DefaultSkillContext : ISkillContext
    {
        public ActorSet Self { get; }
        public int Param { get; }
        public DefaultSkillContext(ActorSet self, int param)
        {
            Self = self;
            Param = param;
        }
    }
    public static class Program
    {
        public static void Main()
        {
            //从中间层启动后端
            BackendStarter backendStarter = new();
            GameContext gameContext = backendStarter.StartBackend();
            //打印提示
            Console.WriteLine("Welcome!");
            Console.WriteLine();
            Console.WriteLine("sys::out >> Usage : Skill Name (all lowercase) + Parameter (default is 0)");
            Console.WriteLine("sys::out >> e.g. Iron / Iron 1(useless) / shield 1/ shield (default)");
            Console.WriteLine();
            Console.WriteLine("sys::out >> /info : Print current game-context to console.");
            Console.WriteLine("sys::out >> /exit : Quit game.");
            Console.WriteLine();
            while (true)
            {
                string skillName = "";
                int param = 0;
                bool ifSucceed = false;
                while (!ifSucceed)
                {
                    Console.Write("sysin << ");
                    string? input = Console.ReadLine();
                    if (input == null || input == "")
                    {
                        continue;
                    }
                    var slices = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (slices.Length <= 0)
                    {
                        continue;
                    }
                    skillName = slices[0];
                    param = 0;
                    if (slices.Length > 1)
                    {
                        try
                        {
                            param = int.Parse(slices[1]);
                            if(param < 0)
                            {
                                Console.WriteLine($"\nsys::out >> Invalid param \"{slices[1]}\" is not positive!\n");
                                continue;
                            }
                        }
                        catch
                        {
                            Console.WriteLine($"\nsys::out >> Invalid param \"{slices[1]}\" is not an integer!\n");
                            continue;
                        }
                    }
                    if (skillName.StartsWith("/info"))
                    {
                        Console.WriteLine("\nsys::out >> This function is in process.\n");
                        continue;
                    }
                    if (skillName.StartsWith("/exit"))
                    {
                        Console.WriteLine("\nsys::out >> Goodbye!\n");
                        return;
                    }
                    var result = gameContext.SkillChoose.TryDeclare(skillName, param);
                    switch (result)
                    {
                        case SkillDeclareResult.Illegal:
                            Console.WriteLine($"\nsys::out >> Cannot find skill \"{skillName}\"! Locked or non-existent!\n");
                            continue;
                        case SkillDeclareResult.Rejected:
                            Console.WriteLine($"\nsys::out >> The conditions for using the skill \"{skillName}\" are not met!\n");
                            continue;
                        case SkillDeclareResult.Success:
                            Console.WriteLine($"\nsys::out >> \"Robert\" is thinking...\n");
                            ifSucceed = true;
                            break;
                    }
                }

                gameContext.SkillChoose.Declare(skillName, param);

                Console.WriteLine($"\nbe::out >> You : \nHP : {gameContext.PlayerActorSetState.HP} / {gameContext.PlayerActorSetState.MHP}\nIron : {gameContext.PlayerActorSetState.Iron}\n");
                Console.WriteLine($"\nbe::out >> Robert : \nHP : {gameContext.EnemyActorSetState.HP} / {gameContext.EnemyActorSetState.MHP}\nIron : {gameContext.EnemyActorSetState.Iron}\n");

                Console.WriteLine($"\nsys::out >> Next round!\n");
            }
        }
    }
}
