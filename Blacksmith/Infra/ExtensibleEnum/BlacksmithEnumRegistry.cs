namespace Blacksmith.Infra.ExtensibleEnum
{
    public static class BlacksmithEnumRegistry
    {
        private static Dictionary<Type, BlacksmithEnum> _supportedEnumDict = new();
        public static IReadOnlyDictionary<Type, BlacksmithEnum> SupportedEnumDict 
            => _supportedEnumDict;
        private static Dictionary<Type, Type>? _EEValueTypeDict = null;
        public static IReadOnlyDictionary<Type, Type> EEValueTypeDict
        {
            get
            {
                if (_EEValueTypeDict == null) {
                    _EEValueTypeDict = SupportedEnumDict.ToDictionary(s => s.Key, s => s.Value.GetEEValueType());
                }
                return _EEValueTypeDict;
            }
        }
        public static void RegistBlacksmithEnum(Type type, BlacksmithEnum instance)
        {
            if(!SupportedEnumDict.TryGetValue(type, out var value))
            {
                _supportedEnumDict[type] = instance;
            }
            else
            {
                throw new ArgumentException($"BlacksmithEnum {type} already exists! Expansion addition failed!");
            }
        }
        public static void RegistBlacksmithEnumModifier(BlacksmithEnum targetEnum, string name, int priority)
        {
            targetEnum.Create(name, priority);
        }

    }
}
