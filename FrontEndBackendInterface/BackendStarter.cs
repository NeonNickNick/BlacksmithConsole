using Blacksmith.FrontendBackendInterface;

namespace Blacksmith.FrontEndBackendInterface
{
    public class BackendStarter
    {
        public GameContext StartBackend()
        {
            BackendEntry entry = new();
            GameContext context = new()
            {
                SkillChoose = entry,
                Player = entry.Player,
                Enemy = entry.Enemy
            };
            return context;
        }
    }
}
