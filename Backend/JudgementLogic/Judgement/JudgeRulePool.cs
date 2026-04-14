using Blacksmith.Backend.JudgementLogic.Core;

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
        public Mutation(int remainingRounds, JudgeStage stage, RuleType ruleType, ModifierOrder modifierOrder, Action<ActorSet, ActorSet> judgeRule)
        {
            RemainingRounds = remainingRounds;
            Stage = stage;
            RuleType = ruleType;
            ModifierOrder = modifierOrder;
            JudgeRule = judgeRule;
        }
    }
    public static class JudgeRulePool
    {
        private class MutationPrototype
        {
            public int RemainingRounds;
            public JudgeStage Stage;
            public RuleType RuleType;
            public ModifierOrder ModifierOrder;
            public Action<ActorSet, ActorSet, ActorSet> JudgeRulePrototype;
            public MutationPrototype(int remainingRounds, JudgeStage stage, RuleType ruleType, ModifierOrder modifierOrder, Action<ActorSet, ActorSet, ActorSet> judgeRulePrototype)
            {
                RemainingRounds = remainingRounds;
                Stage = stage;
                RuleType = ruleType;
                ModifierOrder = modifierOrder;
                JudgeRulePrototype = judgeRulePrototype;
            }
            public Mutation Specialize(ActorSet source)
            {
                return new(RemainingRounds, Stage, RuleType, ModifierOrder, (player, enemy) => JudgeRulePrototype(source, player, enemy));
            }
        }
        private static readonly Dictionary<string, List<MutationPrototype>> _mutationPrototypes = new()
        {
            { "reflect", 
                new(){
                    new(1, JudgeStage.OnEffectSwaping, RuleType.Modifier, ModifierOrder.After, Reflect_AfterEffectSwaping_Modifier_After),
                    new(1, JudgeStage.OnAttackSwaping, RuleType.Modifier, ModifierOrder.After, Reflect_AfterAttackSwaping_Modifier_After)
                } 
            },
        };
        public static List<Mutation> Query(ActorSet source, string mutationName)
        {
            return _mutationPrototypes[mutationName].Select(m => m.Specialize(source)).ToList();
        }
        //从这里开始是转移、延时保护等个别技能的专属规则

        //默认规则中双方是“平等”的，规则本身对称。
        //这些技能会使得规则不再对称，因此必须知道规则的发起者，否则不知道是谁使用了这些技能
        //这就是为什么Query中必须声明是哪个玩家组在查询，且Pool中不能直接存储Mutation列表

        //转移：暂时只考虑转移外源性攻击和效果，不会把自己的防御，资源，效果转移给别人
        //所有Resolution只要是当回合释放，就应该被转移。先讨论Attack
        //由于AttackResolution一定指向对方，OnAttackSwaping阶段以后转移只需要反射使用者的列表给目标
        //暂时对于Effect也是交换后反射。如果日后能够转移自己身上的效果，那么需要将EffectEntity重新包装为EffectResolution

        //接下来以EffectResolution转移（不是Entity转移）为例
        //假设是player使用了转移，此时已经在swap之后，当回合生效的player的effectresolution已经到enemy那里了，反之亦然。
        //因此player现在持有effectresolution中当且仅当“当回合生效且Target标记为Enemy”的来自于本回合enemy行动
        //将这些resolution的延迟回合设为1，就欺骗了Judger
        //让它认为这些其实是player打出的下回合生效的effectresotluion，由此实现了转移的效果
        //AttackResolution转移同理，更加简单，不需要判断Target
        private static void IfElseUtil(ActorSet source, ActorSet player, ActorSet enemy, Action<ActorSet, ActorSet> impl)
        {
            if(source == player)
            {
                impl(player, enemy);
            }else if(source == enemy)
            {
                impl(enemy, player);
            }
            else
            {
                throw new ArgumentException("Unreachable!");
            }
        }
      
        private static void Reflect_AfterEffectSwaping_Modifier_After(ActorSet source, ActorSet player, ActorSet enemy)
        {
            IfElseUtil(source, player, enemy, Reflect_AfterEffectSwaping_Modifier_After_Impl);
        }
        private static void Reflect_AfterEffectSwaping_Modifier_After_Impl(ActorSet player, ActorSet enemy)
        {
            
            var playerResolutions = player.Focus.TurnContext.EffectResolutions;

            var reflect = playerResolutions.Where(e => e.TargetType == EffectTargetType.Enemy || e.DelayRounds == 0).ToList();

            playerResolutions.RemoveAll(e => reflect.Contains(e));

            reflect.ForEach(e => e.DelayRounds = 1);

            playerResolutions.AddRange(reflect);
        }

        private static void Reflect_AfterAttackSwaping_Modifier_After(ActorSet source, ActorSet player, ActorSet enemy)
        {
            IfElseUtil(source, player, enemy, Reflect_AfterAttackSwaping_Modifier_After_Impl);
        }
        private static void Reflect_AfterAttackSwaping_Modifier_After_Impl(ActorSet player, ActorSet enemy)
        {
            var playerResolutions = player.Focus.TurnContext.AttackResolutions;

            var reflect = playerResolutions.Where(a => a.DelayRounds == 0).ToList();

            playerResolutions.RemoveAll(a => reflect.Contains(a));

            reflect.ForEach(a => a.DelayRounds = 1);

            playerResolutions.AddRange(reflect);
        }
    }
}