using System.Text;
using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.FrontendBackendInterface;
using Blacksmith.FrontEndBackendInterface;
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
            Console.WriteLine("sys::out >> /exit : Quit game.");
            Console.WriteLine();
            while (true)
            {
                string skillName = "";
                int param = 0;
                {
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
                                if (param < 0)
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
                                ifSucceed = true;
                                break;
                        }
                    } 
                }
                //临时：改为控制台依次输入玩家和人机技能
                //Console.WriteLine($"\nsys::out >> \"Robert\" is thinking...\n");
                Console.WriteLine($"\nsys::out >> Tell \"Robert\" what to do...\n");
                string esn = "";
                int ep = 0;
                {
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
                        esn = slices[0];
                        ep = 0;
                        if (slices.Length > 1)
                        {
                            try
                            {
                                ep = int.Parse(slices[1]);
                                if (ep < 0)
                                {
                                    Console.WriteLine($"\nsys::out >> Invalid ep \"{slices[1]}\" is not positive!\n");
                                    continue;
                                }
                            }
                            catch
                            {
                                Console.WriteLine($"\nsys::out >> Invalid ep \"{slices[1]}\" is not an integer!\n");
                                continue;
                            }
                        }
                        if (esn.StartsWith("/exit"))
                        {
                            Console.WriteLine("\nsys::out >> Goodbye!\n");
                            return;
                        }
                        var result = gameContext.SkillChoose.ETryDeclare(esn, ep);
                        switch (result)
                        {
                            case SkillDeclareResult.Illegal:
                                Console.WriteLine($"\nsys::out >> Cannot find skill \"{esn}\"! Locked or non-existent!\n");
                                continue;
                            case SkillDeclareResult.Rejected:
                                Console.WriteLine($"\nsys::out >> The conditions for using the skill \"{esn}\" are not met!\n");
                                continue;
                            case SkillDeclareResult.Success:
                                Console.WriteLine($"\nsys::out >> \"Robert\" is thinking...\n");
                                ifSucceed = true;
                                break;
                        }
                    }
                }
                gameContext.SkillChoose.Declare(skillName, param, esn, ep);
                //gameContext.SkillChoose.Declare(skillName, param);

                LogInfo(gameContext.Player.Focus, gameContext.Enemy.Focus);

                if (gameContext.Player.Focus.Health.HP <= 0 && gameContext.Enemy.Focus.Health.HP <= 0)
                {
                    Console.WriteLine($"\nsys::out >> It's a draw!\n");
                    return;
                }
                else if (gameContext.Player.Focus.Health.HP <= 0)
                {
                    Console.WriteLine($"\nsys::out >> You lose!\n");
                    return;
                }
                else if (gameContext.Enemy.Focus.Health.HP <= 0)
                {
                    Console.WriteLine($"\nsys::out >> You win!\n");
                    return;
                }
                else
                {
                    Console.WriteLine($"\nsys::out >> Next round!\n");
                }
            }
        }
        private static void LogInfo(Body player, Body enemy)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"""
                -------------------------------------------------------------------------------------------------
                You:
                HP: {player.Health.HP}/{player.Health.MHP}
                Resource: Iron: {player.Resource.QueryCommon(ResourceType.Iron)}/{player.Resource.QueryGold(ResourceType.Iron)}  Space: {player.Resource.QueryCommon(ResourceType.Space)}  Time: {player.Resource.QueryCommon(ResourceType.Time)}
                """);
            string ds = $"";
            player.Defense.Defenses.ForEach(d => ds += $"{d.Type}: {d.Power}  ");
            if (ds != "")
            {
                sb.AppendLine(ds);
            }
            sb.AppendLine($"""

                +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                Robert:
                HP: {enemy.Health.HP}/{enemy.Health.MHP}
                Resource: Iron: {enemy.Resource.QueryCommon(ResourceType.Iron)}/{enemy.Resource.QueryGold(ResourceType.Iron)}  Space: {enemy.Resource.QueryCommon(ResourceType.Space)}  Time: {enemy.Resource.QueryCommon(ResourceType.Time)}
                """);
            ds = $"";
            enemy.Defense.Defenses.ForEach(d => ds += $"{d.Type}: {d.Power}  ");
            if (ds != "")
            {
                sb.AppendLine(ds);
            }
            sb.AppendLine($"""

                -------------------------------------------------------------------------------------------------
                """);
            Console.WriteLine($"\n{sb.ToString()}\n");
        }
    }
}
