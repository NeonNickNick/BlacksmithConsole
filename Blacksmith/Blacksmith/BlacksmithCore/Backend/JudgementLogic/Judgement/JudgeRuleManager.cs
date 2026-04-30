using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.JudgementLogic.Judgement.Core;
using BlacksmithCore.Backend.JudgementLogic.TurnContexts;
using BlacksmithCore.Backend.Utils;

namespace BlacksmithCore.Backend.JudgementLogic.Judgement
{
    public class JudgeRuleManager
    {
        DynamicJudgeRulePool _dynamicPool = new();
        public class StageRuleContainer
        {
            public class RuleUnit
            {
                public int RemainingRounds;
                public int DelayRounds;
                public Action<ActorSet, ActorSet> Rule;
                public void TimePass()
                {
                    if (DelayRounds > 0)
                    {
                        DelayRounds--;
                    }
                    else
                    {
                        RemainingRounds--;
                    }
                }
                public RuleUnit(int remainingRounds, int delayRounds, Action<ActorSet, ActorSet> rule)
                {
                    RemainingRounds = remainingRounds;
                    DelayRounds = delayRounds;
                    Rule = rule;
                }
            }
            private readonly Action<ActorSet, ActorSet> _baseRule;
            private readonly List<RuleUnit> _overrideRules = new();
            public readonly List<RuleUnit> _modifiersBefore = new();
            public readonly List<RuleUnit> _modifiersAfter = new();
            public StageRuleContainer(Action<ActorSet, ActorSet> baseRule)
            {
                _baseRule = baseRule;
            }
            public void AddOverride(RuleUnit ruleUnit)
            {
                _overrideRules.Add(ruleUnit);
            }
            public void AddModifier(RuleUnit ruleUnit, ModifierOrder modifierOrder)
            {
                if (modifierOrder == ModifierOrder.Before)
                {
                    _modifiersBefore.Add(ruleUnit);
                }
                else
                {
                    _modifiersAfter.Add(ruleUnit);
                }
            }
            public void Update()
            {
                _overrideRules.RemoveAll(o => o.RemainingRounds <= 0);
                _modifiersBefore.RemoveAll(o => o.RemainingRounds <= 0);
                _modifiersAfter.RemoveAll(o => o.RemainingRounds <= 0);

                _overrideRules.ForEach(o => o.TimePass());
                _modifiersBefore.ForEach(o => o.TimePass());
                _modifiersAfter.ForEach(o => o.TimePass());
            }
            public Action<ActorSet, ActorSet> Build()
            {
                Update();
                var overrideRules = _overrideRules
                    .Where(m => m.RemainingRounds == 0)
                    .Select(m => m.Rule)
                    .ToList();
                var modifiersBefore = _modifiersBefore
                    .Where(m => m.RemainingRounds == 0)
                    .Select(m => m.Rule)
                    .ToList();
                var modifiersAfter = _modifiersAfter
                    .Where(m => m.RemainingRounds == 0)
                    .Select(m => m.Rule)
                    .ToList();
                var overrideRule = overrideRules.Count > 0 ? overrideRules[^1] : null;
                return (player, enemy) =>
                {
                    // BEFORE modifiers
                    foreach (var rule in modifiersBefore)
                    {
                        rule(player, enemy);
                    }
                    // 核心规则
                    var core = overrideRule ?? _baseRule;
                    core(player, enemy);
                    // AFTER modifiers
                    foreach (var rule in modifiersAfter)
                    {
                        rule(player, enemy);
                    }
                };
            }
        }

