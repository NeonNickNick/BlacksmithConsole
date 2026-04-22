using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions;

namespace Blacksmith.FrontendBackendInterface.Web
{
    public static class WebGameReader
    {
        public static WebGameSnapshot CreateEmptySnapshot(string modeName = "Not started", bool manualMode = true)
        {
            return new WebGameSnapshot(false, modeName, manualMode, "pending", 0, null, null, Array.Empty<WebTurnRecord>());
        }

        public static WebGameSnapshot CreateSnapshot(GameInstance instance, string modeName, bool manualMode)
        {
            return new WebGameSnapshot(
                true,
                modeName,
                manualMode,
                GetResult(instance.Player.Focus, instance.Enemy.Focus),
                instance.History.Turns.Count,
                CreateActorSnapshot("You", instance.Player.Focus),
                CreateActorSnapshot("Robert", instance.Enemy.Focus),
                instance.History.Turns.ToList());
        }

        public static string GetResult(Body player, Body enemy)
        {
            if (player.Health.HP <= 0 && enemy.Health.HP <= 0)
            {
                return "draw";
            }
            if (player.Health.HP <= 0)
            {
                return "lose";
            }
            if (enemy.Health.HP <= 0)
            {
                return "win";
            }
            return "next";
        }

        private static WebActorSnapshot CreateActorSnapshot(string name, Body body)
        {
            return new WebActorSnapshot(
                name,
                GetProfession(body),
                body.Health.HP,
                body.Health.MHP,
                CreateResources(body),
                body.Defense.Defenses
                    .Select(d => new WebDefenseRecord(d.Type.ToString(), d.Power))
                    .ToList(),
                GetPlayableSkills(body),
                body.Skill.GetActivePackageNames());
        }

        private static IReadOnlyList<string> GetPlayableSkills(Body body)
        {
            return body.Skill
                .GetAvailableSkillNames()
                .Where(skillName => CanDeclareWithAnyReasonableParam(body, body.Community, skillName))
                .Distinct()
                .OrderBy(skillName => skillName)
                .ToList();
        }

        private static bool CanDeclareWithAnyReasonableParam(Body body, ActorSet actorSet, string skillName)
        {
            var maxParam = GetMaxParam(body);
            for (var param = 0; param <= maxParam; param++)
            {
                var context = new DefaultSkillContext(skillName, actorSet, param);
                if (body.Skill.TryDeclare(skillName, context) == SkillDeclareResult.Success)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetMaxParam(Body body)
        {
            var resources = CreateResources(body);
            var maxResource = resources.Count == 0 ? 0 : resources.Max(resource => resource.Total);
            return Math.Max(12, (int)Math.Ceiling(maxResource) + 4);
        }

        private static string GetProfession(Body body)
        {
            var profession = body.Skill
                .GetActivePackageNames()
                .FirstOrDefault(packageName => packageName != nameof(Common));

            return profession ?? "None";
        }

        private static IReadOnlyList<WebResourceRecord> CreateResources(Body body)
        {
            return new List<WebResourceRecord>
            {
                CreateResourceRecord("Iron", body, ResourceType.Instance.Iron()),
                CreateResourceRecord("Space", body, ResourceType.Instance.Space()),
                CreateResourceRecord("Time", body, ResourceType.Instance.Time()),
                CreateResourceRecord("Magic", body, ResourceType.Instance.Magic())
            };
        }

        private static WebResourceRecord CreateResourceRecord(string name, Body body, ResourceType.BEValue type)
        {
            var common = body.Resource.QueryCommon(type);
            var gold = body.Resource.QueryGold(type);
            return new WebResourceRecord(name, common, gold, common + gold);
        }
    }
}
