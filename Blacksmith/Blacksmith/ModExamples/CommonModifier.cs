using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.SkillPackages;
using BlacksmithCore.Backend.SkillPackages.BuitinProfessions;
using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.Profession;

namespace ModExamples
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    [IsProfessionModifier(nameof(Common))]
    public class CommonModifier : ProfessionModifier
    {
        private bool HolyBookCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 2f);
        }
        private DSL.SourceFile HolyBook(ISkillContext sc)
        {
            sc.Self.Focus.Skill.AddPackage(new HolyBook());
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    Common.ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }
    }
}
