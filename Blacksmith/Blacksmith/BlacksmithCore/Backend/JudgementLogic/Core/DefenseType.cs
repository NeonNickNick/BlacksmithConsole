using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Backend.JudgementLogic.Core
{
    public class DefenseType : BlacksmithEnum<DefenseType>
    {
        [IsBlacksmithEnumMember(0)]
        public BEValue RealReduction() => GetBEValue();
        [IsBlacksmithEnumMember(8)]
        public BEValue ThornReduction() => GetBEValue();
        [IsBlacksmithEnumMember(16)]
        public BEValue CommonReduction() => GetBEValue();
        [IsBlacksmithEnumMember(32)]
        public BEValue RockArmor() => GetBEValue();
        [IsBlacksmithEnumMember(64)]
        public BEValue ReadlArmor() => GetBEValue();
        [IsBlacksmithEnumMember(128)]
        public BEValue CommonArmor() => GetBEValue();
    }
}