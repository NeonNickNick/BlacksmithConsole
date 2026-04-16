using Blacksmith.AI;
using Blacksmith.AI.Strategies;
using Blacksmith.Backend.SkillPackages.Core;
using Blacksmith.Mod;
using Blacksmith.Frontend;
using Blacksmith.Infra;
namespace Blacksmith
{
    public static class Program
    {
        public static void Main()
        {
            LoadEnumExtensionPlugins();
            LoadProfessionPlugins();
            List<IAIStrategy> strategies = new()
            {
                new BloodSigilStrategy()
            };
            ConsoleFrontend.Start(strategies);
        }
        private static void LoadEnumExtensionPlugins()
        {
            PluginLoader.LoadEnumExtensionPlugins();
        }
        private static void LoadProfessionPlugins()
        {
            //先注册Mod包名
            var ModProfessionPlugins = PluginLoader.LoadProfessionPlugins<SkillPackageBase>(".");
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
