namespace Blacksmith.Backend.SkillPackages.Core
{
    public abstract class MainProfession : SkillPackageBase
    {
        public override PackageType PackageType { get; protected set; } = PackageType.Main;
        public MainProfession() : base(PackageType.Main)
        {

        }
    }
}
