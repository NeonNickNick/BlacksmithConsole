namespace Blacksmith.Backend.JudgementLogic.Judgement
{
    public enum JudgeStage
    {
        OnEffectTaking_AfterResolutionWritten,
        OnEffectSwaping,
        OnAttackCanceling,
        OnAttackSwaping,
        OnApplyingEffect,
        OnEffectTaking_AfterTransport,
        OnApplyingOthers,
        OnUpdating,
        OnEffectTaking_AfterResult
    }
    public enum RuleType
    {
        Override,
        Modifier
    }
    public enum ModifierOrder
    {
        Before,
        After
    }
    public class Mutation
    {
        public int RemainingRounds;
        public JudgeStage Stage;
        public RuleType RuleType;
        public ModifierOrder ModifierOrder;
        public Action<ActorSet, ActorSet> JudgeRule;
        public Mutation(Mutation origin)
        {
            RemainingRounds = origin.RemainingRounds;
            Stage = origin.Stage;
            RuleType = origin.RuleType;
            ModifierOrder = origin.ModifierOrder;
            JudgeRule = origin.JudgeRule;
        }
        /*
        public Mutation(int remainingRounds, JudgeStage stage, RuleType ruleType, ModifierOrder modifierOrder, Action<ActorSet, ActorSet> judgeRule)
        {
            RemainingRounds = remainingRounds;
            Stage = stage;
            RuleType = ruleType;
            ModifierOrder = modifierOrder;
            JudgeRule = judgeRule;
        }*/
    }
    public static class JudgeRulePool
    {
        private static readonly Dictionary<string, List<Mutation>> _mutations = new();
        public static List<Mutation> Query(string mutationName)
        {
            return new List<Mutation>(_mutations[mutationName]).Select(m => new Mutation(m)).ToList();
        }
    }
}