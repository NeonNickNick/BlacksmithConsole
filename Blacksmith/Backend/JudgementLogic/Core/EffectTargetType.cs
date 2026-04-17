using Blacksmith.Infra.Attributes;
using Blacksmith.Infra.ExtensibleEnum;

namespace Blacksmith.Backend.JudgementLogic.Core
{
    public class EffectTargetType : BlacksmithEnum<EffectTargetType>
    {
        [IsBlacksmithEnumMember(0)]
        public BEValue Self() => GetBEValue();
        [IsBlacksmithEnumMember(8)]
        public BEValue Enemy() => GetBEValue();
    }
}