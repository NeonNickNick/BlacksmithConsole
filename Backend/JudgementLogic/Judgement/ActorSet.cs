using Blacksmith.Backend.JudgementLogic.Actor;

namespace Blacksmith.Backend.JudgementLogic.Judgement
{
    public interface IActorSet
    {
        public float HP { get; }
        public float MHP { get; }
    }
    public class ActorSet
    {
        public Body Focus { get; private set; }
        public List<Body> ActorList { get; private set; }
        public ActorSet()
        {
            Focus = new(this);
            ActorList = new() { Focus };
        }

    }
}