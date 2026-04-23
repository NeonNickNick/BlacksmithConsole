using System.Data;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;
using Blacksmith.Infra.Attributes;
using Blacksmith.Infra.ExtensibleEnum;

namespace Blacksmith.Backend.JudgementLogic.Judgement
{
    public class JudgeStage : BlacksmithEnum<JudgeStage>
    {
        [IsBlacksmithEnumMember(0)]
        public BEValue OnBegin() => GetBEValue();
        [IsBlacksmithEnumMember(8)]
        public BEValue OnEffectTaking_AfterResolutionWritten() => GetBEValue();
        [IsBlacksmithEnumMember(16)]
        public BEValue OnEffectSwaping() => GetBEValue();
        [IsBlacksmithEnumMember(32)]
        public BEValue OnAttackCanceling() => GetBEValue();
        [IsBlacksmithEnumMember(64)]
        public BEValue OnAttackSwaping() => GetBEValue();
        [IsBlacksmithEnumMember(128)]
        public BEValue OnApplyingEffect() => GetBEValue();
        [IsBlacksmithEnumMember(256)]
        public BEValue OnEffectTaking_AfterTransport() => GetBEValue();
        [IsBlacksmithEnumMember(512)]
        public BEValue OnApplyingOthers() => GetBEValue();
        [IsBlacksmithEnumMember(1024)]
        public BEValue OnUpdating() => GetBEValue();
        [IsBlacksmithEnumMember(2048)]
        public BEValue OnEffectTaking_AfterResult() => GetBEValue();
        [IsBlacksmithEnumMember(4096)]
        public BEValue OnEnd() => GetBEValue();
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
        public int DelayRounds;
        public JudgeStage.BEValue Stage;
        public RuleType RuleType;
        public ModifierOrder ModifierOrder;
        public Action<ActorSet, ActorSet> JudgeRule;
        public Mutation(
            Action<ActorSet, ActorSet> judgeRule,
            JudgeStage.BEValue stage,
            RuleType ruleType,
            ModifierOrder modifierOrder,
            int remainingRounds = 1,
            int delayRounds = 0)
        {
            RemainingRounds = remainingRounds;
            DelayRounds = delayRounds;
            Stage = stage;
            RuleType = ruleType;
            ModifierOrder = modifierOrder;
            JudgeRule = judgeRule;
        }
    }
    public class DynamicJudgeRulePool
    {
        private class MutationPrototype
        {
            public int RemainingRounds;
            public int DelayRounds;
            public JudgeStage.BEValue Stage;
            public RuleType RuleType;
            public ModifierOrder ModifierOrder;
            public Action<ActorSet, ActorSet, ActorSet> JudgeRulePrototype;
            public MutationPrototype(int remainingRounds, int delayRounds, JudgeStage.BEValue stage, RuleType ruleType, ModifierOrder modifierOrder, Action<ActorSet, ActorSet, ActorSet> judgeRulePrototype)
            {
                RemainingRounds = remainingRounds;
                DelayRounds = delayRounds;
                Stage = stage;
                RuleType = ruleType;
                ModifierOrder = modifierOrder;
                JudgeRulePrototype = judgeRulePrototype;
            }
            public Mutation Specialize(ActorSet source)
            {
                return new((player, enemy) => JudgeRulePrototype(source, player, enemy), Stage, RuleType, ModifierOrder, RemainingRounds, DelayRounds);
            }
        }
        private readonly Dictionary<DynamicJudgeRuleName.BEValue, List<MutationPrototype>> _mutationPrototypes = new();
        public void RegistDynamic(
            DynamicJudgeRuleName.BEValue name,
            List<Mutation> mutations)
        {
            _mutationPrototypes[name] = new();
            foreach (var mutation in mutations)
            {
                _mutationPrototypes[name].Add(new(
                    mutation.RemainingRounds,
                    mutation.DelayRounds,
                    mutation.Stage,
                    mutation.RuleType,
                    mutation.ModifierOrder,
                    (source, player, enemy) =>
                    {
                        IfElseUtil(source, player, enemy, mutation.JudgeRule);
                    }));
            }
        }
        public List<Mutation> Query(ActorSet source, DynamicJudgeRuleName.BEValue name)
        {
            return _mutationPrototypes[name].Select(m => m.Specialize(source)).ToList();
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

        //延时保护：实际上与转移是类似的
        private void IfElseUtil(ActorSet source, ActorSet player, ActorSet enemy, Action<ActorSet, ActorSet> impl)
        {
            if (source == player)
            {
                impl(player, enemy);
            }
            else if (source == enemy)
            {
                impl(enemy, player);
            }
            else
            {
                throw new ArgumentException("Unreachable!");
            }
        }
    }
}