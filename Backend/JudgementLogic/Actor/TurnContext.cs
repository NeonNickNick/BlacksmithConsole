using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;
using Blacksmith.Backend.JudgementLogic.TurnContexts;

namespace Blacksmith.Backend.JudgementLogic.Actor
{
    public class TurnContext
    {
        public List<AttackResolution> AttackResolutions { get; set; } = new();
        public List<DefenseResolution> DefenseResolutions { get; set; } = new();
        public List<ResourceResolution> ResourceResolutions { get; set; } = new();
        public List<EffectResolution> EffectResolutions { get; set; } = new();

        public void WriteResolution(IResolution resolution)
        {
            if (resolution is AttackResolution attackResolution)
            {
                AttackResolutions.Add(attackResolution);
            }
            else if (resolution is DefenseResolution defenseResolution)
            {
                DefenseResolutions.Add(defenseResolution);
            }else if(resolution is ResourceResolution resourceResolution)
            {
                ResourceResolutions.Add(resourceResolution);
            }
            else if (resolution is EffectResolution effectReslution)
            {
                EffectResolutions.Add(effectReslution);
            }
        }
        public void ExecuteDefense(ActorSet actorSet)
        {
            var effectiveResolutions = DefenseResolutions.Where(d => d.DelayRounds == 0).ToList();
            foreach (var temp in effectiveResolutions)
            {
                temp.Execute(actorSet);
            }
            DefenseResolutions.RemoveAll(d => effectiveResolutions.Contains(d));
            DefenseResolutions.ForEach(d => d.DelayRounds--);
        }
        public void ExecuteResource(ActorSet actorSet)
        {
            var effectiveResolutions = ResourceResolutions.Where(d => d.DelayRounds == 0).ToList();
            foreach (var temp in effectiveResolutions)
            {
                temp.Execute(actorSet);
            }
            ResourceResolutions.RemoveAll(d => effectiveResolutions.Contains(d));
            ResourceResolutions.ForEach(d => d.DelayRounds--);
        }
        public void ExecuteAttack(ActorSet actorSet)
        {
            var effectiveResolutions = AttackResolutions.Where(d => d.DelayRounds == 0).ToList();
            foreach (var temp in effectiveResolutions)
            {
                temp.Execute(actorSet);
            }
            AttackResolutions.RemoveAll(d => effectiveResolutions.Contains(d));
            AttackResolutions.ForEach(d => d.DelayRounds--);
        }
        public void ExecuteEffect(ActorSet actorSet)
        {
            var effectiveResolutions = EffectResolutions.Where(d => d.DelayRounds == 0).ToList();
            foreach (var temp in effectiveResolutions)
            {
                temp.Execute(actorSet);
            }
            EffectResolutions.RemoveAll(d => effectiveResolutions.Contains(d));
            EffectResolutions.ForEach(d => d.DelayRounds--);
        }
    }
}