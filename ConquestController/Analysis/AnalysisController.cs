using System;
using System.Collections.Generic;
using System.Linq;
using ConquestController.Analysis.Components;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

namespace ConquestController.Analysis
{
    public class AnalysisController
    {
        private const double TOLERANCE = 0.0001;

        /// <summary>
        /// Given a list of unit models and options, calculate the output scores and return them back against all other models in the game (broad analysis)
        /// </summary>
        /// <param name="models"></param>
        /// <param name="spells"></param>
        /// <returns></returns>
        public IList<ConquestUnitOutput> BroadAnalysis<T>(IEnumerable<T> models, IEnumerable<SpellModel> spells) where T: ConquestInput<T>
        {
            //regiments we base on 3, characters on 1
            var standCount = typeof(T) == typeof(CharacterInputModel) ? 1 : 3;
            var frontageCount = typeof(T) == typeof(CharacterInputModel) ? 1 : 3;
            
            var rawOutput = InitializeBroadAnalysis(models, spells, analysisStandCount: standCount, frontageCount: frontageCount, 
                applyFullyDeadly: false);

            NormalizeAndEfficiencyData(ref rawOutput);
            FinalizeEfficiency(ref rawOutput);
            FinalizeAnalysis(ref rawOutput);
            
            return rawOutput;
        }

        /// <summary>
        /// Given two army lists, calculate output of those selected models vs the other selected models
        /// </summary>
        public void SpecificAnalysis()
        {
            //todo: implement specific army lists with chosen options
            //character options - need to be able to include a unit that they are with like officer's have bastion... there is output you gain here if you know his unit
            throw new NotImplementedException("TODO: add functionality to examine two army lists and see how they match up against each other");
        }

        /// <summary>
        /// An analysis of a model vs all other models in the game
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models"></param>
        /// <param name="spells"></param>
        /// <param name="analysisStandCount"></param>
        /// <param name="frontageCount"></param>
        /// <param name="applyFullyDeadly"></param>
        /// <returns></returns>
        private static IList<ConquestUnitOutput> InitializeBroadAnalysis<T>(IEnumerable<T> models, IEnumerable<SpellModel> spells, int analysisStandCount, 
            int frontageCount, bool applyFullyDeadly)
            where T: ConquestInput<T>
        {
            var returnList = new List<ConquestUnitOutput>();
            foreach (var primeModel in models)
            {
                //analyze the base model
                var input = new AnalysisInput<T>()
                {
                    Model = primeModel,
                    AnalysisStandCount = analysisStandCount,
                    ApplyFullyDeadly = applyFullyDeadly,
                    FrontageCount = frontageCount
                };
                var baselineOutput = AnalyzeModel(input);
                baselineOutput.IsBaselineOutput = true;

                returnList.Add(baselineOutput);
                input.BaselineOutput = baselineOutput;

                //analyze options and add those to the baseline object as comparisons
                if (primeModel.Options.Any())
                {
                    AnalyzeModelExtras<T, UnitOptionModel>(input);
                }

                //analyze spells
                if (primeModel.CanCastSpells())
                {
                    AnalyzeModelExtras<T, SpellModel>(input);
                }
            }

            return returnList;
        }

        private static ConquestUnitOutput AnalyzeModel<T>(AnalysisInput<T> input)
            where T : ConquestInput<T>
        {
            ProcessModelDefaults(input.Model);
            var output = new ConquestUnitOutput
            {
                Faction = input.Model.Faction,
                Unit = input.Model.Unit,
                StandCount = input.AnalysisStandCount,
                FrontageCount = input.FrontageCount,
                PointsAdditional = input.Model.AdditionalPoints,
                Weight = input.Model.Weight,
                Points = input.Model.Points,
                IsReleased = input.Model.IsReleased
            };

            //the optionals like options, spells, etc methods will all bounce out immediately if the model passed in has none so freely just call the chain
            //process options
            Option.ProcessOption(input, output);
            ProcessSpell(input, output);

            //begin calculations - this is the meat of this method
            CalculateOffense(output, input.Model, input.ApplyFullyDeadly);

            if (input.Model.CanCalculateDefense()) //characterInputModel we don't calculate defense for
            {
                CalculateDefense(output, input.Model);
            }

            //apply misc modifiers to the scores
            var normalizeMove = Movement.CalculateOutput(input.Model);
            output.ApplyMovementScoresToAllStands(input.Model.Move, normalizeMove);
            output.CreateOutputData();
            return output;
        }

