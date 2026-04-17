using Blacksmith.AI;
using Blacksmith.AI.Strategies;
using Blacksmith.Backend.SkillPackages.Core;
using Blacksmith.Mod;
using Blacksmith.Frontend;
using Blacksmith.Infra.ExtensibleProfession;
using Blacksmith.Infra.ExtensibleEnum;
namespace Blacksmith
{
    public static class Program
    {
        public static void Main()
        {
            PluginLoader.Initialize(".");
			LoadBlacksmithEnumModifierPlugins();
            LoadProfessionPlugins();
            Console.WriteLine(TestType.Instance.Physical()._priority);
			Console.WriteLine(TestType.Instance.Magical()._priority);

			List<IAIStrategy> strategies = new()
            {
                new BloodSigilStrategy()
            };
            ConsoleFrontend.Start(strategies);
        }
        private static void LoadBlacksmithEnumModifierPlugins()
        {
            //先注册所有BlacksmithEnum
            var BlacksmithEnumPlugins = PluginLoader.LoadPluginsByType<BlacksmithEnum>();
            
            foreach(var plugin in BlacksmithEnumPlugins)
            {
                BlacksmithEnumRegistry.RegistBlacksmithEnum(plugin.GetType(), plugin);
            }
            //这里扩展方法情形稍微复杂一些
            //在刚才，BlacksmithEnum反射已经处理好定义，接下来只需要加入Modifier
            PluginLoader.LoadBlacksmithEnumModifierPlugins();
        }
        private static void LoadProfessionPlugins()
        {
            //先注册Mod包名
            var ModProfessionPlugins = PluginLoader.LoadPluginsByType<SkillPackageBase>();
            foreach (var plugin in ModProfessionPlugins)
            {
                if (plugin.PackageType == PackageType.Main)
                {
                    ProfessionRegistry.RegistProfessionName(plugin.Name);
                }
            }
            //接下来记录Mod对已有包的修改，最重要的是给Common包扩展技能，否则无法使用Mod职业
            foreach (var plugin in ModProfessionPlugins)
            {
                if(plugin.PackageType == PackageType.Modifier)
                {
                    ProfessionRegistry.RegistProfessionModifier(plugin.Name, plugin);
                }
            }
        }
    }
}
