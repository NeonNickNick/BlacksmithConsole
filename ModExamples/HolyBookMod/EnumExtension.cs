using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Infra.Attributes;

namespace ModExamples.HolyBookMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.BEValue Cross(this ResourceType resourceType) => ResourceType.GetBEValue();
    }
    [IsBlacksmithEnumModifier]
    public static class DefenseExtension
    {
        [IsBlacksmithEnumMember(-8)]//百分比在最外面
        public static DefenseType.BEValue PercentageReduction(this DefenseType defenseType) => DefenseType.GetBEValue();
        [IsBlacksmithEnumMember(32768)]//灰血贴身给一个很大的数值
        public static DefenseType.BEValue GreyHP(this DefenseType defenseType) => DefenseType.GetBEValue();
    }
}
