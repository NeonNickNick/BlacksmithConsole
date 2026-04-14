using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Defenses;
using Blacksmith.Backend.JudgementLogic.Entities;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class Warlock : SkillPackageBase
    {
        public override string Name => "warlock";
        public Warlock()
        {
            InitializeSkills();
            AvailableSkillNames.Remove("midastouch");
        }
        public override DSL.SourceFile PassiveSkill(ISkillContext sc)
        {
            return new(sc.Self);
        }
        private static bool MagicCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1);
        }
        private static DSL.SourceFile Magic(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Iron)
                .WriteResource(1, ResourceType.Magic);
            return DSL.Create(sc.Self, pen);
        }

        private static bool MagicAttackCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Self.Focus.Resource.Check(ResourceType.Magic, sc.Param);
        }
        private static DSL.SourceFile MagicAttack(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteAttack(2 * sc.Param, AttackType.Physical, delayRounds: 0)
                .WriteAttack(2 * sc.Param, AttackType.Physical, delayRounds: 1)
                .WriteAttack(2 * sc.Param, AttackType.Physical, delayRounds: 2);
            return DSL.Create(sc.Self, pen);
        }

        private static bool MuteCheck(ISkillContext sc) => true;
        private static DSL.SourceFile Mute(ISkillContext sc)
        {
            Pen pen = sf => sf
               .WriteEffect(EffectType.AfterTransport, new(){ EffectTag.Debuff}, EffectTargetType.Enemy, 0, 1,
               (ActorSet source, Body main, EffectEntity effectEntity) =>
               {
                   main.TurnContext.ResourceResolutions.RemoveAll(r => r.Type == ResourceType.Space || r.Type == ResourceType.Time);
               });
            return DSL.Create(sc.Self, pen);
        }

        private static bool SacrificeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Health.HP > 1;
        }
        private static DSL.SourceFile Sacrifice(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    source.Focus.Health.LoseHP(1);
                    source.Focus.Health.LoseMHP(1);
                })
                .WriteDefense(7, new RealReduction())
                .WriteResource(2, ResourceType.Iron);
            return DSL.Create(sc.Self, pen);
        }

        private static bool AlchemyCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 2);
        }
        private static DSL.SourceFile Alchemy(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Iron)
                .WriteFree(source =>
                {
                    source.Focus.Skill.AddSkill("warlock", "midastouch");
                    source.Focus.Skill.RemoveSkill("warlock", "alchemy");
                });
            return DSL.Create(sc.Self, pen);
        }

        private static bool MidasTouchCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Resource.Check(ResourceType.Iron, 1, true);
        }

        private static DSL.SourceFile MidasTouch(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Iron, true)
                .WriteResource(5, ResourceType.GoldIron);
            return DSL.Create(sc.Self, pen);
        }
    }
}