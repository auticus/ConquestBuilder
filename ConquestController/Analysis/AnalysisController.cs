using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using ConquestController.Analysis.Components;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

namespace ConquestController.Analysis
{
    public class AnalysisController
    {
        private enum ApplyOptionResult
        {
            NotOption = 0,
            ImpactfulOption,
            NonImpactfulOption
        }

        /// <summary>
        /// Given a list of unit models and options, calculate the output scores and return them back
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public IList<ConquestUnitOutput> AnalyzeTotalGame(List<UnitInputModel> models)
        {
            var rawOutput = InitializeModels(models, analysisStandCount: 3, frontageCount: 3, applyFullyDeadly: false);
            NormalizeData(ref rawOutput);
            FinalizeEfficiency(ref rawOutput);
            FinalizeAnalysis(ref rawOutput);
            
            return rawOutput;
        }

        public void AnalyzeArmyLists()
        {
            //todo: implement
            throw new NotImplementedException("TODO: add functionality to examine two army lists and see how they match up against each other");
        }

        private IList<ConquestUnitOutput> InitializeModels(List<UnitInputModel> models, int analysisStandCount, int frontageCount, bool applyFullyDeadly)
        {
            var returnList = new List<ConquestUnitOutput>();
            var allDefenses = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var allResolve = new List<int>() { 1, 2, 3, 4, 5, 6 };
            var allCleave = new List<int>() { 0, 1, 2, 3, 4 };
            foreach (var primeModel in models)
            {
                var primaryModelInitialized = false;

                var optionQueue = new Queue<IConquestInput>();
                foreach(var opt in primeModel.Options)
                    optionQueue.Enqueue(opt);

                while (!primaryModelInitialized || optionQueue.Count > 0)
                {
                    var model = primeModel;
                    ApplyOptionResult optionResult = ApplyOptionResult.NotOption;

                    if (!primaryModelInitialized)
                    {
                        primaryModelInitialized = true;
                    }
                    else
                    {
                        var option = optionQueue.Dequeue() as UnitOptionModel;
                        model = model.Copy();
                        optionResult = ApplyOptionToUnit(model, option);
                    }

                    var output = new ConquestUnitOutput
                    {
                        Faction = model.Faction,
                        Unit = model.Unit,
                        StandCount = analysisStandCount,
                        FrontageCount = frontageCount,
                        PointsAdditional = model.AdditionalPoints,
                        Weight = model.Weight,
                        Points = model.Points,
                        IsReleased =  model.IsReleased
                    };

                    switch (optionResult)
                    {
                        case ApplyOptionResult.ImpactfulOption:
                            output.HasOptionAdded = true;
                            break;
                        case ApplyOptionResult.NonImpactfulOption:
                            output.HasNoImpactOptionAdded = true;
                            break;
                    }

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

                    //calculate defense scores
                    var defenseOutputs = Defense.CalculateOutput(model, allCleave, standCount: 3);
                    output.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.RawOutput = defenseOutputs[0];
                    output.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.ResolveOutput = defenseOutputs[1];

                    defenseOutputs = Defense.CalculateOutput(model, allCleave, standCount: 6);
                    output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_4_6].Defense.RawOutput = defenseOutputs[0];
                    output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_4_6].Defense.ResolveOutput = defenseOutputs[1];
                    output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_4_6].Defense.RawOutput = defenseOutputs[0];
                    output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_4_6].Defense.ResolveOutput =
                        defenseOutputs[1];

