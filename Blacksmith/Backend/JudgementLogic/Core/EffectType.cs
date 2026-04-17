using Blacksmith.Infra.Attributes;
using Blacksmith.Infra.ExtensibleEnum;

namespace Blacksmith.Backend.JudgementLogic.Core
{
    public class EffectType : BlacksmithEnum<EffectType>
    {
        [IsBlacksmithEnumMember(0)]
        public BEValue AfterResolutionWritten() => GetBEValue();
        [IsBlacksmithEnumMember(8)]
        public BEValue AfterTransport() => GetBEValue();
        [IsBlacksmithEnumMember(16)]
        public BEValue AfterResult() => GetBEValue();
    }
}