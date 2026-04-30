using BlacksmithCore.Backend.JudgementLogic.Actor;
using BlacksmithCore.Backend.JudgementLogic.Core;
using BlacksmithCore.Backend.JudgementLogic.Entities;
using BlacksmithCore.Backend.JudgementLogic.Judgement;
using BlacksmithCore.Backend.JudgementLogic.Judgement.Core;
using BlacksmithCore.Backend.JudgementLogic.TurnContexts;
namespace BlacksmithCore.Backend.SkillPackages
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public static class DSLforSkillLogic
    {
        public class SourceFile
        {
            protected enum SentenceType
            {
                Attack,
                Defense,
                Resource,
                Effect,
                Recovery,
                Free
            }
            protected enum StructureType
            {
                Main,
                Rhetoric
            }
            protected class Sentence
            {
                public Action<ActorSet> Structure { get; }
                public SentenceType SentenceType { get; }
                public StructureType StructureType { get; }
                public Sentence? BindSentence { get; }
                public Sentence(Action<ActorSet> structure, SentenceType sentenceType, StructureType structureType, Sentence? bindSentence = null)
                {
                    Structure = structure;
                    SentenceType = sentenceType;
                    StructureType = structureType;
                    BindSentence = bindSentence;
                }
            }

            protected readonly ActorSet _owner;
            protected List<Sentence> _sentences = new();
            protected Stack<Sentence> _rhetoricCache = new();
            protected Dictionary<DynamicJudgeRuleName.BEValue, List<Mutation>> _mutationsOnCompile = new();
            protected SourceFile(SourceFile origin)
            {
                _owner = origin._owner;
                _sentences = origin._sentences;
                _rhetoricCache = origin._rhetoricCache;
                _mutationsOnCompile = origin._mutationsOnCompile;
            }
            public SourceFile(ActorSet owner)
            {
                _owner = owner;
            }
            public Intent Compile(Judger? judger = null)
            {
                List<Sentence> sentences = new(_sentences);
                int n = _rhetoricCache.Count;
                for (int i = 0; i < n; ++i)
                {
                    var rhetoric = _rhetoricCache.Pop();
                    int index = sentences.IndexOf(rhetoric.BindSentence!) + 1;
                    sentences.Insert(index, new(rhetoric.Structure, rhetoric.SentenceType, StructureType.Rhetoric));
                }
                Action<ActorSet> result = (a) => { };
                if (judger != null)
                {
                    foreach (var pair in _mutationsOnCompile)
                    {
                        judger.JudgeRuleManager.RegistJudgeRuleDynamic(pair.Key, pair.Value);
                        judger.JudgeRuleManager.AddJudgeRule(_owner, pair.Key);
                    }
                }
                foreach (var sentence in sentences)
                {
                    result += sentence.Structure;
                }
                return new() { Execute = result };
            }
            public SourceFile WriteFree(Action<ActorSet> action)
            {
                _sentences.Add(new(action, SentenceType.Free, StructureType.Main));
                return this;
            }
            public AttackFile WriteAttack(
                float power,
                AttackType.BEValue attackType,
                float APFactor = 1,
                int delayRounds = 0
            )
            {
                _sentences.Add(new((source) =>
                {
                    var resolution = new AttackResolution
                    {
                        Source = source,
                        DelayRounds = delayRounds,
                        Type = attackType,
                        Power = power
                    };
                    resolution.Execute = (target) =>
                    {
                        Body main = target.Focus;
                        if (resolution.Power <= 0f)
                        {
                            return;
                        }
                        bool ifHitArmor = false;
                        if (resolution.Type != AttackType.Instance.Real())
                        {
                            var defenses = main.Defense.Get();
                            var APList = new List<DefenseType.BEValue>()
                            {
                                DefenseType.Instance.ThornReduction(),
                                DefenseType.Instance.CommonReduction(),
                                DefenseType.Instance.RockArmor(),
                                DefenseType.Instance.ReadlArmor(),
                                DefenseType.Instance.CommonArmor()
                            };
                            var armorList = new List<DefenseType.BEValue>()
                            {
                                DefenseType.Instance.RockArmor(),
                                DefenseType.Instance.ReadlArmor(),
                                DefenseType.Instance.CommonArmor()
                            };
                            foreach (var temp in defenses)
                            {
                                if (!ifHitArmor && armorList.Contains(temp.Type))
                                {
                                    ifHitArmor = true;
                                    resolution.RunStage(AttackStage.OnHitArmorFirstTime, main);
                                }
                                if (APList.Contains(temp.Type))
                                {
                                    resolution.Power *= APFactor;
                                }
                                var res = temp.Work(resolution.Source.Focus, main, (int)resolution.Power, resolution.Type);
                                resolution.Power = res.Item1;
                                resolution.TotalDamage += res.Item2;
                                if (APList.Contains(temp.Type))
                                {
                                    resolution.Power = MathF.Ceiling(resolution.Power / APFactor);
                                }
                                if (resolution.Power <= 0f)
                                {
                                    resolution.RunStage(AttackStage.OnEnd, main);
                                    return;
                                }
                            }
                        }
                        if (!ifHitArmor)
                        {
                            resolution.RunStage(AttackStage.OnHitArmorFirstTime, main);
                        }
                        main.Health.LoseHP((int)resolution.Power);
                        resolution.TotalDamage += (int)resolution.Power;
                        resolution.RunStage(AttackStage.OnEnd, main);
                    };
                    resolution.Source.Focus.TurnContext.WriteResolution(resolution);
                }, SentenceType.Attack, StructureType.Main));
                return new(this);
            }

            public SourceFile WriteRecovery(int power)
            {
                _sentences.Add(new((source) =>
                {
                    source.Focus.Health.GainHP(power);
                }, SentenceType.Recovery, StructureType.Main));
                return new(this);
            }
            public SourceFile WriteDefense(
                float power,
                DefenseBase defense,
                int delayRounds = 0
            )
            {
                _sentences.Add(new((source) =>
                {
                    var resolution = new DefenseResolution()
                    {
                        DelayRounds = delayRounds,
                        Defense = defense,
                        Power = power
                    };
                    resolution.Execute = (target) =>
                    {
                        defense.Power = (int)resolution.Power;
                        target.Focus.Defense.Add(resolution.Defense);
                    };
                    source.Focus.TurnContext.WriteResolution(resolution);
                }, SentenceType.Defense, StructureType.Main));
                return new(this);
            }
            public SourceFile WriteResource(
                float power,
                ResourceType.BEValue type,
                int delayRounds = 0
            )
            {
                _sentences.Add(new((source) =>
                {
                    var resolution = new ResourceResolution()
                    {
                        DelayRounds = delayRounds,
                        Power = power,
                        Type = type
                    };
                    resolution.Execute = (target) =>
                    {
                        target.Focus.Resource.Gain(resolution.Type, resolution.Power);
                    };
                    source.Focus.TurnContext.WriteResolution(resolution);
                }, SentenceType.Resource, StructureType.Main));
                return new(this);
            }
            public SourceFile WriteEffect(
                EffectType.BEValue type,
                EffectTargetType.BEValue targetType,
                float power,
                int duration,
                Action<ActorSet, Body, EffectEntity> effectAction
                )
            {
                _sentences.Add(new((source) =>
                {
                    var resolution = new EffectResolution(type, targetType, power);
                    resolution.Execute = (target) =>
                    {
                        Body main = target.Focus;
                        AddEffectEntity(source, main, type, duration, resolution, effectAction);
                        resolution.RunStage(EffectStage.OnSuccessfullyAdded, source, main);
                    };
                    source.Focus.TurnContext.WriteResolution(resolution);
                }, SentenceType.Effect, StructureType.Main));
                return new(this);
            }
            public SourceFile UseResource(float need, ResourceType.BEValue type, bool ifCommonOnly = false)
            {
                return WriteFree(source => source.Focus.Resource.Use(type, need, ifCommonOnly));
            }
            public SourceFile LinkJudgeRuleDynamic(
                DynamicJudgeRuleName.BEValue ruleKey,
                List<Mutation> mutations)
            {
                _mutationsOnCompile[ruleKey] = mutations;
                return this;
            }
        }

        public class DefenseFile : SourceFile
        {
            public DefenseFile(SourceFile self) : base(self)
            {
            }
        }
        public class RecoveryFile : SourceFile
        {
            public RecoveryFile(SourceFile self) : base(self)
            {
            }
        }
        public class AttackFile : SourceFile
        {
            public AttackFile WithFree(AttackStage stage, Action<ActorSet?, Body, AttackResolution> action)
            {
                _rhetoricCache.Push(new((source) =>
                {
                    var list = source.Focus.TurnContext.AttackResolutions;
                    if (list.Count == 0)
                    {
                        return;
                    }
                    var last = list[^1];
                    last.AddStage(stage, action);
                }, SentenceType.Attack, StructureType.Rhetoric, _sentences[^1]));
                return this;
            }
            public AttackFile WithBloodSuck(float percent)
            {
                var suck = (ActorSet? source, Body target, AttackResolution resolution) =>
                {
                    source?.Focus.Health.GainHP((int)MathF.Ceiling(resolution.Power * percent));
                };
                return WithFree(AttackStage.OnEnd, suck);
            }
            public AttackFile WithInterupt()
            {
                var interuptList = new List<ResourceType.BEValue>()
                {
                    ResourceType.Instance.Iron(),
                    ResourceType.Instance.Gold_Iron(),
                    ResourceType.Instance.Magic()
                };
                var interupt = (ActorSet? source, Body target, AttackResolution resolution) =>
                {
                    target.TurnContext.ResourceResolutions.RemoveAll(r => interuptList.Contains(r.Type));
                };
                return WithFree(AttackStage.OnHitArmorFirstTime, interupt);
            }
            public AttackFile(SourceFile self) : base(self)
            {
            }
        }
        public class ResourceFile : SourceFile
        {
            public ResourceFile(SourceFile self) : base(self)
            {
            }
        }
        public class EffectFile : SourceFile
        {
            public EffectFile(SourceFile self) : base(self)
            {
            }
        }
        /// <summary>
        /// 专用于外部产生的孤立效果生成
        /// </summary>
        public static void AddEffectEntity(Body main, EffectType.BEValue type, int duration, IResolution resolution, Action<Body, EffectEntity> effectAction)
        {
            EffectEntity effect = new EffectEntity(type, duration, resolution);
            effect.Execute = (body) => effectAction(body, effect);
            main.Effect.Add(effect);
        }
        /// <summary>
        /// 专用于被EffectResolution引导的效果生成
        /// </summary>
        public static void AddEffectEntity(ActorSet source, Body main, EffectType.BEValue type, int duration, EffectResolution resolution, Action<ActorSet, Body, EffectEntity> effectAction)
        {
            EffectEntity effect = new EffectEntity(type, duration, resolution);
            effect.Execute = (body) => effectAction(source, body, effect);
            main.Effect.Add(effect);
        }
        public static SourceFile Create(ActorSet source, Pen Pen)
        {
            var sourceFile = new SourceFile(source);
            return Pen(sourceFile);
        }
    }
}
