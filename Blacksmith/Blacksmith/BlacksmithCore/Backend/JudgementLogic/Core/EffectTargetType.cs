using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Backend.JudgementLogic.Core
{
    public class EffectTargetType : BlacksmithEnum<EffectTargetType>
    {
        [IsBlacksmithEnumMember(0)]
        public BEValue Self() => GetBEValue();
        [IsBlacksmithEnumMember(8)]
        public BEValue Enemy() => GetBEValue();
    }
}