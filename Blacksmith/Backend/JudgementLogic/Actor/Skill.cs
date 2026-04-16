using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.SkillPackages.Core;
using Blacksmith.Backend.SkillPackages.Logic;
using Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions;
namespace Blacksmith.Backend.JudgementLogic.Actor
{
    using DSL = DSLforSkillLogic;
    public enum SkillDeclareResult
    {
        Success,
        Illegal,
        Rejected
    }
    public class Skill
    {
        private class Package
        {
            public string Name { get; }
            public ISkillPackage SkillPackage { get; }
            public bool IsActive { get; set; } = true;
            public Package(ISkillPackage skillpackage)
            {
                Name = skillpackage.Name;
                SkillPackage = skillpackage;
            }
        }
        private List<Package> _packages = new()
        {
            new(new Common())
        };
        public void AddPackage(ISkillPackage skillPacakge)
        {
            _packages.Add(new(skillPacakge));
        }
        public void AddSkill(string packageName, string skillName)
        {
            _packages.Find(p => p.Name == packageName)?.SkillPackage.AvailableSkillNames.Add(skillName);
        }
        public void RemoveSkill(string packageName, string skillName)
        {
            _packages.Find(p => p.Name == packageName)?.SkillPackage.AvailableSkillNames.Remove(skillName);
        }
        public SkillDeclareResult TryDeclare(string skillName, ISkillContext sc)
        {
            foreach(var package in _packages)
            {
                if (!package.IsActive || !package.SkillPackage.AvailableSkillNames.Contains(skillName))
                {
                    continue;
                }
                if (package.SkillPackage.SkillChecker[skillName](sc))
                {
                    return SkillDeclareResult.Success;
                }
                else
                {
                    return SkillDeclareResult.Rejected;
                }
            }
            return SkillDeclareResult.Illegal;
        }
        public DSL.SourceFile Declare(string skillName, ISkillContext sc)
        {
            foreach (var package in _packages)
            {
                if (!package.IsActive || !package.SkillPackage.AvailableSkillNames.Contains(skillName))
                {
                    continue;
                }
                return package.SkillPackage.SkillSourceFileGenerator[skillName](sc);
            }
            throw new ArgumentException("Unreachable!");
        }
        public List<DSL.SourceFile> GetPassiveSkill(ISkillContext sc)
        {
            return _packages.Select(p => p.SkillPackage.PassiveSkill(sc)).ToList();
        }
        public List<string> GetAvailableSkillNames()
        {
            return _packages
                .Where(p => p.IsActive)
                .SelectMany(p => p.SkillPackage.AvailableSkillNames)
                .ToList();
        }
        public List<string> GetActivePackageNames()
        {
            return _packages
                .Where(p => p.IsActive)
                .Select(p => p.Name)
                .ToList();
        }
    }
}
