using Blacksmith.Backend.Backend.SkillPackages.Logic;

namespace Blacksmith.FrontendBackendInterface
{
    public class GameHistory
    {
        public List<(ISkillContext, ISkillContext)> SkillHistory { get; set; } = new();
    }
}
