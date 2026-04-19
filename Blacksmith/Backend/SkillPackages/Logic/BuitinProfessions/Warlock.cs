using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.JudgementLogic.Entities;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions
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
                .WriteResource(2, ResourceType.Instance.Iron());
            return DSL.Create(sc.Self, pen);
        }

        private bool AlchemyCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Instance.Iron(), 2);
        }
        private DSL.SourceFile Alchemy(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    source.Focus.Skill.AddSkill("warlock", "midastouch");
                    source.Focus.Skill.RemoveSkill("warlock", "alchemy");
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