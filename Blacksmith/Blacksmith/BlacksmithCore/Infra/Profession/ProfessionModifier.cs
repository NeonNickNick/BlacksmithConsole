using BlacksmithCore.Backend.SkillPackages;
using ClapInfra.ClapProfession;

namespace BlacksmithCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;
    public abstract class ProfessionModifier : SkillPackageBase
    {
        public override PackageType PackageType { get; protected set; } = PackageType.Modifier;
        public ProfessionModifier() : base(PackageType.Main)
        {

        }
    }
}
