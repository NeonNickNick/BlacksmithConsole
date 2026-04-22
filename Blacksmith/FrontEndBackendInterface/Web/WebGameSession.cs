using Blacksmith.AI;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;

namespace Blacksmith.FrontendBackendInterface.Web
{
    public class WebGameSession
    {
        private readonly BackendStarter backendStarter = new();
        private readonly List<IAIStrategy> availableStrategies;
        private GameInstance? gameInstance;
        private IAIStrategy? selectedAI;
        private bool isManualMode = true;
        private string modeName = "Not started";

        public WebGameSession(List<IAIStrategy> availableStrategies)
        {
            this.availableStrategies = availableStrategies;
        }

        public IReadOnlyList<WebModeOption> GetStrategies()
        {
            var list = new List<WebModeOption> { new(1, "Manual") };
            for (var i = 0; i < availableStrategies.Count; i++)
            {
                list.Add(new WebModeOption(i + 2, availableStrategies[i].Name));
            }
            return list;
        }

        public WebGameSnapshot StartGame(int mode)
        {
            gameInstance = backendStarter.StartBackend();
            selectedAI = null;
            isManualMode = true;
            modeName = "Manual";

            if (mode >= 2 && mode <= availableStrategies.Count + 1)
            {
                selectedAI = availableStrategies[mode - 2];
                selectedAI.Init(gameInstance);
                isManualMode = false;
                modeName = selectedAI.Name;
            }

            return CreateSnapshot();
        }

        public WebGameSnapshot GetSnapshot()
        {
            return gameInstance == null
                ? WebGameReader.CreateEmptySnapshot(modeName, isManualMode)
                : CreateSnapshot();
        }

        public WebCommandResult DeclareTurn(string skillName, int param, string enemySkillName, int enemyParam)
        {
            if (gameInstance == null)
            {
                return new WebCommandResult(false, "Game not started", GetSnapshot());
            }

            var result = WebGameReader.GetResult(gameInstance.Player.Focus, gameInstance.Enemy.Focus);
            if (result != "next")
            {
                return new WebCommandResult(false, BuildEndMessage(result), CreateSnapshot());
            }

            if (string.IsNullOrWhiteSpace(skillName))
            {
                return new WebCommandResult(false, "Invalid input", CreateSnapshot());
            }

            skillName = skillName.Trim();
            param = Math.Max(0, param);

            var playerCheck = gameInstance.TryDeclare(skillName, param);
            if (playerCheck != SkillDeclareResult.Success)
            {
                return new WebCommandResult(false, $"Player skill invalid: {playerCheck}", CreateSnapshot());
            }

            enemySkillName = string.IsNullOrWhiteSpace(enemySkillName) ? "iron" : enemySkillName.Trim();
            enemyParam = Math.Max(0, enemyParam);

            if (!isManualMode && selectedAI != null)
            {
                var choice = selectedAI.ChooseSkill(gameInstance.Enemy, gameInstance.Player);
                enemySkillName = choice.Item1;
                enemyParam = choice.Item2;
                var aiCheck = gameInstance.ETryDeclare(enemySkillName, enemyParam);
                if (aiCheck != SkillDeclareResult.Success)
                {
                    enemySkillName = "iron";
                    enemyParam = 0;
                    gameInstance.ETryDeclare(enemySkillName, enemyParam);
                }
            }
            else
            {
                var enemyCheck = gameInstance.ETryDeclare(enemySkillName, enemyParam);
                if (enemyCheck != SkillDeclareResult.Success)
                {
                    return new WebCommandResult(false, $"Enemy skill invalid: {enemyCheck}", CreateSnapshot());
                }
            }

            gameInstance.Declare(skillName, param, enemySkillName, enemyParam);
            var snapshot = CreateSnapshot();
            return new WebCommandResult(true, "Turn declared", snapshot);
        }

        private WebGameSnapshot CreateSnapshot()
        {
            return gameInstance == null
                ? WebGameReader.CreateEmptySnapshot(modeName, isManualMode)
                : WebGameReader.CreateSnapshot(gameInstance, modeName, isManualMode);
        }

        private static string BuildEndMessage(string result)
        {
            return result switch
            {
                "win" => "You win!",
                "lose" => "You lose!",
                "draw" => "It's a draw!",
                _ => "Game ended"
            };
        }
    }
}
