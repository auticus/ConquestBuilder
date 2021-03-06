﻿using System;
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
        public IList<IConquestAnalysisOutput> BroadAnalysis<T>(IEnumerable<T> models, IEnumerable<ISpell> spells) where T: IConquestGamePiece
        {
            //regiments we base on 3, characters on 1
            var standCount = typeof(T) == typeof(IConquestCharacter) ? 1 : 3;
            var frontageCount = typeof(T) == typeof(IConquestCharacter) ? 1 : 3;
            
            //using keyvalue pair here because we need to be able to see what input model was attached to the baseline output that was generated from it
            var outputKvp = InitializeBroadAnalysis(models, spells, analysisStandCount: standCount, frontageCount: frontageCount, 
                applyFullyDeadly: false);

            ApplyExtraOptions(normalizedOutputKvp: outputKvp, analysisStandCount: standCount, frontageCount: frontageCount, applyFullyDeadly: false);
            NormalizeAndEfficiencyData(outputKvp);
            FinalizeEfficiency(outputKvp);
            FinalizeAnalysis(outputKvp);
            
            return outputKvp.Keys.Cast<IConquestAnalysisOutput>().ToList();
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
        /// <returns>A dictionary containing a key value that is the output matched by the model that it is derived from as the value</returns>
        private static IDictionary<IConquestAnalysisOutput, IConquestGamePiece> InitializeBroadAnalysis<T>(IEnumerable<T> models, 
            IEnumerable<ISpell> spells, 
            int analysisStandCount, 
            int frontageCount, 
            bool applyFullyDeadly)
            where T: IConquestGamePiece
        {
            var dictionary = new Dictionary<IConquestAnalysisOutput, IConquestGamePiece>();

            foreach (var primeModel in models)
            {
                //analyze the base model
                var input = new AnalysisInput()
                {
                    Model = primeModel,
                    AnalysisStandCount = analysisStandCount,
                    ApplyFullyDeadly = applyFullyDeadly,
                    FrontageCount = frontageCount
                };
                var baselineOutput = AnalyzeModel(input);
                baselineOutput.IsBaselineOutput = true;

                dictionary.Add(baselineOutput, primeModel);
            }

            return dictionary;
        }

        private static ConquestUnitOutput AnalyzeModel(AnalysisInput input)
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
            Option.ProcessOption(input, output, input.BaseOption, input.Model is IConquestCharacter);

            var allClash = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var allDefenses = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var allResolve = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var magicOutput = Magic.CalculateOutput(input.Model, input.Spell, allClash, allDefenses, allResolve, useSmartCasting: true);

            if (magicOutput > 0)
            {
                output.Unit = output.Unit + $" ({input.Spell.Name})";
                output.Points += input.Spell.Points;
            }

            //begin calculations - this is the meat of this method
            CalculateOffense(output, input.Model, input.ApplyFullyDeadly, magicOutput);

            if (input.Model.CanCalculateDefense()) //characterInputModel we don't calculate defense for
            {
                CalculateDefense(output, input.Model);
            }

            //apply misc modifiers to the scores
            var normalizeMove = Movement.CalculateOutput(input.Model);
            output.ApplyMovementScoresToAllStands(input.Model.Move, normalizeMove);
            return output;
        }

        private static void ApplyExtraOptions(IDictionary<IConquestAnalysisOutput, IConquestGamePiece> normalizedOutputKvp, 
            int analysisStandCount,
            int frontageCount, 
            bool applyFullyDeadly)
        {
            //T primeModel, AnalysisInput< T > input
            //input has the BaselineOutput applied to it so make sure you apply that here

            foreach (var output in normalizedOutputKvp)
            {
                //analyze the base model
                var input = new AnalysisInput()
                {
                    Model = output.Value,
                    AnalysisStandCount = analysisStandCount,
                    ApplyFullyDeadly = applyFullyDeadly,
                    FrontageCount = frontageCount
                };
                input.BaselineOutput = output.Key;

                //analyze options and add those to the baseline object as comparisons
                if (output.Value.Options.Any())
                {
                    AnalyzeModelExtras<IBaseOption>(input);
                }

                //analyze spells
                if (output.Value.CanCastSpells())
                {
                    AnalyzeModelExtras<ISpell>(input);
                }
            }
        }

        /// <summary>
        /// Will take the mainOutput object passed and pack output modifications to it that can be analyzed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TExtra">The extra we are processing (option, spell, etc)</typeparam>
        private static void AnalyzeModelExtras<TExtra>(AnalysisInput input)
        {
            if (!ExtraProcessingRequired<TExtra>(input)) return;

            //options (assumption can only take one)
            var optionQueue = new Queue<object>(); //IConquestGameElement or SpellModel
            ExtraBuildQueue<TExtra>(input, optionQueue);

            while (optionQueue.Count > 0)
            {
                var option = optionQueue.Dequeue();
                var aInput = BuildInput(input, option);
                var output = AnalyzeModel(aInput);
                input.BaselineOutput.UpgradeOutputModifications.Add(output);
            }
        }

        /// <summary>
        /// Based on the extra type will determine if the appropriate property or collection is populated or not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TExtra"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool ExtraProcessingRequired<TExtra>(AnalysisInput input)
        {
            if (typeof(TExtra) == typeof(BaseOption))
            {
                if (input.Model.Options.Any()) return true;
            }

            if (typeof(TExtra) == typeof(ISpell))
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
        private static void ExtraBuildQueue<TExtra>(AnalysisInput input, Queue<object> q)
        {
            if (typeof(TExtra) == typeof(BaseOption))
            {
                //compiler warning:  UnitOptionModel is a type of IConquestGameElement and if typeof TExtra is this it will always be ok
                foreach (var opt in input.Model.Options)
                {
                    q.Enqueue(opt);
                }

                return;
            }

            if (typeof(TExtra) == typeof(ISpell))
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
        private static AnalysisInput BuildInput(AnalysisInput input, object extra)
        {
            if (extra.GetType() == typeof(BaseOption))
                return BuildOptionInput(input, extra);
            if (extra.GetType() == typeof(SpellModel))
                return BuildSpellInput(input, extra);

            throw new InvalidOperationException("BuildInput was passed an object that is not recognized");
        }

        private static string GetOptionName(object option)
        {
            if (option.GetType() == typeof(BaseOption))
            {
                var opt = option as BaseOption;
                return opt.Name;
            }

            if (option.GetType() == typeof(ISpell))
            {
                var opt = option as ISpell;
                return opt.Name;
            }

            throw new InvalidOperationException("GetOptionName was passed an object that is not recognized");
        }

        private static AnalysisInput BuildOptionInput(AnalysisInput input, object extra)
        {
            var optionModel = (IConquestGamePiece)input.Model.Clone();
            if (!(extra is BaseOption option)) throw new InvalidOperationException("Object sent to BuildOptionInput is not an option model");

            var aInput = new AnalysisInput()
            {
                Model = optionModel,
                AnalysisStandCount = input.AnalysisStandCount,
                ApplyFullyDeadly = input.ApplyFullyDeadly,
                FrontageCount = input.FrontageCount,
                BaseOption = option
            };

            return aInput;
        }

        private static AnalysisInput BuildSpellInput(AnalysisInput input, object extra)
        {
            var optionModel = (IConquestGamePiece)input.Model.Clone();
            if (!(extra is ISpell option))
                throw new InvalidOperationException("Object sent to BuildSpellInput is not a spell model");

            var aInput = new AnalysisInput()
            {
                Model = optionModel,
                AnalysisStandCount = input.AnalysisStandCount,
                ApplyFullyDeadly = input.ApplyFullyDeadly,
                FrontageCount = input.FrontageCount,
                Spell = option
            };

            return aInput;
        }

        private static void CalculateOffense(ConquestUnitOutput output, IConquestGamePiece model, 
            bool applyFullyDeadly, double magicOutput)
        {
            var allDefenses = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var allResolve = new List<int>() { 1, 2, 3, 4, 5, 6 };

            output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.RangedOutput =
                        RangedOffense.CalculateOutput(model, allDefenses, supportOnly: false, applyFullyDeadly);

            var clashOutputs = ClashOffense.CalculateOutput(model, allDefenses, allResolve, supportOnly: false,
                applyFullyDeadly);
            output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.ClashOutput = clashOutputs[0];
            output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.ImpactOutput = clashOutputs[1];
            output.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Output = magicOutput;

            //*************************calculate its partial stand data*********************************************************//
            //full output by stand (indices 1-3 which represent the full extra output constants
            if (model.AdditionalPoints > 0) //if its 0, you can't add models to it so these partial stats don't exist
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
                    output.Stands[i].Magic.Output = 
                        output.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Output / 
                        ConquestUnitOutput.BASE_STAND_COUNT;
                }
            }

            if (model.DoubleAttack)
            {
                //if double attack, the best option would be to double up on whatever is best
                if (output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.ClashOutput >
                    output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.RangedOutput)
                {
                    output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.ClashOutput *= 2;
                }
                else
                {
                    output.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.RangedOutput *= 2;
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

                if (model.DoubleAttack)
                {
                    if (clashOutputs[0] > rangedOutput)
                    {
                        clashOutputs[0] *= 2;
                    }
                    else
                    {
                        rangedOutput *= 2;
                    }
                }

                for (var i = 4; i < 7; i++)
                {
                    output.Stands[i].Offense.ClashOutput = clashOutputs[0];
                    output.Stands[i].Offense.ImpactOutput = clashOutputs[1];
                    output.Stands[i].Offense.RangedOutput = rangedOutput;
                }
            }
        }

        private static void CalculateDefense(IConquestAnalysisOutput output, IConquestGamePiece model)
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

        private static void NormalizeAndEfficiencyData(IDictionary<IConquestAnalysisOutput, IConquestGamePiece> data)
        {
            var aggregateList = GetAllOutputsFromRawOutputs(data);

            //the offense is typically going to be much smaller than the defense score, so we need to pump it up to match the scale
            var averageOffense = aggregateList.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.TotalOutput);
            var averageDefense = aggregateList.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.TotalOutput);

            var averageSpellOutput = 1.0d;
            if (aggregateList.Any(p=>p.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Output > 0))
                averageSpellOutput = aggregateList.Where(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Output > 0)
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
                    dataPoint.Key.Stands[i].Offense.NormalizedVector = normalizationVector;
                    dataPoint.Key.Stands[i].Magic.NormalizationVector = spellNormalizationVector;

                    if (i == ConquestUnitOutput.FULL_OUTPUT)
                    {
                        CalculateEfficiency(i, dataPoint.Key);
                    }

                    foreach (var option in dataPoint.Key.UpgradeOutputModifications)
                    {
                        option.Stands[i].Offense.NormalizedVector = normalizationVector;
                        option.Stands[i].Magic.NormalizationVector = spellNormalizationVector;

                        if (i == ConquestUnitOutput.FULL_OUTPUT)
                        {
                            CalculateEfficiency(i, option);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Pulls all values and their child outputs into one list for aggregate purposes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private static IList<IConquestAnalysisOutput> GetAllOutputsFromRawOutputs(IDictionary<IConquestAnalysisOutput,IConquestGamePiece> data)
        {
            var returnList = new List<IConquestAnalysisOutput>();
            foreach (var key in data.Keys)
            {
                returnList.Add(key);
                returnList.AddRange(key.UpgradeOutputModifications);
            }

            return returnList;
        }

        /// <summary>
        /// Calculates the efficiency of a stand
        /// </summary>
        /// <param name="standIndex"></param>
        /// <param name="dataPoint"></param>
        private static void CalculateEfficiency(int standIndex, IConquestAnalysisOutput dataPoint)
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
        private static void FinalizeEfficiency(IDictionary<IConquestAnalysisOutput, IConquestGamePiece> data)
        {
            var aggregateList = GetAllOutputsFromRawOutputs(data);

            //intentionally only getting the average output of the full output stand (the primary) and not flipping the sub-stands
            var averageOutput = aggregateList.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].OutputScore);

            //adding one to it so that when we flip, the lowest will have a value and not be 0
            var maxOffenseEfficiency = aggregateList.Max(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency) +1;
            var maxDefenseEfficiency = aggregateList.Max(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency) +1;
            var maxMagicEfficiency = aggregateList.Max(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency) + 1;
            var maxOverallEfficiency = aggregateList.Max(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency) + 1;

            //flip scores
            foreach (var dataPoint in data)
            {
                dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency =
                    maxOffenseEfficiency - dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency;

                dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency =
                    maxDefenseEfficiency - dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency;

                dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency =
                    maxMagicEfficiency - dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency;

                dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency =
                    maxOverallEfficiency - dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency;
            }

            //now normalize the efficiency scores so that the output score and efficiency scores are the same scale.  
            //currently output score should be bigger

            var avgOffenseEfficiency = aggregateList.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency);
            var avgDefenseEfficiency = aggregateList.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency);
            var avgMagicEfficiency = aggregateList.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency);
            var avgEfficiency = aggregateList.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency);

            var normalizationOffenseVector = averageOutput / avgOffenseEfficiency;
            var normalizationDefenseVector = averageOutput / avgDefenseEfficiency;
            var normalizationMagicVector = averageOutput / avgMagicEfficiency;
            var normalizationVector = averageOutput / avgEfficiency;

            foreach (var dataPoint in data)
            {
                dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency *= normalizationOffenseVector;
                dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency *= normalizationDefenseVector;
                dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Efficiency *= normalizationMagicVector;
                dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency *= normalizationVector;
            }
        }

        private static double[] GetAverageScores(IList<IConquestAnalysisOutput> data)
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

        private static void FinalizeAnalysis(IDictionary<IConquestAnalysisOutput, IConquestGamePiece> data)
        {
            //todo: flaw here - using average defense efficiency but character defense score is not calculated
            //todo: magic score not calculated as part of this analysis

            var aggregateList = GetAllOutputsFromRawOutputs(data);

            var avgScores = GetAverageScores(aggregateList);
            var avgOutput = avgScores[0];
            var avgOffenseEfficiency = avgScores[1];
            var avgDefenseEfficiency = avgScores[2];
            var avgEfficiency = avgScores[3];

            foreach (var dataPoint in data)
            {
                dataPoint.Key.Analysis.DeviationScore =
                    dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].OutputScore - avgOutput;

                dataPoint.Key.Analysis.DeviationScorePercent =
                    (dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].OutputScore - avgOutput) / avgOutput * 100;

                dataPoint.Key.Analysis.DeviationOffenseEfficiency =
                    dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency - avgOffenseEfficiency;

                dataPoint.Key.Analysis.DeviationOffenseEfficiencyPercent =
                    (dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency - avgOffenseEfficiency) /
                    avgOffenseEfficiency * 100;

                dataPoint.Key.Analysis.DeviationDefenseEfficiency =
                    dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency - avgDefenseEfficiency;

                dataPoint.Key.Analysis.DeviationDefenseEfficiencyPercent =
                    (dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency - avgDefenseEfficiency) /
                    avgDefenseEfficiency * 100;

                dataPoint.Key.Analysis.DeviationEfficiency =
                    dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency - avgEfficiency;

                dataPoint.Key.Analysis.DeviationEfficiencyPercent =
                    (dataPoint.Key.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency - avgEfficiency) / avgEfficiency * 100;

                dataPoint.Key.Summary.MeanScore = avgOutput;
                dataPoint.Key.Summary.MeanOffenseEfficiency = avgOffenseEfficiency;
                dataPoint.Key.Summary.MeanDefenseEfficiency = avgDefenseEfficiency;
                dataPoint.Key.Summary.MeanEfficiency = avgEfficiency;

                dataPoint.Key.CreateOutputData();

                foreach (var option in dataPoint.Key.UpgradeOutputModifications)
                {
                    option.CreateOutputData();
                }
            }
        }

        private static void ProcessModelDefaults(IConquestGamePiece model) 
        {
            if (model is IConquestCharacter)
            {
                model.Models = 1;
            }
        }
    }
}
