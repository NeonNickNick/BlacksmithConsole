using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Judgement;

namespace Blacksmith.AI.Strategies
{
    public class BloodSigilStrategy : IAIStrategy
    {
        public string Name => "Blood Sigil";

        private int _turn = 0;
        private readonly Random _random = new();

        public (string skillName, int param) ChooseSkill(
            ActorSet self,
            ActorSet opponent)
        {
            _turn++;

            if (_turn <= 7)
                return ("iron", 0);

            if (_turn == 8)
                return ("bloodsigil", 0);

            int hp = self.Focus.Health.HP;

            if (hp <= 1)
                return ("bloodrecovery", 0);
            if (hp <= 5)
                return ("bloodrage", 0);

            var packages = opponent.Focus.Skill.GetActivePackageNames();
            string profession = packages.FirstOrDefault(p => p != "common") ?? "common";
            int maxDmg = GetMaxSingleTurnDamage(opponent);
            bool killable = maxDmg >= hp + 2;

            return profession switch
            {
                "cannon"  => ChooseVsCannon(hp, maxDmg, killable),
                "driver"  => ChooseVsDriver(hp, maxDmg, killable),
                "warlock" => ChooseBase(),
                _         => ChooseBase()
            };
        }

        // 0.7 bloodblade / 0.3 bloodlust, no recovery
        private (string, int) ChooseBase()
        {
            return _random.NextDouble() < 0.70
                ? ("bloodblade", 0)
                : ("bloodlust", 0);
        }

        private (string, int) ChooseVsCannon(int hp, int maxDmg, bool killable)
        {
            if (killable)
            {
                // _advancekillable: bloodshield can prevent death this turn
                int shieldDef = (int)MathF.Ceiling(0.4f * hp);
                bool advanceKillable = maxDmg < (hp - 1) + shieldDef;

                if (advanceKillable)
                {
                    double roll = _random.NextDouble();
                    if (roll < 0.4) return ("bloodshield", 0);
                    if (roll < 0.8) return ("bloodblade", 0);
                    return ("bloodrecovery", 0);
                }
                return ("bloodblade", 0);
            }

            if (maxDmg == hp)
            {
                double roll = _random.NextDouble();
                if (roll < 0.8) return ("bloodrecovery", 0);
                if (roll < 0.9) return ("bloodblade", 0);
                return ("bloodshield", 0);
            }

            return ChooseBase();
        }

        private (string, int) ChooseVsDriver(int hp, int maxDmg, bool killable)
        {
            if (killable)
            {
                int shieldDef = (int)MathF.Ceiling(0.4f * hp);
                bool advanceKillable = maxDmg < (hp - 1) + shieldDef;

                if (advanceKillable)
                {
                    // cannon: 0.4 shield / 0.4 blade / 0.2 recovery
                    // driver: shift 0.1 blade -> lust
                    double roll = _random.NextDouble();
                    if (roll < 0.4) return ("bloodshield", 0);
                    if (roll < 0.7) return ("bloodblade", 0);
                    if (roll < 0.8) return ("bloodlust", 0);
                    return ("bloodrecovery", 0);
                }
                // cannon: 1.0 blade -> driver: 0.9 blade / 0.1 lust
                return _random.NextDouble() < 0.9
                    ? ("bloodblade", 0)
                    : ("bloodlust", 0);
            }

            if (maxDmg == hp)
            {
                // cannon: 0.8 recovery / 0.1 blade / 0.1 shield
                // driver: blade 0.1 -> 0.0, +0.1 lust
                double roll = _random.NextDouble();
                if (roll < 0.8) return ("bloodrecovery", 0);
                if (roll < 0.9) return ("bloodlust", 0);
                return ("bloodshield", 0);
            }

            // cannon base: 0.7 blade / 0.3 lust -> driver: 0.6 blade / 0.4 lust
            return _random.NextDouble() < 0.6
                ? ("bloodblade", 0)
                : ("bloodlust", 0);
        }

        private int GetMaxSingleTurnDamage(ActorSet opponent)
        {
            var skills = opponent.Focus.Skill.GetAvailableSkillNames();
            var res = opponent.Focus.Resource;
            int maxDmg = 0;

            // Common attacks
            if (skills.Contains("tear") && res.Check(ResourceType.Space, 1))
                maxDmg = Math.Max(maxDmg, 8);
            if (skills.Contains("slash") && res.Check(ResourceType.Iron, 2.5f))
                maxDmg = Math.Max(maxDmg, 5);
            if (skills.Contains("drill") && res.Check(ResourceType.Iron, 1.5f))
                maxDmg = Math.Max(maxDmg, 3);
            if (skills.Contains("stick") && res.Check(ResourceType.Iron, 0.5f))
                maxDmg = Math.Max(maxDmg, 1);

            // Cannon attacks
            if (skills.Contains("triplestrike") && res.Check(ResourceType.Iron, 3))
                maxDmg = Math.Max(maxDmg, 11);
            if (skills.Contains("doublestrike") && res.Check(ResourceType.Iron, 2))
                maxDmg = Math.Max(maxDmg, 7);
            if (skills.Contains("strike") && res.Check(ResourceType.Iron, 1))
                maxDmg = Math.Max(maxDmg, 3);
            if (skills.Contains("cannonbarrel"))
                maxDmg = Math.Max(maxDmg, 1);

            // Driver attacks
            if (skills.Contains("spaceattack") && res.Check(ResourceType.Space, 1))
                maxDmg = Math.Max(maxDmg, 11);

            // BloodSigil attacks
            if (skills.Contains("bloodblade") && opponent.Focus.Health.HP > 4)
                maxDmg = Math.Max(maxDmg, 6);
            if (skills.Contains("bloodrage") && opponent.Focus.Health.HP > 1 && opponent.Focus.Health.HP <= 5)
                maxDmg = Math.Max(maxDmg, 5);

            return maxDmg;
        }
    }
}
