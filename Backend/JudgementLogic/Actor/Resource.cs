namespace Blacksmith.Backend.JudgementLogic.Actor
{
    public enum ResourceType
    {
        Iron,
        GoldIron,
        Space,
        Time,
        Magic,
        Count
    }
    public class Resource
    {
        private class ResourceTemplate
        {
            public ResourceType CommonType { get; }
            public ResourceType GoldType { get; }
            public float Common { get; set; } = 0;
            public float Gold { get; set; } = 0;
            public ResourceTemplate(ResourceType commonType, ResourceType goldType = ResourceType.Count)
            {
                CommonType = commonType;
                if (goldType != ResourceType.Count)
                {
                    GoldType = goldType;
                }
                else
                {
                    GoldType = commonType;
                }
            }
            public bool Check(float need, bool ifCommonOnly = false)
            {
                float temp = Common;
                if (!ifCommonOnly)
                {
                    temp += Gold;
                }
                return temp >= need;
            }
            public void Use(float need, bool ifCommonOnly = false)
            {
                if (!Check(need, ifCommonOnly))
                {
                    throw new ArgumentException("Unreachable!");
                }
                if (!ifCommonOnly)
                {
                    if (need <= Gold)
                    {
                        Gold -= need;
                    }
                    else
                    {
                        Common -= (need - Gold);
                        Gold = 0;
                    }
                }
                else
                {
                    Common -= need;
                }
            }
            public void Gain(ResourceType type, float add)
            {
                if(type == CommonType)
                {
                    Common += add;
                }else if(type == GoldType)
                {
                    Gold += add;
                }
                else
                {
                    throw new ArgumentException("Unreachable!");
                }
            }
        }
        private Dictionary<ResourceType, ResourceTemplate> _resources = new();
        public Resource()
        {
            var iron = new ResourceTemplate(ResourceType.Iron, ResourceType.GoldIron);
            var space = new ResourceTemplate(ResourceType.Space);
            var time = new ResourceTemplate(ResourceType.Time);
            var magic = new ResourceTemplate(ResourceType.Magic);
            _resources[ResourceType.Iron] = iron;
            _resources[ResourceType.GoldIron] = iron;
            _resources[ResourceType.Space] = space;
            _resources[ResourceType.Time] = time;
            _resources[ResourceType.Magic] = magic;
        }
        public bool Check(ResourceType type, float need, bool ifCommonOnly = false)
        {
            return _resources[type].Check(need, ifCommonOnly);
        }
        public void Use(ResourceType type, float need, bool ifCommonOnly = false)
        {
            _resources[type].Use(need, ifCommonOnly);
        }
        public void Gain(ResourceType type, float need)
        {
            _resources[type].Gain(type, need);
        }
        public float QueryCommon(ResourceType type)
        {
            return _resources[type].Common;
        }
        public float QueryGold(ResourceType type)
        {
            return _resources[type].Gold;
        }
    }
}
