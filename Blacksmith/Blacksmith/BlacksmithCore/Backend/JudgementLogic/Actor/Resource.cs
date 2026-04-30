using System.Reflection;
using BlacksmithCore.Backend.JudgementLogic.Core;

namespace BlacksmithCore.Backend.JudgementLogic.Actor
{

    public class Resource
    {
        private class ResourceTemplate
        {
            public ResourceType.BEValue CommonType { get; }
            public ResourceType.BEValue GoldType { get; }
            public float Common { get; set; } = 0;
            public float Gold { get; set; } = 0;
            public ResourceTemplate(ResourceType.BEValue commonType, ResourceType.BEValue goldType)
            {
                CommonType = commonType;
                GoldType = goldType;
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
                    throw new ArgumentException("Unreachable5!");
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
            public void Gain(ResourceType.BEValue type, float add)
            {
                if (type == CommonType)
                {
                    Common += add;
                }
                else if (type == GoldType)
                {
                    Gold += add;
                }
                else
                {
                    throw new ArgumentException("Unreachable4!");
                }
            }
        }
        private Dictionary<ResourceType.BEValue, ResourceTemplate> _resources = new();
        public Resource()
        {
            Type type = ResourceType.Instance.GetType();
            FieldInfo? field = type.BaseType?.BaseType?.GetField("_enumDict", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
            {
                throw new ArgumentException("Unreachable2!");//不应到达这里
            }
            var dict = field.GetValue(null) as Dictionary<string, ResourceType.BEValue>;
            if (dict == null)
            {
                throw new ArgumentException("Unreachable3!");//不应到达这里
            }
            List<string> enumNames = dict.Keys.ToList();
            string prefix = "Gold_";
            List<string> golds = enumNames.Where(e => e.StartsWith(prefix)).ToList();
            enumNames.RemoveAll(e => golds.Contains(e));
            foreach (var gold in golds)
            {
                string commonName = gold.Remove(0, prefix.Length);
                if (enumNames.Contains(commonName))
                {
                    var shareTemplate = new ResourceTemplate(dict[commonName], dict[gold]);
                    _resources[dict[commonName]] = shareTemplate;
                    _resources[dict[gold]] = shareTemplate;
                    enumNames.Remove(commonName);
                }
                else
                {
                    throw new ArgumentException($"ResourceType {gold} has no paired general resourceType!");
                }
            }
            foreach (var rest in enumNames)
            {
                var template = new ResourceTemplate(dict[rest], dict[rest]);
                _resources[dict[rest]] = template;
            }
        }
        public bool Check(ResourceType.BEValue type, float need, bool ifCommonOnly = false)
        {
            return _resources[type].Check(need, ifCommonOnly);
        }
        public void Use(ResourceType.BEValue type, float need, bool ifCommonOnly = false)
        {
            _resources[type].Use(need, ifCommonOnly);
        }
        public void Gain(ResourceType.BEValue type, float need)
        {
            _resources[type].Gain(type, need);
        }
        public float Query(ResourceType.BEValue type)
        {
            if (type == _resources[type].CommonType)
            {
                return _resources[type].Common;
            }
            else
            {
                return _resources[type].Gold;
            }
        }
        public float QueryAll(ResourceType.BEValue type)
        {
            return _resources[type].Gold + _resources[type].Common;
        }
        public float QuerySpecific(ResourceType.BEValue type)
        {
            float res = 0;
            foreach (var name in _resources.Keys)
            {
                if( name == ResourceType.Instance.Iron() ||
                    name == ResourceType.Instance.Gold_Iron() ||
                    name == ResourceType.Instance.Space() ||
                    name == ResourceType.Instance.Space())
                {
                    continue;
                }
                res +=  _resources[type].Gold + _resources[type].Common;
            }
            return res;
        }
        public List<(string name, float quantity)> GetView()
        {
            List<(string name, float quantity)> view = new();
            foreach(var key in _resources.Keys)
            {
                view.Add((key.ToString(), Query(key)));
            }
            return view;
        }
    }
}
