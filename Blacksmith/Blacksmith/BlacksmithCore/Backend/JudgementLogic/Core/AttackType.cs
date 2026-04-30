using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Backend.JudgementLogic.Core
{
    public class AttackType : BlacksmithEnum<AttackType>
    {
        [IsBlacksmithEnumMember(256)]
        public BEValue Physical() => GetBEValue();
        [IsBlacksmithEnumMember(128)]
        public BEValue Magical() => GetBEValue();
        [IsBlacksmithEnumMember(0)]
        public BEValue Real() => GetBEValue();
    }
}