using BlacksmithCore.AI;

namespace BlacksmithClient.Frontend
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

            WebGameSession webGameSession = new(availableStrategies);

            app.MapGet("/api/strategies", () =>
            {
                return Results.Json(webGameSession.GetStrategies());
            });

            app.MapPost("/api/start", async (HttpContext ctx) =>
            {
                var dto = await ctx.Request.ReadFromJsonAsync<StartDto>();
                var snapshot = webGameSession.StartGame(dto?.mode ?? 1);
                return Results.Json(new { ok = true, snapshot });
            });

            app.MapPost("/api/declare", async (HttpContext ctx) =>
            {
                var dto = await ctx.Request.ReadFromJsonAsync<DeclareDto>();
                if (dto == null)
                {
                    return Results.Json(new { ok = false, message = "Invalid input", snapshot = webGameSession.GetSnapshot() });
                }

                var result = webGameSession.DeclareTurn(dto.skillName ?? string.Empty, dto.param, dto.esn ?? "iron", dto.ep);
                return Results.Json(new { ok = result.Ok, message = result.Message, snapshot = result.Snapshot });
            });

            app.MapGet("/api/status", () =>
            {
                return Results.Json(new { ok = true, snapshot = webGameSession.GetSnapshot() });
            });

            Console.WriteLine("Starting local web host at http://localhost:5000/");

            // 自动打开浏览器
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "http://localhost:5000",
                UseShellExecute = true
            });

            app.Run();
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
