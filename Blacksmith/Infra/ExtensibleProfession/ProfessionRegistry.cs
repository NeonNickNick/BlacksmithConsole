using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Infra.ExtensibleProfession
{
    public static class ProfessionRegistry
    {
        public static readonly List<string> Professions = new();
        private static Dictionary<string, List<SkillPackageBase>> _modifiers = new();
        public static void RegistProfessionName(string professionName)
        {
            if (Professions.Contains(professionName))
            {
                throw new ArgumentException($"Profession \"{professionName}\" already exists! Expansion addition failed!");
            }
            else
            {
                Professions.Add(professionName);
                Console.WriteLine($"Successfully added the extended profession \"{professionName}\"!");
            }
        }
        public static void RegistProfessionModifier(string targetName, SkillPackageBase modifier)
        {
            if (!_modifiers.TryGetValue(targetName, out var value))
            {
                _modifiers[targetName] = new();
            }
            _modifiers[targetName].Add(modifier);
        }
        public static void AddModeOnInit(SkillPackageBase package)
        {
            if (_modifiers.TryGetValue(package.GetType().Name, out var modifiers))
            {
                foreach (var modifierOrigin in modifiers)
                {
                    var type = modifierOrigin.GetType();
                    SkillPackageBase modifier = (SkillPackageBase)Activator.CreateInstance(type)!;
                    package.AvailableSkillNames.AddRange(modifier.AvailableSkillNames);
                    foreach (var kv in modifier.SkillChecker)
                    {
                        package.SkillChecker[kv.Key] = kv.Value;
                    }
                    foreach (var kv in modifier.SkillSourceFileGenerator)
                    {
                        package.SkillSourceFileGenerator[kv.Key] = kv.Value;
                    }
                }
            }
        }
    }
}
