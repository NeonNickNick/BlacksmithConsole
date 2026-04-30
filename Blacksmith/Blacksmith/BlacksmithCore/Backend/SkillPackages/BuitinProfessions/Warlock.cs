using BlacksmithCore.Backend.JudgementLogic.Actor;
using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.JudgementLogic.Defenses;
using BlacksmithCore.Backend.JudgementLogic.Entities;
using BlacksmithCore.Backend.JudgementLogic.Judgement;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Backend.SkillPackages.BuitinProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public class Warlock : MainProfession
    {
        public Warlock()
        {
            AvailableSkillNames.Remove("midastouch");
        }
        private bool MagicCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 1);
        }
        private DSL.SourceFile Magic(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Magic());
            return DSL.Create(sc.Self, pen);
        }

        private bool MagicAttackCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Self.Focus.Resource.Check(ResourceType.Instance.Magic(), sc.Param);
        }
        private DSL.SourceFile MagicAttack(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param, ResourceType.Instance.Magic())
                .WriteAttack(2 * sc.Param, AttackType.Instance.Physical(), delayRounds: 0)
                .WriteAttack(2 * sc.Param, AttackType.Instance.Physical(), delayRounds: 1)
                .WriteAttack(2 * sc.Param, AttackType.Instance.Physical(), delayRounds: 2);
            return DSL.Create(sc.Self, pen);
        }

        private bool MuteCheck(ISkillContext sc) => true;
        private DSL.SourceFile Mute(ISkillContext sc)
        {
            Pen pen = sf => sf
               .WriteEffect(EffectType.Instance.AfterTransport(), EffectTargetType.Instance.Enemy(), 0, 1,
               (ActorSet source, Body main, EffectEntity effectEntity) =>
               {
                   main.TurnContext.ResourceResolutions.RemoveAll(r => r.Type == ResourceType.Instance.Space() || r.Type == ResourceType.Instance.Time());
               });
            return DSL.Create(sc.Self, pen);
        }

        private bool SacrificeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Health.HP > 1;
        }
        private DSL.SourceFile Sacrifice(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    source.Focus.Health.LoseHP(1);
                    source.Focus.Health.LoseMHP(1);
                })
                .WriteDefense(7, new RealReduction())
                .WriteResource(1.5f, ResourceType.Instance.Iron());
            return DSL.Create(sc.Self, pen);
        }

        private bool AlchemyCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 2.5f);
        }
        private DSL.SourceFile Alchemy(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    source.Focus.Skill.AddSkill("Warlock", "midastouch");
                    source.Focus.Skill.RemoveSkill("Warlock", "alchemy");
                });
            return DSL.Create(sc.Self, pen);
        }

        private bool MidasTouchCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 1, true);
        }

        private DSL.SourceFile MidasTouch(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron(), true)
                .WriteResource(5, ResourceType.Instance.Gold_Iron());
            return DSL.Create(sc.Self, pen);
        }
    }
}