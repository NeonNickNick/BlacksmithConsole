using BlacksmithCore.Backend.SkillPackages;
using ClapInfra.ClapProfession;

namespace BlacksmithCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;
    public abstract class MainProfession : SkillPackageBase, ISkillPackage
    {
        public override PackageType PackageType { get; protected set; } = PackageType.Main;
        
        public MainProfession() : base(PackageType.Main)
        {

        }
        
    }
}
