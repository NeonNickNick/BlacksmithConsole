namespace BlacksmithCore.Backend.JudgementLogic.Actor
{
    public class Health
    {
        public int HP { get; private set; }
        public int MHP { get; private set; }
        public Health(int hp, int mhp)
        {
            HP = hp;
            MHP = mhp;
        }
        public void Init(int hp, int mhp)
        {
            HP = hp;
            MHP = mhp;
        }
        public void GainHP(int addition)
        {
            HP = (int)MathF.Min(MHP, HP + addition);
        }
        public void GainMHP(int addition)
        {
            MHP += addition;
        }
        public void LoseHP(int loss)
        {
            HP = HP - loss;
        }
        public bool LoseMHP(int loss)
        {
            if (loss >= MHP)
            {
                return false;
            }
            MHP -= loss;
            HP = (int)MathF.Min(MHP, HP);
            return true;
        }
    }
}