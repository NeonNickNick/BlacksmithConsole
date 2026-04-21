using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.SkillPackages.Core;
using Blacksmith.Backend.SkillPackages.Logic;
using Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions;
using Blacksmith.Infra.Attributes;

namespace ModExamples.HolyBookMod
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
