using Blacksmith.Backend.Backend.SkillPackages.Logic;

namespace Blacksmith.FrontendBackendInterface
{
    public class GameHistory
    {
        public List<(ISkillContext, ISkillContext)> SkillHistory { get; set; } = new();
        public void Swap()
        {
            SkillHistory = SkillHistory.Select(s => (s.Item2, s.Item1)).ToList();
        }
    }
}
