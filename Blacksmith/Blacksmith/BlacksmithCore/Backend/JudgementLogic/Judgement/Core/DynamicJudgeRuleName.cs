using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Backend.JudgementLogic.Judgement.Core
{
    public class DynamicJudgeRuleName : BlacksmithEnum<DynamicJudgeRuleName>
    {
        [IsBlacksmithEnumMember(0)]
        public BEValue Reflect() => GetBEValue();
        [IsBlacksmithEnumMember(1)]
        public BEValue Charge() => GetBEValue();
    }
}
