using System.Text;
using Blacksmith.AI;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.FrontendBackendInterface;

namespace Blacksmith.Frontend
{
    public static class LocalHost
    {
        public static void Start(List<IAIStrategy> availableStrategies)
        { 

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                ContentRootPath = Directory.GetCurrentDirectory()
            });

            builder.WebHost.UseUrls("http://localhost:5000");
            var app = builder.Build();

            Console.WriteLine($"WebRootPath: {app.Environment.WebRootPath}");
            Console.WriteLine($"ContentRootPath: {app.Environment.ContentRootPath}");

            app.UseDefaultFiles();
            app.UseStaticFiles();

            BackendStarter backendStarter = new();
            GameInstance? gameInstance = null;
            IAIStrategy? selectedAI = null;
            bool isManualMode = true;

            app.MapGet("/api/strategies", () =>
            {
                var list = new List<object>();
                list.Add(new { id = 1, name = "Manual" });
                for (int i = 0; i < availableStrategies.Count; i++)
                {
                    list.Add(new { id = i + 2, name = availableStrategies[i].Name });
                }
                return Results.Json(list);
            });

            app.MapPost("/api/start", async (HttpContext ctx) =>
            {
                var dto = await ctx.Request.ReadFromJsonAsync<StartDto>();
                gameInstance = backendStarter.StartBackend();
                selectedAI = null;
                isManualMode = true;
                if (dto != null)
                {
                    if (dto.mode >= 2 && dto.mode <= availableStrategies.Count + 1)
                    {
                        selectedAI = availableStrategies[dto.mode - 2];
                        selectedAI.Init(gameInstance);
                        isManualMode = false;
                    }
                }
                return Results.Json(new { ok = true, manual = isManualMode, ai = selectedAI?.Name });
            });

            app.MapPost("/api/declare", async (HttpContext ctx) =>
            {
                if (gameInstance == null)
                {
                    return Results.Json(new { ok = false, message = "Game not started" });
                }
                if (gameInstance.Player.Focus.Health.HP <= 0 && gameInstance.Enemy.Focus.Health.HP <= 0)
                {
                    return Results.Json(new { ok = false, message = "It's a draw!" });
                }
                else if (gameInstance.Player.Focus.Health.HP <= 0)
                {
                    return Results.Json(new { ok = false, message = "You lose!" });
                }
                else if (gameInstance.Enemy.Focus.Health.HP <= 0)
                {
                    return Results.Json(new { ok = false, message = "You win!" });
                }
                var dto = await ctx.Request.ReadFromJsonAsync<DeclareDto>();
                if (dto == null || string.IsNullOrWhiteSpace(dto.skillName))
                {
                    return Results.Json(new { ok = false, message = "Invalid input" });
                }

                string skillName = dto.skillName.Trim();
                int param = dto.param >= 0 ? dto.param : 0;

                var tryRes = gameInstance.TryDeclare(skillName, param);
                if (tryRes != SkillDeclareResult.Success)
                {
                    string reason = tryRes.ToString();
                    return Results.Json(new { ok = false, message = $"Player skill invalid: {reason}" });
                }

                string esn = dto.esn ?? "iron";
                int ep = dto.ep >= 0 ? dto.ep : 0;

                if (!isManualMode && selectedAI != null)
                {
                    var (aiSkill, aiParam) = selectedAI.ChooseSkill(gameInstance.Enemy, gameInstance.Player);
                    esn = aiSkill;
                    ep = aiParam;
                    var check = gameInstance.ETryDeclare(esn, ep);
                    if (check != SkillDeclareResult.Success)
                    {
                        esn = "iron";
                        ep = 0;
                        gameInstance.ETryDeclare(esn, ep);
                    }
                }
                else
                {
                    var check = gameInstance.ETryDeclare(esn, ep);
                    if (check != SkillDeclareResult.Success)
                    {
                        return Results.Json(new { ok = false, message = $"Enemy skill invalid: {check}" });
                    }
                }

                gameInstance.Declare(skillName, param, esn, ep);

                var log = LogInfo(gameInstance.Player.Focus, gameInstance.Enemy.Focus);

                string resultMessage = "next";
                if (gameInstance.Player.Focus.Health.HP <= 0 && gameInstance.Enemy.Focus.Health.HP <= 0)
                {
                    resultMessage = "draw";
                }
                else if (gameInstance.Player.Focus.Health.HP <= 0)
                {
                    resultMessage = "lose";
                }
                else if (gameInstance.Enemy.Focus.Health.HP <= 0)
                {
                    resultMessage = "win";
                }

                return Results.Json(new
                {
                    ok = true,
                    message = $"Player used: {skillName} {param}; Enemy used: {esn} {ep}",
                    log,
                    result = resultMessage,
                    player = new { hp = gameInstance.Player.Focus.Health.HP, mhp = gameInstance.Player.Focus.Health.MHP },
                    enemy = new { hp = gameInstance.Enemy.Focus.Health.HP, mhp = gameInstance.Enemy.Focus.Health.MHP }
                });
            });

