using System.Reflection;
using Blacksmith.Infra.Attributes;
using Blacksmith.Infra.ExtensibleEnum;
namespace Blacksmith.Mod
{
    public static class PluginLoader
    {
        private static readonly List<Assembly> _cache = new();
        public static void Initialize(string folderPath = ".")
        {
            if (!Directory.Exists(folderPath))
                return;

            foreach(var dll in Directory.GetFiles(folderPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dll);
                    _cache.Add(assembly);
                }
                catch
                {
					Console.WriteLine($"加载 {dll} 失败");
				}
            }
        }
        public static List<T> LoadPluginsByType<T>()
        {
            var plugins = new List<T>();
            foreach (var assembly in _cache)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => typeof(T).IsAssignableFrom(t)
                                    && t.IsClass
                                    && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        // 创建实例
                        if (Activator.CreateInstance(type) is T plugin)
                            plugins.Add(plugin);
                    }
                }
                catch
                {
                    Console.WriteLine($"加载 {assembly} 失败");
                }
            }

            return plugins;
        }
        public static void LoadBlacksmithEnumModifierPlugins()
        {
            foreach (var assembly in _cache)
            {
                try
                {
                    var staticClasses = assembly.GetTypes()
                        .Where(t => t.IsClass
                                    && t.IsAbstract
                                    && t.IsSealed  // 静态类的特征
                                    && t.GetCustomAttribute<IsBlacksmithEnumModifier>() != null);

                    foreach (Type type in staticClasses)
                    {
						ProcessBlacksmithEnumModifierPlugins(type);
                    }
                }
                catch
                {
                    Console.WriteLine($"加载{assembly}失败");
                }
            }
            BlacksmithEnum.CloseFactory();
        }
        private static void ProcessBlacksmithEnumModifierPlugins(Type type)
        {
            var supportedEnumDict = BlacksmithEnumRegistry.SupportedEnumDict;
            var eeValueTypeDict = BlacksmithEnumRegistry.EEValueTypeDict;
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                var metaData = method.GetCustomAttribute<IsBlacksmithEnumMenberExtension>();
                var temp = method.GetParameters()[0].ParameterType;
                if (metaData == null ||
                    method.GetParameters().Length != 1 ||
                    !supportedEnumDict.Keys.Contains(temp) ||
                    method.ReturnType != eeValueTypeDict[temp])
                {
                    continue;
                }
                BlacksmithEnumRegistry.RegistBlacksmithEnumModifier(supportedEnumDict[temp], method.Name, metaData.Priority);
            }
        }
    }
}