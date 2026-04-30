using BlacksmithCore.Backend.JudgementLogic.Actor;

namespace BlacksmithCore.Backend.JudgementLogic.Core
{
    public interface IDefenseWork
    {
        public abstract DefenseType.BEValue Type { get; set; }
        public (int, int) Work(Body source, Body owner, int Attack, AttackType.BEValue type);
    }
}
