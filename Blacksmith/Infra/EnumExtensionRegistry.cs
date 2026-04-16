namespace Blacksmith.Infra
{
    public static class EnumExtensionRegistry
    {
        public static IReadOnlyDictionary<Type, ExtensibleEnum> SupportedEnumDict = new Dictionary<Type, ExtensibleEnum>()
        {
            { typeof(TestType), TestType.Instance }
        };
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
        public static void RegistEnumExtension(ExtensibleEnum targetEnum, string name, int priority)
        {
            targetEnum.Create(name, priority);
        }

    }
}
