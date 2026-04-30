using System.Reflection;
namespace ClapInfra.ClapProfession
{
    public interface ISkillPackage<TISkillContext, TDSLSourceFile>
    {
        public List<string> AvailableSkillNames { get; }
        public Dictionary<string, Func<TISkillContext, bool>> SkillChecker { get; }
        public Dictionary<string, Func<TISkillContext, TDSLSourceFile>> SkillSourceFileGenerator { get; }
        public abstract TDSLSourceFile PassiveSkill(TISkillContext sc);
    }
    public enum PackageType
    {
        Main,
        Modifier
    }
    public abstract class ClapSkillPackage<TISkillContext, TDSLSourceFile> 
        : ISkillPackage<TISkillContext, TDSLSourceFile>
    {
        public abstract PackageType PackageType { get; protected set; }
        private readonly List<string> _availableSkillNames = new();
        private readonly Dictionary<string, Func<TISkillContext, bool>> _skillChecker = new();
        private readonly Dictionary<string, Func<TISkillContext, TDSLSourceFile>> _skillSourceFileGenerator = new();

        public List<string> AvailableSkillNames => _availableSkillNames;
        public Dictionary<string, Func<TISkillContext, bool>> SkillChecker => _skillChecker;
        public Dictionary<string, Func<TISkillContext, TDSLSourceFile>> SkillSourceFileGenerator => _skillSourceFileGenerator;
        protected ClapSkillPackage(PackageType packageType)
        {
            var type = GetType();
            // 获取所有私有静态方法
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var method in methods)
            {
                // 仅处理以 "Check" 结尾的方法
                if (!method.Name.EndsWith("Check"))
                    continue;

                // 验证方法签名：bool (TISkillContext)
                if (method.ReturnType != typeof(bool) ||
                    method.GetParameters().Length != 1 ||
                    method.GetParameters()[0].ParameterType != typeof(TISkillContext))
                    continue;

                // 提取技能名（去掉 "Check" 并转小写）
                string skillName = method.Name.Substring(0, method.Name.Length - "Check".Length).ToLowerInvariant();

                // 查找对应的生成方法（方法名与技能名一致，忽略大小写）
                var generatorMethod = methods.FirstOrDefault(m =>
                    m.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase) &&
                    m.ReturnType == typeof(TDSLSourceFile) &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(TISkillContext));

                if (generatorMethod == null)
                {
                    // 可以记录日志或抛出异常，这里选择忽略不完整的技能对
                    continue;
                }

                // 创建委托
                var checkDelegate = (Func<TISkillContext, bool>)Delegate.CreateDelegate(
                    typeof(Func<TISkillContext, bool>), this, method);
                var generatorDelegate = (Func<TISkillContext, TDSLSourceFile>)Delegate.CreateDelegate(
                    typeof(Func<TISkillContext, TDSLSourceFile>), this, generatorMethod);

                // 添加到集合
                _availableSkillNames.Add(skillName);
                _skillChecker.Add(skillName, checkDelegate);
                _skillSourceFileGenerator.Add(skillName, generatorDelegate);
            }
            PackageType = packageType;
            if (PackageType == PackageType.Main)
            {
                AddModOnInit();
            }
        }
        protected abstract void AddModOnInit();
        public abstract TDSLSourceFile PassiveSkill(TISkillContext sc);
    }

}
