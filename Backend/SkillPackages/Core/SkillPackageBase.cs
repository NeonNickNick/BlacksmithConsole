using System.Reflection;
using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.SkillPackages.Logic;

namespace Blacksmith.Backend.SkillPackages.Core
{
    using DSL = DSLforSkillLogic;
    public interface ISkillPackage
    {
        public string Name { get; }
        public List<string> AvailableSkillNames { get; }
        public IReadOnlyDictionary<string, Func<ISkillContext, bool>> SkillChecker { get; }
        public IReadOnlyDictionary<string, Func<ISkillContext, DSL.SourceFile>> SkillSourceFileGenerator { get; }
        public abstract DSL.SourceFile PassiveSkill(ISkillContext sc);
    }
    public abstract class SkillPackageBase : ISkillPackage
    {
        private readonly List<string> _availableSkillNames = new();
        private readonly Dictionary<string, Func<ISkillContext, bool>> _skillChecker = new();
        private readonly Dictionary<string, Func<ISkillContext, DSL.SourceFile>> _skillSourceFileGenerator = new();
        public abstract string Name { get; }
        public List<string> AvailableSkillNames => _availableSkillNames;
        public IReadOnlyDictionary<string, Func<ISkillContext, bool>> SkillChecker => _skillChecker;
        public IReadOnlyDictionary<string, Func<ISkillContext, DSL.SourceFile>> SkillSourceFileGenerator => _skillSourceFileGenerator;
        protected void InitializeSkills()
        {
            var type = this.GetType();
            // 获取所有私有静态方法
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var method in methods)
            {
                // 仅处理以 "Check" 结尾的方法
                if (!method.Name.EndsWith("Check"))
                    continue;

                // 验证方法签名：bool (ISkillContext)
                if (method.ReturnType != typeof(bool) ||
                    method.GetParameters().Length != 1 ||
                    method.GetParameters()[0].ParameterType != typeof(ISkillContext))
                    continue;

                // 提取技能名（去掉 "Check" 并转小写）
                string skillName = method.Name.Substring(0, method.Name.Length - "Check".Length).ToLowerInvariant();

                // 查找对应的生成方法（方法名与技能名一致，忽略大小写）
                var generatorMethod = methods.FirstOrDefault(m =>
                    m.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase) &&
                    m.ReturnType == typeof(DSL.SourceFile) &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(ISkillContext));

                if (generatorMethod == null)
                {
                    // 可以记录日志或抛出异常，这里选择忽略不完整的技能对
                    continue;
                }

                // 创建委托
                var checkDelegate = (Func<ISkillContext, bool>)Delegate.CreateDelegate(
                    typeof(Func<ISkillContext, bool>), method);
                var generatorDelegate = (Func<ISkillContext, DSL.SourceFile>)Delegate.CreateDelegate(
                    typeof(Func<ISkillContext, DSL.SourceFile>), generatorMethod);

                // 添加到集合
                _availableSkillNames.Add(skillName);
                _skillChecker.Add(skillName, checkDelegate);
                _skillSourceFileGenerator.Add(skillName, generatorDelegate);
            }
        }
        public abstract DSL.SourceFile PassiveSkill(ISkillContext sc);
    }
}
