using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Backend.JudgementLogic.Core
{
    public class ResourceType : BlacksmithEnum<ResourceType>
    {
        [IsBlacksmithEnumMember(0)]
        public BEValue Iron() => GetBEValue();
        [IsBlacksmithEnumMember(1)]
        public BEValue Gold_Iron() => GetBEValue();
        [IsBlacksmithEnumMember(2)]
        public BEValue Space() => GetBEValue();
        [IsBlacksmithEnumMember(3)]
        public BEValue Time() => GetBEValue();
        [IsBlacksmithEnumMember(4)]
        public BEValue Magic() => GetBEValue();
    }
}
