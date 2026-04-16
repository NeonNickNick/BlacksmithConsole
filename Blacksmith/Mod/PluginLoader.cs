using System.Reflection;
using Blacksmith.Infra;
namespace Blacksmith.Mod
{
    public static class PluginLoader
    {
        public static List<T> LoadProfessionPlugins<T>(string folderPath = ".")
        {
            var plugins = new List<T>();

            if (!Directory.Exists(folderPath))
                return plugins;

            foreach (var dll in Directory.GetFiles(folderPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dll);

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
                catch (BadImageFormatException)
                {
                    // 不是有效的 .NET 程序集，跳过
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载 {dll} 失败: {ex.Message}");
                }
            }

            return plugins;
        }
        public static void LoadEnumExtensionPlugins(string folderPath = ".")
        {
            foreach (string dllPath in Directory.GetFiles(folderPath, "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    var staticClasses = assembly.GetTypes()
                        .Where(t => t.IsClass
                                    && t.IsAbstract
                                    && t.IsSealed  // 静态类的特征
                                    && t.Name.EndsWith("EnumExtension", StringComparison.OrdinalIgnoreCase));

                    foreach (Type type in staticClasses)
                    {
                        ProcessEnumExtensionPlugins(type);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载 DLL 失败: {dllPath}, 错误: {ex.Message}");
                }
            }
            ExtensibleEnum.CloseFactory();
        }
        private static void ProcessEnumExtensionPlugins(Type type)
        {
            var supportedEnumDict = EnumExtensionRegistry.SupportedEnumDict;
            var eeValueTypeDict = EnumExtensionRegistry.EEValueTypeDict;
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                             .Where(f => f.IsInitOnly)
                             .Where(f => f.FieldType == typeof(int))
                             .ToList();
            foreach (var method in methods)
            {
                var temp = method.GetParameters()[0].ParameterType;
                if (method.GetParameters().Length != 1 ||
                    !supportedEnumDict.Keys.Contains(temp) ||
                    method.ReturnType != eeValueTypeDict[temp])
                {
                    continue;
                }
                string methodName = method.Name;
                string prefix = "_" + methodName.ToLower();
                var fieldNames = fields.Select(f => f.Name).Where(f => f.StartsWith(prefix)).ToList();
                if (fieldNames.Count != 1)
                {
                    continue;
                }
                int priority = 0;
                try
                {
                    priority = int.Parse(fieldNames[0].Remove(0, prefix.Length));
                }
                catch
                {
                    continue;
                }
                EnumExtensionRegistry.RegistEnumExtension(supportedEnumDict[temp], method.Name, priority);
            }
        }
    }
}