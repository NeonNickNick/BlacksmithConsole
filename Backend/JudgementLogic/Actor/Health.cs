namespace Blacksmith.Backend.JudgementLogic.Actor
{
    public class Health
    {
        public int HP { get; private set; }
        public int MHP { get; private set; }
        public event Action<int, int> OnHealthChanged;
        public Health(int hp, int mhp)
        {
            HP = hp;
            MHP = mhp;
        }
        public void Init(int hp, int mhp)
        {
            HP = hp;
            MHP = mhp;
            OnHealthChanged?.Invoke(HP, MHP);
        }
        public void GainHP(int addition)
        {
            HP = (int)MathF.Min(MHP, HP + addition);
            OnHealthChanged?.Invoke(HP, MHP);
        }
        public void GainMHP(int addition)
        {
            MHP += addition;
            OnHealthChanged?.Invoke(HP, MHP);
        }
        public void LoseHP(int loss)
        {
            HP = (int)MathF.Max(0f, HP - loss);
            OnHealthChanged?.Invoke(HP, MHP);
        }
        public bool LoseMHP(int loss)
        {
            if (loss >= MHP)
            {
                return false;
            }
            MHP -= loss;
            HP = (int)MathF.Min(MHP, HP);
            OnHealthChanged?.Invoke(HP, MHP);
            return true;
        }
    }
}