        /// <summary>
        /// Will take the mainOutput object passed and pack output modifications to it that can be analyzed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TExtra">The extra we are processing (option, spell, etc)</typeparam>
        private static void AnalyzeModelExtras<T, TExtra>(AnalysisInput<T> input) where T : ConquestInput<T>
        {
            if (!ExtraProcessingRequired<T, TExtra>(input)) return;

            //options (assumption can only take one)
            var optionQueue = new Queue<object>(); //IConquestInput or SpellModel
            ExtraBuildQueue<T, TExtra>(input, optionQueue);

            while (optionQueue.Count > 0)
            {
                var option = optionQueue.Dequeue();
                var aInput = BuildInput(input, option);
                var output = AnalyzeModel<T>(aInput);

                //compare output with mainOutput.  The output that comes out of this will simply be a value that adds or subtracts from the output value
                var comparison = CreateComparisonOutput(input.BaselineOutput, output);
                input.BaselineOutput.UpgradeOutputModifications.Add(comparison);
            }
        }

        /// <summary>
        /// Based on the extra type will determine if the appropriate property or collection is populated or not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TExtra"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool ExtraProcessingRequired<T, TExtra>(AnalysisInput<T> input) where T : ConquestInput<T>
        {
            if (typeof(TExtra) == typeof(UnitOptionModel))
            {
                if (input.Model.Options.Any()) return true;
            }

            if (typeof(TExtra) == typeof(SpellModel))
            {
                if (!(input.Model is IConquestSpellcaster spellcaster)) return false;
                if (spellcaster.Spells.Any()) return true;
            }

            return false;
        }

        /// <summary>
        /// Based on the extra passed in will populate the generic object queue passed in with the appropriate model to cycle through
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TExtra"></typeparam>
        /// <param name="input"></param>
        /// <param name="q"></param>
        private static void ExtraBuildQueue<T, TExtra>(AnalysisInput<T> input, Queue<object> q) where T : ConquestInput<T>
        {
            if (typeof(TExtra) == typeof(UnitOptionModel))
            {
                //compiler warning:  UnitOptionModel is a type of IConquestInput and if typeof TExtra is this it will always be ok
                foreach (var opt in input.Model.Options)
                {
                    q.Enqueue(opt);
                }

                return;
            }

            if (typeof(TExtra) == typeof(SpellModel))
            {
                if (!(input.Model is IConquestSpellcaster spellcaster)) return;

                foreach (var spell in spellcaster.Spells)
                {
                    q.Enqueue(spell);
                }
            }
        }

        /// <summary>
        /// Will examine the extra parameter and based off of its type will return the proper AnalysisInput object or throw an InvalidOperationException
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="extra"></param>
        /// <returns></returns>
        private static AnalysisInput<T> BuildInput<T>(AnalysisInput<T> input, object extra) where T: ConquestInput<T>
        {
            if (extra.GetType() == typeof(UnitOptionModel))
                return BuildOptionInput(input, extra);
            if (extra.GetType() == typeof(SpellModel))
                return BuildSpellInput(input, extra);

            throw new InvalidOperationException("BuildInput was passed an object that is not recognized");
        }

        private static AnalysisInput<T> BuildOptionInput<T>(AnalysisInput<T> input, object extra) where T : ConquestInput<T>
        {
            var optionModel = input.Model.Copy();
            if (!(extra is UnitOptionModel option)) throw new InvalidOperationException("Object sent to BuildOptionInput is not an option model");

            var aInput = new AnalysisInput<T>()
            {
                Model = optionModel,
                AnalysisStandCount = input.AnalysisStandCount,
                ApplyFullyDeadly = input.ApplyFullyDeadly,
                FrontageCount = input.FrontageCount,
                Option = option
            };

            return aInput;
        }

