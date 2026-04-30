namespace BlacksmithCore.Infra.Enum
{
    public static class BlacksmithEnumRegistry
    {
        private static Dictionary<Type, IBlacksmithEnum> _supportedEnumDict = new();
        public static IReadOnlyDictionary<Type, IBlacksmithEnum> SupportedEnumDict
            => _supportedEnumDict;
        private static Dictionary<Type, Type>? _BEValueTypeDict = null;
        public static IReadOnlyDictionary<Type, Type> BEValueTypeDict
        {
            get
            {
                if (_BEValueTypeDict == null)
                {
                    _BEValueTypeDict = SupportedEnumDict.ToDictionary(s => s.Key, s => s.Value.GetBEValueType());
                }
                return _BEValueTypeDict;
            }
        }
        private static List<string> _names = new();
        public static void RegistBlacksmithEnum(Type type, IBlacksmithEnum instance)
        {
            if (!SupportedEnumDict.TryGetValue(type, out var value) && !_names.Contains(type.Name))
            {
                _supportedEnumDict[type] = instance;
                _names.Add(type.Name);
            }
            else
            {
                throw new ArgumentException($"IBlacksmithEnum {type} already exists! Expansion addition failed!");
            }
        }
        public static void RegistBlacksmithEnumModifier(IBlacksmithEnum targetEnum, string name, int priority)
        {
            targetEnum.Create(name, priority);
        }

    }
}
