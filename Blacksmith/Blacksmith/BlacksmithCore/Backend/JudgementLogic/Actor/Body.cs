using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.JudgementLogic.Judgement;

namespace BlacksmithCore.Backend.JudgementLogic.Actor
{
    public class Body
    {
        public ActorSet Community { get; }
        public Skill Skill { get; private set; } = new();
        public Health Health { get; private set; } = new(10, 10);
        public Defense Defense { get; private set; } = new();
        public Resource Resource { get; private set; } = new();
        public Effect Effect { get; private set; } = new();
        public TurnContext TurnContext { get; private set; } = new();
        public Body(ActorSet community)
        {
            Community = community;
        }

        public void Update()
        {
            Defense.Update();
            Effect.Update();
        }
        public void EffectEntityWork(EffectType.BEValue type)
        {
            Effect.Execute(type, this);
        }
        public BodyView GetView()
        {
            return new() {
                ProfessionNames = Skill.GetView(),
                HP = Health.HP,
                MHP = Health.MHP,
                DefenseView = Defense.GetView(),
                ResourcesView = Resource.GetView(),
                FutureAttackView = TurnContext.GetFutureAttackView(),
                FutureDefenseView = TurnContext.GetFutureDefenseView()
            };
        }
    }
    public class BodyView
    {
        public required List<string> ProfessionNames { get; set; }
        public required int HP { get; set; }
        public required int MHP { get; set; }
        public required List<(string name, int power)> DefenseView { get; set; }
        public required List<(string name, float quantity)> ResourcesView { get; set; }
        public required List<(string name, int delayRounds, int power)> FutureAttackView { get; set; }
        public required List<(string name, int delayRounds, int power)> FutureDefenseView { get; set; }
    }
}