        private static AnalysisInput<T> BuildSpellInput<T>(AnalysisInput<T> input, object extra)
            where T : ConquestInput<T>
        {
            var optionModel = input.Model.Copy();
            if (!(extra is SpellModel option))
                throw new InvalidOperationException("Object sent to BuildSpellInput is not a spell model");

            var aInput = new AnalysisInput<T>()
            {
                Model = optionModel,
                AnalysisStandCount = input.AnalysisStandCount,
                ApplyFullyDeadly = input.ApplyFullyDeadly,
                FrontageCount = input.FrontageCount,
                Spell = option
            };

            return aInput;
        }

        private static ConquestUnitOutput CreateComparisonOutput(ConquestUnitOutput baseline, ConquestUnitOutput compare)
        {
            var output = baseline.Copy();
            output.IsBaselineOutput = false;
            
            var o = output.Stands[ConquestUnitOutput.FULL_OUTPUT];
            var b = baseline.Stands[ConquestUnitOutput.FULL_OUTPUT];
            var c = compare.Stands[ConquestUnitOutput.FULL_OUTPUT];

            var overrideData = new AnalysisFileOutput
            {
                ClashOffense = c.Offense.NormalizedClashOutput - b.Offense.NormalizedClashOutput,
                RangedOffense = c.Offense.NormalizedRangedOutput - b.Offense.NormalizedRangedOutput,
                NormalizedOffense = c.Offense.NormalizedTotalOutput - b.Offense.NormalizedTotalOutput,
                TotalDefense = c.Defense.TotalOutput - b.Defense.TotalOutput,
                OutputScore = c.OutputScore - b.OutputScore,
                OffenseEfficiency = c.Offense.Efficiency - b.Offense.Efficiency,
                DefenseEfficiency = c.Defense.Efficiency - b.Defense.Efficiency,
                Efficiency = c.Efficiency - b.Efficiency
            };

            output.OverrideOutputScores(overrideData);

            return output;
        }

        private static void CalculateOffense<T>(ConquestUnitOutput output, T model, 
            bool applyFullyDeadly) where T: ConquestInput<T>
        {
            var allDefenses = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var allResolve = new List<int>() { 1, 2, 3, 4, 5, 6 };

            output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.RangedOutput =
                        RangedOffense.CalculateOutput(model, allDefenses, supportOnly: false, applyFullyDeadly);

            var clashOutputs = ClashOffense.CalculateOutput(model, allDefenses, allResolve, supportOnly: false,
                applyFullyDeadly);
            output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.ClashOutput = clashOutputs[0];
            output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.ImpactOutput = clashOutputs[1];

            //*************************calculate its partial stand data*********************************************************//
            //full output by stand (indices 1-3 which represent the full extra output constants
            if (model.AdditionalPoints > 0
            ) //if its 0, you can't add models to it so these partial stats don't exist
            {
                //the output is for the base stand count output (so 3 stands).  so in that case, just divide by that to see what each stand
                //provides by itself
                for (var i = 1; i < 4; i++)
                {
                    output.Stands[i].Offense.RangedOutput =
                        output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.RangedOutput /
                        ConquestUnitOutput.BASE_STAND_COUNT;
                    output.Stands[i].Offense.ClashOutput =
                        output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.ClashOutput /
                        ConquestUnitOutput.BASE_STAND_COUNT;
                    output.Stands[i].Offense.ImpactOutput =
                        output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.ImpactOutput /
                        ConquestUnitOutput.BASE_STAND_COUNT;
                }
            }

            //support stand output by stand (typically get one attack per stand, though support rule will give two attacks per stand)
            if (model.AdditionalPoints > 0)
            {
                //calculate the output of just one attack out of the stand
                clashOutputs = ClashOffense.CalculateOutput(model, allDefenses, allResolve, supportOnly: true,
                    applyFullyDeadly);
                var rangedOutput =
                    RangedOffense.CalculateOutput(model, allDefenses, supportOnly: true, applyFullyDeadly);

                for (var i = 4; i < 7; i++)
                {
                    output.Stands[i].Offense.ClashOutput = clashOutputs[0];
                    output.Stands[i].Offense.ImpactOutput = clashOutputs[1];
                    output.Stands[i].Offense.RangedOutput = rangedOutput;
                }
            }
        }

