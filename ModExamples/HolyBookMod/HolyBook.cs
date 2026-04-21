using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.JudgementLogic.Entities;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.TurnContexts;
using Blacksmith.Backend.SkillPackages.Core;
using Blacksmith.Backend.SkillPackages.Logic;
using ModExamples.HolyBookMod.Defense;
namespace ModExamples.HolyBookMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public class HolyBook : MainProfession
    {
        private bool CrossCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 0.5f);
        }
        private DSL.SourceFile Cross(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Cross());
            return DSL.Create(sc.Self, pen);
        }
        private bool PrayCheck(ISkillContext sc) => true;
        private DSL.SourceFile Pray(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(3, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }
        private bool ArkCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Cross(), 2f);
        }
        private DSL.SourceFile Ark(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Cross())
                .WriteAttack(8, AttackType.Instance.Physical())
                .WriteDefense(1, new PercentageReduction(baseline: 2));
            return DSL.Create(sc.Self, pen);
        }
        private int _blasphemyCount = 0;
        private bool BlasphemyCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Cross(), 1f);
        }
        private DSL.SourceFile Blasphemy(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Cross())
                .WriteAttack(2, AttackType.Instance.Real())
                .WriteDefense(2 + (int)MathF.Ceiling(_blasphemyCount / 3), new GreyHP())
                .WriteFree(a => _blasphemyCount++);
            return DSL.Create(sc.Self, pen);
        }
        private bool RebirthCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Cross(), 1f);
        }
        private DSL.SourceFile Rebirth(ISkillContext sc)
        {
            EffectResolution fakeResolution = new(
                EffectType.Instance.AfterResolutionWritten(),
                EffectTargetType.Instance.Self(),
                3);
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Cross())
                .WriteFree(source =>
                {
                    var entity = new EffectEntity(fakeResolution.Type, 3, fakeResolution);
                    entity.Execute = (Body body) =>
                    {
                        body.Health.GainHP(3);
                    };
                    source.Focus.Effect.Add(entity);
                })
                .WriteDefense(1, new PercentageReduction(baseline: 4), delayRounds: 0)
                .WriteDefense(1, new PercentageReduction(baseline: 4), delayRounds: 1)
                .WriteDefense(1, new PercentageReduction(baseline: 4), delayRounds: 2);
            return DSL.Create(sc.Self, pen);
        }
        private bool ExonerationCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Cross(), 1f);
        }
        private DSL.SourceFile Exoneration(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Cross())
                .WriteDefense(1, new PermanentRealReduction());
            return DSL.Create(sc.Self, pen);
        }

    }
}
