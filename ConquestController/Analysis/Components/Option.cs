using System;
using System.Collections.Generic;
using System.Text;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

namespace ConquestController.Analysis.Components
{
    public enum ApplyExtrasResult
    {
        NotOption = 0,
        ImpactfulOption,
        ImpactfulWithUnit,
        NonImpactfulOption,
    }

    public class Option
    {
        /// <summary>
        /// Takes in an option and applys it to the unit passed in.  If the option is null, does nothing and returns
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="option"></param>
        public static void ProcessOption<T>(AnalysisInput<T> input, ConquestUnitOutput output, IOption option) where T : ConquestInput<T>
        {
            //process options
            var optionResult = ApplyExtrasResult.NotOption;
            if (option == null) return;

            optionResult = ApplyOptionToUnit(input.Model, option);

            switch (optionResult)
            {
                case ApplyExtrasResult.ImpactfulOption:
                    output.HasOptionAdded = true;
                    break;
                case ApplyExtrasResult.NonImpactfulOption:
                case ApplyExtrasResult.ImpactfulWithUnit: //no reason to include this in broad analysis it needs the unit he's with to matter and we dont know that in broad analysis
                    output.HasNoImpactOptionAdded = true;
                    break;
            }
        }

        /// <summary>
        /// Will attempt to apply the given option to the model.  However, if the option does not impact the output score, will return false
        /// </summary>
        /// <param name="model"></param>
        /// <param name="option"></param>
        /// <returns>TRUE if option affects output, FALSE otherwise</returns>
        private static ApplyExtrasResult ApplyOptionToUnit<T>(T model, IOption option) where T : ConquestInput<T>
        {
            //NonImpactfulOption - an intangible that cannot be analyzed with math
            //ImpactfulWithUnit - is impactful but only after the character is joined with the unit 
            //ImpactfulOption - impactful always

            //option.SelfOnly applies only to character inputs, and means only applying to the character themself

            model.Unit += $" ({option.Name})";
            model.Points += option.Points;

            var impactful =
                option.SelfOnly == 1 ? ApplyExtrasResult.ImpactfulWithUnit : ApplyExtrasResult.ImpactfulOption;

            var onlyImpactfulWithUnit = option.SelfOnly == 1
                ? ApplyExtrasResult.NonImpactfulOption
                : ApplyExtrasResult.ImpactfulWithUnit;

            //whether the rule is useful or not depends on the tag
            ApplyExtrasResult result = ApplyExtrasResult.NotOption;
            var tags = new List<string>(option.Tag.Split('|'));
            foreach (var tag in tags)
            {
                switch (tag.ToLower())
                {
                    case "m":
                        model.Move++;
                        result = impactful;
                        break;
                    case "v":
                        model.Volley++;
                        result = impactful;
                        break;
                    case "c":
                        model.Clash++;
                        result = impactful;
                        break;
                    case "r":
                        model.Resolve++;
                        result = ApplyExtrasResult.ImpactfulWithUnit;
                        break;
                    case "d":
                        model.Defense++;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "d-":
                        model.Defense--;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "e":
                        model.Evasion++;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "de":
                        model.BuffDefenseOrEvasion = true;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "alwaysinspire":
                        model.AlwaysInspire = 1;
                        result = impactful;
                        break;
                    case "cleave1":
                        model.Cleave = 1;
                        result = impactful;
                        break;
                    case "cleave2":
                        model.Cleave = 2;
                        result = impactful;
                        break;
                    case "cleave3":
                        model.Cleave = 3;
                        result = impactful;
                        break;
                    case "isbastion":
                        model.IsBastion = 1;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "isfury":
                        model.IsFury = 1;
                        result = impactful;
                        break;
                    case "isauradeath":
                        model.IsAuraDeath = 1;
                        result = impactful;
                        break;
                    case "isdeadlyblades":
                        model.IsDeadlyBlades = 1;
                        result = impactful;
                        break;
                    case "isdeadlyshots":
                        model.IsDeadlyShot = 1;
                        result = impactful;
                        break;
                    case "isblessed":
                        model.IsBlessed = 1;
                        result = impactful;
                        break;
                    case "isfluid":
                        model.IsFluid = 1;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "healing1":
                        model.Healing = 1;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "healing2":
                        model.Healing = 2;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "healing3":
                        model.Healing = 3;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "healing4":
                        model.Healing = 4;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "healing5":
                        model.Healing = 5;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "noobscure":
                        model.NoObscure = true;
                        result = impactful;
                        break;
                    case "terrify":
                        model.IsTerrifying = 1;
                        result = impactful;
                        break;
                    case "reroll6_volley":
                        model.Reroll6_Volley = true;
                        result = impactful;
                        break;
                    case "reroll6_defense":
                        model.Reroll6_Defense = true;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "meleeheal4":
                        model.MeleeHeal4 = true;
                        result = ApplyExtrasResult.NonImpactfulOption;  //intangible for right now
                        break;
                    case "doubleattack":
                        model.DoubleAttack = true;
                        result = impactful;
                        break;
                    case "onehitperfile":
                        model.OneHitPerFile = true;
                        result = impactful;
                        break;
                    case "d_volley":
                        model.D_Volley = true;
                        result = onlyImpactfulWithUnit;
                        break; 
                    case "decay1":
                        model.Decay = 1;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "decay2":
                        model.Decay = 2;
                        result = onlyImpactfulWithUnit;
                        break;
                    case "decay3":
                        model.Decay = 3;
                        result = onlyImpactfulWithUnit;
                        break;
                }
            }

            return result;
        }
    }
}