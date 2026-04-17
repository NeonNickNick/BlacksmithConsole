namespace Blacksmith.Backend.JudgementLogic.Core
{
    public enum AttackType
    {
        Physical,
        Magic,
        Real,
    }/*
    public class AttackType : BlacksmithEnum<AttackType>
    {
        [IsBlacksmithEnumMenberExtension(256)]
        public EEValue Physical() => GetEEValue();
        [IsBlacksmithEnumMenberExtension(128)]
        public EEValue Magical() => GetEEValue();
        [IsBlacksmithEnumMenberExtension(0)]
        public EEValue Real() => GetEEValue();
    }*/
}