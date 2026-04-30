using BlacksmithCore.Backend.SkillPackages;
using ClapInfra.ClapProfession;

namespace BlacksmithCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;
    public interface ISkillPackage : ISkillPackage<ISkillContext, DSL.SourceFile>
    {
    }
}
