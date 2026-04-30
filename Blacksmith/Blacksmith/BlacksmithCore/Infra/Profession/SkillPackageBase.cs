using BlacksmithCore.Backend.SkillPackages;
using ClapInfra.ClapProfession;

namespace BlacksmithCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;
    public abstract class SkillPackageBase 
        : ClapSkillPackage<ISkillContext, DSL.SourceFile>
    {
        protected override void AddModOnInit() => ProfessionRegistry.AddModOnInit(this);
        protected SkillPackageBase(PackageType packageType) : base(packageType)
        {
        }
        public override DSL.SourceFile PassiveSkill(ISkillContext sc)
        {
            return new(sc.Self);
        }
    }
}