        private static void CalculateDefense<T>(ConquestUnitOutput output, T model) where T: ConquestInput<T>
        {
            var allCleave = new List<int>() { 0, 1, 2, 3, 4 };

            //calculate defense scores - this should only be for regiments (and thus hard-coding the stand-counts to 3, etc is what we expect)
            var defenseOutputs = Defense.CalculateOutput(model, allCleave, standCount: 3);
            output.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.RawOutput = defenseOutputs[0];
            output.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.ResolveOutput = defenseOutputs[1];

            defenseOutputs = Defense.CalculateOutput(model, allCleave, standCount: 6);
            output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_4_6].Defense.RawOutput = defenseOutputs[0];
            output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_4_6].Defense.ResolveOutput =
                defenseOutputs[1];
            output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_4_6].Defense.RawOutput =
                defenseOutputs[0];
            output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_4_6].Defense.ResolveOutput =
                defenseOutputs[1];

            defenseOutputs = Defense.CalculateOutput(model, allCleave, standCount: 9);
            output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_7_9].Defense.RawOutput = defenseOutputs[0];
            output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_7_9].Defense.ResolveOutput =
                defenseOutputs[1];
            output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_7_9].Defense.RawOutput =
                defenseOutputs[0];
            output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_7_9].Defense.ResolveOutput =
                defenseOutputs[1];

            defenseOutputs = Defense.CalculateOutput(model, allCleave, standCount: 12);
            output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_10].Defense.RawOutput = defenseOutputs[0];
            output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_10].Defense.ResolveOutput =
                defenseOutputs[1];
            output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_10].Defense.RawOutput = defenseOutputs[0];
            output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_10].Defense.ResolveOutput =
                defenseOutputs[1];
        }

        private static void NormalizeAndEfficiencyData(ref IList<ConquestUnitOutput> data)
        {
            //the offense is typically going to be much smaller than the defense score, so we need to pump it up to match the scale
            var averageOffense = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.TotalOutput);
            var averageDefense = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.TotalOutput);

            var averageSpellOutput = 1.0d;
            if (data.Any(p=>p.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Output > 0))
                averageSpellOutput = data.Where(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Output > 0)
                                            .Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Output);

            var normalizationVector = averageDefense / averageOffense;
            var spellNormalizationVector = averageDefense / averageSpellOutput; //get the factor to match up with offense

            if (Math.Abs(normalizationVector) < TOLERANCE) normalizationVector = 1;
            if (Math.Abs(spellNormalizationVector) < TOLERANCE) spellNormalizationVector = 1;

            //normalize offense
            foreach (var dataPoint in data)
            {
                for (var i = 0; i < 7; i++)
                {
                    dataPoint.Stands[i].Offense.NormalizedVector = normalizationVector;
                    dataPoint.Stands[i].Magic.NormalizationVector = spellNormalizationVector;

                    if (i == ConquestUnitOutput.FULL_OUTPUT)
                        CalculateEfficiency(i, dataPoint);
                }
            }
        }

        /// <summary>
        /// Calculates the efficiency of a stand
        /// </summary>
        /// <param name="standIndex"></param>
        /// <param name="dataPoint"></param>
        private static void CalculateEfficiency(int standIndex, ConquestUnitOutput dataPoint)
        {
            //this is designed to only work with the full output stand.  If you want to break this into the smaller stand calculations
            //you must use their AdditionalPoints value, not the Points.
            dataPoint.Stands[standIndex].Offense.Efficiency =
                dataPoint.Points / dataPoint.Stands[standIndex].Offense.NormalizedTotalOutput;

            dataPoint.Stands[standIndex].Defense.Efficiency =
                dataPoint.Points / dataPoint.Stands[standIndex].Defense.TotalOutput;

            dataPoint.Stands[standIndex].Magic.Efficiency =
                dataPoint.Points / dataPoint.Stands[standIndex].Magic.NormalizationOutput;

            dataPoint.Stands[standIndex].Efficiency = dataPoint.Points / dataPoint.Stands[standIndex].OutputScore;
        }

        /// <summary>
        /// Efficiency is where the lower the score, the better that is.  That is not intuitive and this method will flip those scores
        /// </summary>
        private static void FinalizeEfficiency(ref IList<ConquestUnitOutput> data)
        {
            //intentionally only getting the average output of the full output stand (the primary) and not flipping the sub-stands
            var averageOutput = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].OutputScore);

            //adding one to it so that when we flip, the lowest will have a value and not be 0
            var maxOffenseEfficiency = data.Max(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency) +1;
            var maxDefenseEfficiency = data.Max(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency) +1;
            var maxMagicEfficiency = data.Max(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency) + 1;
            var maxOverallEfficiency = data.Max(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency) + 1;

            //flip scores
            foreach (var dataPoint in data)
            {
                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency =
                    maxOffenseEfficiency - dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency;

                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency =
                    maxDefenseEfficiency - dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency;

                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency =
                    maxMagicEfficiency - dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency;

                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency =
                    maxOverallEfficiency - dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency;
            }

            //now normalize the efficiency scores so that the output score and efficiency scores are the same scale.  
            //currently output score should be bigger

            var avgOffenseEfficiency = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency);
            var avgDefenseEfficiency = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency);
            var avgMagicEfficiency = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency);
            var avgEfficiency = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency);

            var normalizationOffenseVector = averageOutput / avgOffenseEfficiency;
            var normalizationDefenseVector = averageOutput / avgDefenseEfficiency;
            var normalizationMagicVector = averageOutput / avgMagicEfficiency;
            var normalizationVector = averageOutput / avgEfficiency;

            foreach (var dataPoint in data)
            {
                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency *= normalizationOffenseVector;
                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency *= normalizationDefenseVector;
                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency *= normalizationMagicVector;
                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency *= normalizationVector;
            }
        }

        private static double[] GetAverageScores(IList<ConquestUnitOutput> data)
        {
            var totalOutput = data.Sum(dataPoint => dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].OutputScore);
            var totalOffenseEfficiency = data.Sum(dataPoint => dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency);
            var totalDefenseEfficiency = data.Sum(dataPoint => dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency);
            var totalEfficiency = data.Sum(dataPoint => dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency);

            return new[]
            {
                totalOutput / data.Count, totalOffenseEfficiency / data.Count, totalDefenseEfficiency / data.Count,
                totalEfficiency / data.Count
            };
        }

        private static void FinalizeAnalysis(ref IList<ConquestUnitOutput> data)
        {
            //todo: flaw here - using average defense efficiency but character defense score is not calculated
            //todo: magic score not caculated as part of this analysis
            var avgScores = GetAverageScores(data);
            var avgOutput = avgScores[0];
            var avgOffenseEfficiency = avgScores[1];
            var avgDefenseEfficiency = avgScores[2];
            var avgEfficiency = avgScores[3];

            foreach (var dataPoint in data)
            {
                dataPoint.Analysis.DeviationScore =
                    dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].OutputScore - avgOutput;

                dataPoint.Analysis.DeviationScorePercent =
                    (dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].OutputScore - avgOutput) / avgOutput * 100;

                dataPoint.Analysis.DeviationOffenseEfficiency =
                    dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency - avgOffenseEfficiency;

                dataPoint.Analysis.DeviationOffenseEfficiencyPercent =
                    (dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency - avgOffenseEfficiency) /
                    avgOffenseEfficiency * 100;

                dataPoint.Analysis.DeviationDefenseEfficiency =
                    dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency - avgDefenseEfficiency;

                dataPoint.Analysis.DeviationDefenseEfficiencyPercent =
                    (dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency - avgDefenseEfficiency) /
                    avgDefenseEfficiency * 100;

                dataPoint.Analysis.DeviationEfficiency =
                    dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency - avgEfficiency;

                dataPoint.Analysis.DeviationEfficiencyPercent =
                    (dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency - avgEfficiency) / avgEfficiency * 100;

                dataPoint.Summary.MeanScore = avgOutput;
                dataPoint.Summary.MeanOffenseEfficiency = avgOffenseEfficiency;
                dataPoint.Summary.MeanDefenseEfficiency = avgDefenseEfficiency;
                dataPoint.Summary.MeanEfficiency = avgEfficiency;
            }
        }

        private static void ProcessModelDefaults<T>(T model) where T: ConquestInput<T>
        {
            if (typeof(T) == typeof(CharacterInputModel))
            {
                model.Models = 1;
            }
        }
    }
}
