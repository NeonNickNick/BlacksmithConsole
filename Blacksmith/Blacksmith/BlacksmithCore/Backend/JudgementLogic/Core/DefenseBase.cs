using BlacksmithCore.Backend.JudgementLogic.Actor;
namespace BlacksmithCore.Backend.JudgementLogic.Core
{
    public abstract class DefenseBase : IDefenseWork
    {
        public abstract DefenseType.BEValue Type { get; set; }
        public abstract int Power { get; set; }
        public abstract bool CanMerge { get; set; }
        public abstract bool IsDead { get; set; }
        public abstract void Merge(DefenseBase addition);
        public abstract (int, int) Work(Body source, Body owner, int attack, AttackType.BEValue type);
        public abstract void Update();
    }
}