                    defenseOutputs = Defense.CalculateOutput(model, allCleave, standCount: 9);
                    output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_7_9].Defense.RawOutput = defenseOutputs[0];
                    output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_7_9].Defense.ResolveOutput = defenseOutputs[1];
                    output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_7_9].Defense.RawOutput = defenseOutputs[0];
                    output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_7_9].Defense.ResolveOutput =
                        defenseOutputs[1];

                    defenseOutputs = Defense.CalculateOutput(model, allCleave, standCount: 12);
                    output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_10].Defense.RawOutput = defenseOutputs[0];
                    output.Stands[ConquestUnitOutput.FULL_EXTRA_OUTPUT_10].Defense.ResolveOutput = defenseOutputs[1];
                    output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_10].Defense.RawOutput = defenseOutputs[0];
                    output.Stands[ConquestUnitOutput.SUPPORT_EXTRA_OUTPUT_10].Defense.ResolveOutput = defenseOutputs[1];

                    //apply misc modifiers to the scores
                    var normalizeMove = Movement.CalculateOutput(model);
                    output.ApplyMovementScoresToAllStands(model.Move, normalizeMove);

                    returnList.Add(output);
                }
            }

            return returnList;
        }

        private static void NormalizeData(ref IList<ConquestUnitOutput> data)
        {
            //the offense is typically going to be much smaller than the defense score, so we need to pump it up to match the scale
            var averageOffense = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.TotalOutput);
            var averageDefense = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.TotalOutput);
            var normalizationVector = averageDefense / averageOffense;

            //normalize offense
            foreach (var dataPoint in data)
            {
                for (var i = 0; i < 7; i++)
                {
                    dataPoint.Stands[i].Offense.NormalizedVector = normalizationVector;

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
            var maxOverallEfficiency = data.Max(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency) + 1;

            //flip scores
            foreach (var dataPoint in data)
            {
                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency =
                    maxOffenseEfficiency - dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency;

                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency =
                    maxDefenseEfficiency - dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency;

                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency =
                    maxOverallEfficiency - dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency;
            }

            //now normalize the efficiency scores so that the output score and efficiency scores are the same scale.  
            //currently output score should be bigger

            var avgOffenseEfficiency = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency);
            var avgDefenseEfficiency = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency);
            var avgEfficiency = data.Average(p => p.Stands[ConquestUnitOutput.FULL_OUTPUT].Efficiency);

            var normalizationOffenseVector = averageOutput / avgOffenseEfficiency;
            var normalizationDefenseVector = averageOutput / avgDefenseEfficiency;
            var normalizationVector = averageOutput / avgEfficiency;

            foreach (var dataPoint in data)
            {
                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Offense.Efficiency *= normalizationOffenseVector;
                dataPoint.Stands[ConquestUnitOutput.FULL_OUTPUT].Defense.Efficiency *= normalizationDefenseVector;
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

        /// <summary>
        /// Will attempt to apply the given option to the model.  However, if the option does not impact the output score, will return false
        /// </summary>
        /// <param name="model"></param>
        /// <param name="option"></param>
        /// <returns>TRUE if option affects output, FALSE otherwise</returns>
        private static ApplyOptionResult ApplyOptionToUnit(UnitInputModel model, UnitOptionModel option)
        {
            model.Unit += $" ({option.Upgrade})";
            model.Points += option.Points;

            //whether the rule is useful or not depends on the tag
            var isUseful = false;
            var tags = new List<string>(option.Tag.Split('|'));
            foreach (var tag in tags)
            {
                switch (tag.ToLower())
                {
                    case "m":
                        model.Move++;
                        isUseful = true;
                        break;
                    case "v":
                        model.Volley++;
                        isUseful = true;
                        break;
                    case "c":
                        model.Clash++;
                        isUseful = true;
                        break;
                    case "r":
                        model.Resolve++;
                        isUseful = true;
                        break;
                    case "d":
                        model.Defense++;
                        isUseful = true;
                        break;
                    case "e":
                        model.Evasion++;
                        isUseful = true;
                        break;
                    case "alwaysinspire":
                        model.AlwaysInspire = 1;
                        isUseful = true;
                        break;
                    case "isbastion":
                        model.IsBastion = 1;
                        isUseful = true;
                        break;
                    case "isfury":
                        model.IsFury = 1;
                        isUseful = true;
                        break;
                    case "isauradeath":
                        model.IsAuraDeath = 1;
                        isUseful = true;
                        break;
                }
            }

            return isUseful ? ApplyOptionResult.ImpactfulOption : ApplyOptionResult.NonImpactfulOption;
        }
    }
}
