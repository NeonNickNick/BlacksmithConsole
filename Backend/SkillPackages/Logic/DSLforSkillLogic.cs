using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Entities;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;
using Blacksmith.Backend.JudgementLogic.TurnContexts;
namespace Blacksmith.Backend.SkillPackages.Logic
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public static class DSLforSkillLogic
    {
        public class SourceFile
        {
            private enum SentenceType
            {
                Attack,
                Defense,
                Resource,
                Effect,
                Recovery,
                Free
            }
            private enum StructureType
            {
                Main,
                Rhetoric
            }
            private class Sentence
            {
                public Action<ActorSet> Structure { get; }
                public SentenceType SentenceType { get; }
                public StructureType StructureType { get; }
                public Sentence BindSentence { get; }
                public Sentence(Action<ActorSet> structure, SentenceType sentenceType, StructureType structureType, Sentence bindSentence = null)
                {
                    Structure = structure;
                    SentenceType = sentenceType;
                    StructureType = structureType;
                    BindSentence = bindSentence;
                }
            }
            private List<Sentence> _sentences = new();
            private Stack<Sentence> _rhetoricCache = new();
            private List<string> _mutationsOnCompile = new();
            public Intent Compile(Judger judger = null)
            {
                List<Sentence> sentences = new(_sentences);
                int n = _rhetoricCache.Count;
                for (int i = 0; i < n; ++i)
                {
                    var rhetoric = _rhetoricCache.Pop();
                    int index = sentences.IndexOf(rhetoric.BindSentence) + 1;
                    sentences.Insert(index, new(rhetoric.Structure, rhetoric.SentenceType, StructureType.Rhetoric));
                }
                Action<ActorSet> result = null;
                if (judger != null)
                {
                    foreach (var key in _mutationsOnCompile)
                    {
                        judger.JudgeRuleManager.AddJudgeRule(key);
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
            public SourceFile WriteAttack(
                float power,
                AttackType attackType,
                float APFactor = 1,
                int delayRounds = 0
            )
            {
                _sentences.Add(new((ActorSet source) =>
                {
                    var resolution = new AttackResolution
                    {
                        DelayRounds = delayRounds,
                        Type = attackType,
                        Power = power
                    };
                    resolution.Execute = (ActorSet target) =>
                    {
                        Body main = target.Focus;
                        if (resolution.Power <= 0f)
                        {
                            return;
                        }
                        if (resolution.Type != AttackType.Real)
                        {
                            var defenses = main.Defense.Get();
                            resolution.RunStage(AttackStage.OnHitDefense, source, main);
                            
                            foreach (var temp in defenses)
                            {
                                if (temp.Type != DefenseType.RealReduction)
                                {
                                    resolution.Power *= APFactor;
                                }
                                resolution.Power = temp.Work(source.Focus, main, (int)resolution.Power, resolution.Type);
                                if (temp.Type != DefenseType.RealReduction)
                                {
                                    resolution.Power = MathF.Ceiling(resolution.Power / APFactor);
                                }
                                if (resolution.Power <= 0f)
                                {
                                    return;
                                }
                            }
                            
                        }
                        resolution.RunStage(AttackStage.OnHitBody, source, main);
                        main.Health.LoseHP((int)resolution.Power);
                    };
                    source.Focus.TurnContext.WriteResolution(resolution);
                }, SentenceType.Attack, StructureType.Main));
                return this;
            }
            public SourceFile OnHitDefense(Action<ActorSet, Body, AttackResolution> action)
            {
                _rhetoricCache.Push(new((ActorSet source) =>
                {
                    var list = source.Focus.TurnContext.AttackResolutions;
                    if (list.Count == 0)
                    {
                        return;
                    }
                    var last = list[^1];
                    last.AddStage(AttackStage.OnHitDefense, action);
                }, SentenceType.Attack, StructureType.Rhetoric, _sentences[^1]));
                return this;
            }
            public SourceFile OnHitBody(Action<ActorSet, Body, AttackResolution> action)
            {
                _rhetoricCache.Push(new((ActorSet source) =>
                {
                    var list = source.Focus.TurnContext.AttackResolutions;
                    if (list.Count == 0)
                    {
                        return;
                    }
                    var last = list[^1];
                    last.AddStage(AttackStage.OnHitBody, action);
                }, SentenceType.Attack, StructureType.Rhetoric, _sentences[^1]));
                return this;
            }
            public SourceFile WriteRecovery(int power)
            {
                _sentences.Add(new((ActorSet source) =>
                {
                    source.Focus.Health.GainHP(power);
                }, SentenceType.Recovery, StructureType.Main));
                return this;
            }
            public SourceFile WriteDefense(
                float power,
                DefenseBase defense,
                int delayRounds = 0
            )
            {
                _sentences.Add(new((ActorSet source) =>
                {
                    var resolution = new DefenseResolution()
                    {
                        DelayRounds = delayRounds,
                        Defense = defense,
                        Power = power
                    };
                    resolution.Execute = (ActorSet target) =>
                    {
                        defense.Power = (int)resolution.Power;
                        target.Focus.Defense.Add(resolution.Defense);
                    };
                    source.Focus.TurnContext.WriteResolution(resolution);
                }, SentenceType.Defense, StructureType.Main));
                return this;
            }
            public SourceFile WriteResource(
                float power,
                ResourceType type,
                int delayRounds = 0
            )
            {
                _sentences.Add(new((ActorSet source) =>
                {
                    var resolution = new ResourceResolution()
                    {
                        DelayRounds = delayRounds,
                        Power = power,
                        Type = type
                    };
                    resolution.Execute = (ActorSet target) =>
                    {
                        target.Focus.Resource.Gain(resolution.Type, (int)resolution.Power);
                    };
                    source.Focus.TurnContext.WriteResolution(resolution);
                }, SentenceType.Resource, StructureType.Main));
                return this;
            }
            public SourceFile WriteEffect(
                EffectType type,
                List<EffectTag> tags,
                EffectTargetType targetType,
                float power,
                int duration,
                Action<ActorSet, Body, EffectEntity> effectAction
                )
            {
                _sentences.Add(new((ActorSet source) =>
                {
                    var resolution = new EffectResolution(type, tags, targetType, power);
                    resolution.Execute = (ActorSet target) =>
                    {
                        Body main = target.Focus;
                        AddEffectEntity(source, main, type, tags, duration, resolution, effectAction);
                        resolution.RunStage(EffectStage.OnSuccessfullyAdded, source, main);
                    };
                    source.Focus.TurnContext.WriteResolution(resolution);
                }, SentenceType.Effect, StructureType.Main));
                return this;
            }
            public SourceFile OnSuccessfullyAdded(Action<ActorSet, Body, EffectResolution> action)
            {
                _rhetoricCache.Push(new((ActorSet source) =>
                {
                    var list = source.Focus.TurnContext.EffectResolutions;
                    if (list.Count == 0)
                    {
                        return;
                    }
                    var last = list[^1];
                    last.AddStage(EffectStage.OnSuccessfullyAdded, action);
                }, SentenceType.Effect, StructureType.Rhetoric, _sentences[^1]));
                return this;
            }
            public SourceFile UseResource(float need, ResourceType type, bool ifCommonOnly = false)
            {
                return WriteFree(source => source.Focus.Resource.Use(type, need, ifCommonOnly));
            }
            public SourceFile LinkJudgeRule(string ruleKey)
            {
                _mutationsOnCompile.Add(ruleKey);
                return this;
            }
        }
        /// <summary>
        /// 专用于外部产生的孤立效果生成
        /// </summary>
        public static void AddEffectEntity(Body main, EffectType type, List<EffectTag> tags, int duration, IResolution resolution, Action<Body, EffectEntity> effectAction)
        {
            EffectEntity effect = new EffectEntity(type, tags, duration, resolution);
            effect.Execute = (Body body) => effectAction(body, effect);
            main.Effect.Add(effect);
        }
        /// <summary>
        /// 专用于被EffectResolution引导的效果生成
        /// </summary>
        public static void AddEffectEntity(ActorSet source, Body main, EffectType type, List<EffectTag> tags, int duration, EffectResolution resolution, Action<ActorSet, Body, EffectEntity> effectAction)
        {
            EffectEntity effect = new EffectEntity(type, tags, duration, resolution);
            effect.Execute = (Body body) => effectAction(source, body, effect);
            main.Effect.Add(effect);
        }
        public static SourceFile Create(Pen Pen)
        {
            var sourceFile = new SourceFile();
            return Pen(sourceFile);
        }
    }
}
