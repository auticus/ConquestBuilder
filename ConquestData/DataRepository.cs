using ConquestData.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ConquestData
{
    public class DataRepository
    {
        public static List<UnitInputModel> GetUnitInput(string filePath)
        {
            var models = new List<UnitInputModel>();

            using (var rdr = new StreamReader(filePath))
            {
                var header = rdr.ReadLine().Split(',');

                while (!rdr.EndOfStream)
                {
                    var line = rdr.ReadLine().Split(',');

                    if (header.Length != line.Length)
                        throw new InvalidOperationException("The header and the data line do not match length of field count");

                    models.Add(ProcessUnitModel(header, line));
                }
            }

            return models;
        }

        private static UnitInputModel ProcessUnitModel(string[] header, string[] nibble)
        {
            var model = new UnitInputModel();
            var stringFields = GetAllStringFields(); //the dataset is either numeric or strings

            for (int i = 0; i < header.Length; i++)
            {
                if (stringFields.Contains(header[i]))
                    AssignString(header[i], nibble[i], model);
                else
                    AssignInt(header[i], nibble[i], model);
            }
            return model;
        }

        private static List<string> GetAllStringFields()
        {
            var returnList = new List<string>()
            {
                "Faction",
                "Unit",
                "Weight",
                "Intangibles"
            };

            return returnList;
        }

        private static void AssignString(string header, string data, UnitInputModel model)
        {
            switch (header)
            {
                case "Faction":
                    model.Faction = data;
                    break;
                case "Unit":
                    model.Unit = data;
                    break;
                case "Weight":
                    model.Weight = data;
                    break;
                case "Intangibles":
                    model.Intangibles = data;
                    break;
            }
        }

        private static void AssignInt(string header, string data, UnitInputModel model)
        {
            int out_data;
            if (!int.TryParse(data, out out_data))
                throw new InvalidOperationException($"\"{header}\" value passed was no numeric");

            switch (header)
            {
                case "Models":
                    model.Models = out_data;
                    break;
                case "M":
                    model.UnitStats.Move = out_data;
                    break;
                case "V":
                    model.UnitStats.Volley = out_data;
                    break;
                case "C":
                    model.UnitStats.Clash = out_data;
                    break;
                case "A":
                    model.UnitStats.Attacks = out_data;
                    break;
                case "W":
                    model.UnitStats.Wounds = out_data;
                    break;
                case "R":
                    model.UnitStats.Resolve = out_data;
                    break;
                case "D":
                    model.UnitStats.Defense = out_data;
                    break;
                case "E":
                    model.UnitStats.Evasion = out_data;
                    break;
                case "Barrage":
                    model.UnitRules.Barrage = out_data;
                    break;
                case "Range":
                    model.UnitRules.Range = out_data;
                    break;
                case "AP":
                    model.UnitRules.ArmorPiercing = out_data;
                    break;
                case "Cleave":
                    model.UnitRules.Cleave = out_data;
                    break;
                case "IsImpact":
                    model.UnitRules.IsImpact = out_data;
                    break;
                case "BrutalImpact":
                    model.UnitRules.BrutalImpact = out_data;
                    break;
                case "IsShields":
                    model.UnitRules.IsShields = out_data;
                    break;
                case "IsFury":
                    model.UnitRules.IsFury = out_data;
                    break;
                case "IsFlurry":
                    model.UnitRules.IsFlurry = out_data;
                    break;
                case "IsFluid":
                    model.UnitRules.IsFluid = out_data;
                    break;
                case "IsFly":
                    model.UnitRules.IsFly = out_data;
                    break;
                case "IsDeadlyShot":
                    model.UnitRules.IsDeadlyShot = out_data;
                    break;
                case "IsDeadlyBlades":
                    model.UnitRules.IsDeadlyBlades = out_data;
                    break;
                case "IsAuraDeath":
                    model.UnitRules.IsAuraDeath = out_data;
                    break;
                case "IsSupport":
                    model.UnitRules.IsSupport = out_data;
                    break;
                case "IsBastion":
                    model.UnitRules.IsBastion = out_data;
                    break;
                case "IsTerrifying":
                    model.UnitRules.IsTerrifying = out_data;
                    break;
                case "IsArcOfFire":
                    model.UnitRules.IsArcOfFire = out_data;
                    break;
            }
        }
    }
}