        private readonly SortedDictionary<JudgeStage.BEValue, StageRuleContainer> _ruleContainers = new()
        {
            {
                JudgeStage.Instance.OnBegin(),
                new((player, enemy) => { })
            },
            {
                JudgeStage.Instance.OnEffectTaking_AfterResolutionWritten(),
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterResolutionWritten(), player, enemy))
            },
            {
                JudgeStage.Instance.OnEffectSwaping(),
                new(SwapEffects)
            },
            {
                JudgeStage.Instance.OnAttackCanceling(),
                new(CancelAttacks)
            },
            {
                JudgeStage.Instance.OnAttackSwaping(),
                new(SwapAttacks)
            },
            {
                JudgeStage.Instance.OnApplyingEffect(),
                new(ApplyEffect)
            },
            {
                JudgeStage.Instance.OnEffectTaking_AfterTransport(),
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterTransport(), player, enemy))
            },
            {
                JudgeStage.Instance.OnApplyingOthers(),
                new(ApplyOthers)
            },
            {
                JudgeStage.Instance.OnUpdating(),
                new(Update)
            },
            {
                JudgeStage.Instance.OnEffectTaking_AfterResult(),
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterResult(), player, enemy))
            },
            {
                JudgeStage.Instance.OnEnd(),
                new((player, enemy) => { })
            }
        };
        #region Default Rules（原有逻辑）
        private static void TakeEffects(EffectType.BEValue type, ActorSet player, ActorSet enemy)
        {
            foreach (var temp in player.ActorList)
                temp.EffectEntityWork(type);

            foreach (var temp in enemy.ActorList)
                temp.EffectEntityWork(type);
        }

        private static void SwapEffects(ActorSet player, ActorSet enemy)
        {
            SwapEffects(player.Focus.TurnContext.EffectResolutions,
                        enemy.Focus.TurnContext.EffectResolutions);
        }

        private static void SwapEffects(List<EffectResolution> playerResolutions,
            List<EffectResolution> enemyResolutions)
        {
            var playerTemp = playerResolutions.Where(e => e.TargetType == EffectTargetType.Instance.Enemy() || e.DelayRounds == 0).ToList();
            var enemyTemp = enemyResolutions.Where(e => e.TargetType == EffectTargetType.Instance.Enemy() || e.DelayRounds == 0).ToList();

            playerResolutions.RemoveAll(e => playerTemp.Contains(e));
            enemyResolutions.RemoveAll(e => enemyTemp.Contains(e));

            playerResolutions.AddRange(enemyTemp);
            enemyResolutions.AddRange(playerTemp);
        }

        private static void CancelAttacks(ActorSet player, ActorSet enemy)
        {
            CancelAttackResolutions(player.Focus.TurnContext.AttackResolutions,
                                    enemy.Focus.TurnContext.AttackResolutions);
        }

        private static void CancelAttackResolutions(List<AttackResolution> playerResolutions,
            List<AttackResolution> enemyResolutions)
        {
            playerResolutions.OrderBy(a => a.Type);
            enemyResolutions.OrderBy(a => a.Type);
            int playerIndex = 0;
            int enemyIndex = 0;

            while (playerIndex < playerResolutions.Count && enemyIndex < enemyResolutions.Count)
            {
                var playerAttack = playerResolutions[playerIndex];
                var enemyAttack = enemyResolutions[enemyIndex];

                if (playerAttack.Type == AttackType.Instance.Real() || playerAttack.DelayRounds != 0)
                {
                    playerIndex++;
                    continue;
                }

                if (enemyAttack.Type == AttackType.Instance.Real() || enemyAttack.DelayRounds != 0)
                {
                    enemyIndex++;
                    continue;
                }

                (playerAttack.Power, enemyAttack.Power) =
                    MathFExtensions.Cancel(playerAttack.Power, enemyAttack.Power);

                if (playerAttack.Power <= 0f)
                    playerResolutions.RemoveAt(playerIndex);

                if (enemyAttack.Power <= 0f)
                    enemyResolutions.RemoveAt(enemyIndex);
            }
        }

        private static void SwapAttacks(ActorSet player, ActorSet enemy)
        {
            SwapAttacks(player.Focus.TurnContext.AttackResolutions,
                        enemy.Focus.TurnContext.AttackResolutions);
        }

        private static void SwapAttacks(List<AttackResolution> playerResolutions,
            List<AttackResolution> enemyResolutions)
        {
            var playerTemp = playerResolutions.Where(e => e.DelayRounds == 0).ToList();
            var enemyTemp = enemyResolutions.Where(e => e.DelayRounds == 0).ToList();

            playerResolutions.RemoveAll(e => playerTemp.Contains(e));
            enemyResolutions.RemoveAll(e => enemyTemp.Contains(e));

            playerResolutions.AddRange(enemyTemp);
            enemyResolutions.AddRange(playerTemp);
        }
        private static void ApplyEffect(ActorSet player, ActorSet enemy)
        {
            player.Focus.TurnContext.ExecuteEffect(player);
            enemy.Focus.TurnContext.ExecuteEffect(enemy);
        }
        private static void ApplyOthers(ActorSet player, ActorSet enemy)
        {
            player.Focus.TurnContext.ExecuteDefense(player);
            enemy.Focus.TurnContext.ExecuteDefense(enemy);

            player.Focus.TurnContext.ExecuteAttack(player);
            enemy.Focus.TurnContext.ExecuteAttack(enemy);

            player.Focus.TurnContext.ExecuteResource(player);
            enemy.Focus.TurnContext.ExecuteResource(enemy);
        }

        private static void Update(ActorSet player, ActorSet enemy)
        {
            foreach (var temp in player.ActorList)
                temp.Update();

            foreach (var temp in enemy.ActorList)
                temp.Update();
        }

        #endregion
        public Action<ActorSet, ActorSet> GetRule()
        {
            Action<ActorSet, ActorSet> result = (a, b) => { };

            foreach (var stage in _ruleContainers.OrderBy(k => k.Key))
            {
                result += stage.Value.Build();
            }

            return result;
        }
        public void RegistJudgeRuleDynamic(DynamicJudgeRuleName.BEValue name, List<Mutation> mutations)
        {
            _dynamicPool.RegistDynamic(name, mutations);
        }
        public void AddJudgeRule(ActorSet source, DynamicJudgeRuleName.BEValue name)
        {
            List<Mutation> mutations = _dynamicPool.Query(source, name);
            foreach (var mutation in mutations)
            {
                StageRuleContainer.RuleUnit unit = new(mutation.RemainingRounds, mutation.DelayRounds, mutation.JudgeRule);
                if (mutation.RuleType == RuleType.Override)
                {
                    _ruleContainers[mutation.Stage].AddOverride(unit);
                }
                else
                {
                    _ruleContainers[mutation.Stage].AddModifier(unit, mutation.ModifierOrder);
                }
            }
        }
    }
}
