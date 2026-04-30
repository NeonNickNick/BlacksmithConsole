using System.Reflection;
using BlacksmithCore.Backend.SkillPackages;
using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Enum;
using BlacksmithCore.Infra.Profession;
using ClapInfra.ClapEnum;
using ClapInfra.ClapProfession;
namespace BlacksmithCore.Infra
{
    using DSL = DSLforSkillLogic;
    public static class PluginLoader
    {
        private static readonly List<Assembly> _cache = new();
        public static void Initialize(string folderPath = ".")
        {
            if (!Directory.Exists(folderPath))
                return;

            foreach (var dll in Directory.GetFiles(folderPath, "*.dll"))
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

            LoadBlacksmithEnums();
            LoadProfessions();
        }
        private static void LoadBlacksmithEnums()
        {
            //先注册所有BlacksmithEnum
            var BlacksmithEnumPlugins = PluginLoader.LoadByType<IBlacksmithEnum>();

            foreach (var plugin in BlacksmithEnumPlugins)
            {
                BlacksmithEnumRegistry.RegistBlacksmithEnum(plugin.GetType(), plugin);
            }
            //这里扩展方法情形稍微复杂一些
            //在刚才，BlacksmithEnum反射已经处理好定义，接下来只需要加入Modifier
            PluginLoader.LoadBlacksmithEnumModifiers();
        }
        private static void LoadProfessions()
        {
            //先注册Mod包名
            var ModProfessionPlugins = PluginLoader.LoadByType<SkillPackageBase>();
            foreach (var plugin in ModProfessionPlugins)
            {
                if (plugin.PackageType == PackageType.Main)
                {
                    ProfessionRegistry.RegistProfessionName(plugin.GetType().Name);
                }
            }
            //接下来记录Mod对已有包的修改，最重要的是给Common包扩展技能，否则无法使用Mod职业
            foreach (var plugin in ModProfessionPlugins)
            {
                if (plugin.PackageType == PackageType.Modifier)
                {
                    var metaData = plugin.GetType().GetCustomAttribute<IsProfessionModifier>();
                    if (metaData == null)
                    {
                        return;
                    }
                    ProfessionRegistry.RegistProfessionModifier(metaData.TargetName, plugin);
                }
            }
        }
        private static List<T> LoadByType<T>()
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
        private static void LoadBlacksmithEnumModifiers()
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
                        ProcessBlacksmithEnumModifiers(type);
                    }
                }
                catch
                {
                    Console.WriteLine($"加载{assembly}失败");
                }
            }
            ClapEnum.CloseFactory();
        }
        private static void ProcessBlacksmithEnumModifiers(Type type)
        {
            var supportedEnumDict = BlacksmithEnumRegistry.SupportedEnumDict;
            var eeValueTypeDict = BlacksmithEnumRegistry.BEValueTypeDict;
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                var metaData = method.GetCustomAttribute<IsBlacksmithEnumMember>();
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