            app.MapGet("/api/status", () =>
            {
                if (gameInstance == null)
                {
                    return Results.Json(new { ok = false, message = "Game not started" });
                }
                var log = LogInfo(gameInstance.Player.Focus, gameInstance.Enemy.Focus);
                return Results.Json(new
                {
                    ok = true,
                    log,
                    player = new { hp = gameInstance.Player.Focus.Health.HP, mhp = gameInstance.Player.Focus.Health.MHP },
                    enemy = new { hp = gameInstance.Enemy.Focus.Health.HP, mhp = gameInstance.Enemy.Focus.Health.MHP }
                });
            });

            Console.WriteLine("Starting local web host at http://localhost:5000/");
            app.Run();
        }

        private static string LogInfo(Body player, Body enemy)
        {
            var sb = new StringBuilder();
            sb.AppendLine("-------------------------------------------------------------------------------------------------");
            sb.AppendLine("You:");
            sb.AppendLine($"HP: {player.Health.HP}/{player.Health.MHP}");
            try
            {
                sb.AppendLine($"Resource: Iron: {player.Resource.QueryCommon(ResourceType.Instance.Iron())}/{player.Resource.QueryGold(ResourceType.Instance.Iron())}  Space: {player.Resource.QueryCommon(ResourceType.Instance.Space())}  Time: {player.Resource.QueryCommon(ResourceType.Instance.Time())}  Magic: {player.Resource.QueryCommon(ResourceType.Instance.Magic())}");
            }
            catch { }
            string ds = "";
            try
            {
                player.Defense.Defenses.ForEach(d => ds += $"{d.Type}: {d.Power}  ");
                if (ds != "") sb.AppendLine(ds);
            }
            catch { }

            sb.AppendLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            sb.AppendLine("Robert:");
            sb.AppendLine($"HP: {enemy.Health.HP}/{enemy.Health.MHP}");
            try
            {
                sb.AppendLine($"Resource: Iron: {enemy.Resource.QueryCommon(ResourceType.Instance.Iron())}/{enemy.Resource.QueryGold(ResourceType.Instance.Iron())}  Space: {enemy.Resource.QueryCommon(ResourceType.Instance.Space())}  Time: {enemy.Resource.QueryCommon(ResourceType.Instance.Time())}  Magic: {enemy.Resource.QueryCommon(ResourceType.Instance.Magic())}");
            }
            catch { }
            ds = "";
            try
            {
                enemy.Defense.Defenses.ForEach(d => ds += $"{d.Type}: {d.Power}  ");
                if (ds != "") sb.AppendLine(ds);
            }
            catch { }
            sb.AppendLine("-------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }

        private class StartDto
        {
            public int mode { get; set; }
        }
        private class DeclareDto
        {
            public string? skillName { get; set; }
            public int param { get; set; }
            public string? esn { get; set; }
            public int ep { get; set; }
        }
    }
}
