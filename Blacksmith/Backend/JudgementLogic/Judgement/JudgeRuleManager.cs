using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.TurnContexts;
using Blacksmith.Backend.Utils;

namespace Blacksmith.Backend.JudgementLogic.Judgement
{
    public class JudgeRuleManager
    {
        public class StageRuleContainer
        {
            private enum Mark
            {
                Before,
                Override,
                After
            }
            private readonly Action<ActorSet, ActorSet> _baseRule;
            private Action<ActorSet, ActorSet>? _overrideRule; // 非对合
            public readonly List<Action<ActorSet, ActorSet>> _modifiersBefore = new();
            public readonly List<Action<ActorSet, ActorSet>> _modifiersAfter = new();
            public StageRuleContainer(Action<ActorSet, ActorSet> baseRule)
            {
                _baseRule = baseRule;
            }
            public void SetOverride(Action<ActorSet, ActorSet>? rule)
            {
                _overrideRule = rule;
            }
            public void AddModifier(Action<ActorSet, ActorSet> rule, ModifierOrder modifierOrder)
            {
                if (modifierOrder == ModifierOrder.Before)
                {
                    _modifiersBefore.Add(rule);
                }
                else
                {
                    _modifiersAfter.Add(rule);
                }
            }
            public void RemoveModifier(Action<ActorSet, ActorSet> rule, ModifierOrder modifierOrder)
            {
                if (modifierOrder == ModifierOrder.Before)
                {
                    _modifiersBefore.Remove(rule);
                }
                else
                {
                    _modifiersAfter.Remove(rule);
                }
            }
            public Action<ActorSet, ActorSet> Build()
            {
                return (player, enemy) =>
                {
                    // BEFORE modifiers
                    foreach (var rule in _modifiersBefore)
                    {
                        rule(player, enemy);
                    }
                    // 核心规则
                    var core = _overrideRule ?? _baseRule;
                    core(player, enemy);
                    // AFTER modifiers
                    foreach (var rule in _modifiersAfter)
                    {
                        rule(player, enemy);
                    }
                };
            }
        }

        private readonly SortedDictionary<JudgeStage, StageRuleContainer> _ruleContainers = new()
        {
            {
                JudgeStage.OnEffectTaking_AfterResolutionWritten,
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterResolutionWritten(), player, enemy))
            },
            {
                JudgeStage.OnEffectSwaping,
                new(SwapEffects)
            },
            {
                JudgeStage.OnAttackCanceling,
                new(CancelAttacks)
            },
            {
                JudgeStage.OnAttackSwaping,
                new(SwapAttacks)
            },
            { 
                JudgeStage.OnApplyingEffect,
                new(ApplyEffect)
            },
            {
                JudgeStage.OnEffectTaking_AfterTransport,
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterTransport(), player, enemy))
            },
            {
                JudgeStage.OnApplyingOthers,
                new(ApplyOthers)
            },
            {
                JudgeStage.OnUpdating,
                new(Update)
            },
            {
                JudgeStage.OnEffectTaking_AfterResult,
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterResult(), player, enemy))
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
        public class ActiveJudgeRuleMark
        {
            public string MutationName = "";
            public int RemainingRounds;
            public JudgeStage Stage;
            public RuleType RuleType;
            public ModifierOrder ModifierOrder;
            public Action<ActorSet, ActorSet>? RuleRef;
        }
        private List<ActiveJudgeRuleMark> _activeJudgeRuleMarks = new();
        public void AddJudgeRule(ActorSet source, string mutationName)
        {
            List<Mutation> mutations = JudgeRulePool.Query(source, mutationName);
            foreach (var mutation in mutations)
            {
                ActiveJudgeRuleMark mark = new()
                {
                    MutationName = mutationName,
                    RemainingRounds = mutation.RemainingRounds,
                    Stage = mutation.Stage,
                    RuleType = mutation.RuleType,
                    ModifierOrder = mutation.ModifierOrder,
                    RuleRef = mutation.JudgeRule
                };
                if (mark.RuleType == RuleType.Override)
                {
                    int index = _activeJudgeRuleMarks.FindIndex(a => a.Stage == mark.Stage && a.RuleType == RuleType.Override);
                    if (index > -1)
                    {
                        _activeJudgeRuleMarks.RemoveAt(index);
                        _activeJudgeRuleMarks.Add(mark);
                        _ruleContainers[mark.Stage].SetOverride(mutation.JudgeRule);
                        return;
                    }
                    _activeJudgeRuleMarks.Add(mark);
                    _ruleContainers[mark.Stage].SetOverride(mutation.JudgeRule);
                }
                else
                {
                    _activeJudgeRuleMarks.Add(mark);
                    _ruleContainers[mark.Stage].AddModifier(mutation.JudgeRule, mutation.ModifierOrder);
                }
            }
        }
        public void Update()
        {
            
            foreach(var mark in _activeJudgeRuleMarks)
            {
                mark.RemainingRounds--;
                if(mark.RemainingRounds > 0)
                {
                    continue;
                }
                if (mark.RuleType == RuleType.Override)
                {
                    _ruleContainers[mark.Stage].SetOverride(null);
                }
                else
                {
                    _ruleContainers[mark.Stage].RemoveModifier(mark.RuleRef!, mark.ModifierOrder);
                }
            }
            _activeJudgeRuleMarks.RemoveAll(a => a.RemainingRounds <= 0);
        }
    }
}
