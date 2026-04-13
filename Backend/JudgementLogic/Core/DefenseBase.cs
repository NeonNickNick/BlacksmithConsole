using Blacksmith.Backend.JudgementLogic.Actor;
namespace Blacksmith.Backend.JudgementLogic.Core
{
    public abstract class DefenseBase : IDefenseWork
    {
        public abstract DefenseType Type { get; set; }
        public abstract int Power { get; set; }
        public abstract bool CanMerge { get; set; }
        public abstract bool IsDead { get; set; }
        public abstract void Merge(DefenseBase addition);
        public abstract int Work(Body source, Body owner, int attack, AttackType type);
        public abstract void Update();
    }
}