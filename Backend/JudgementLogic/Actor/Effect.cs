using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Entities;
namespace Blacksmith.Backend.JudgementLogic.Actor
{
    public class Effect
    {
        private readonly List<EffectEntity> _effects = new();
        public void Add(EffectEntity effectEntity)
        {
            _effects.Add(effectEntity);
        }
        public void AddRange(List<EffectEntity> effectEntities)
        {
            _effects.AddRange(effectEntities);
        }
        public void Execute(EffectType type, Body body)
        {
            List<EffectEntity> tempList = _effects.Where(e => e.Type == type).ToList();
            foreach (var temp in tempList)
            {
                if (temp.DelayTimes > 0)
                {
                    temp.DelayTimes--;
                    continue;
                }
                if (temp.RemainingTimes > 0)
                {
                    temp.Execute(body);
                    temp.RemainingTimes--;
                }
            }
        }
        public void Update()
        {
            _effects.RemoveAll(e => e.RemainingTimes <= 0);
        }
        public List<EffectEntity> Get()
        {
            return _effects;
        }
    }
}