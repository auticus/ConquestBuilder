﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ConquestController.Models.Output;

namespace ConquestController.Data
{
    public class AnalysisFile
    {
        public static void WriteAnalysis(string filePath, IList<ConquestUnitOutput> data)
        {
            using var writer = new StreamWriter(filePath, append: false);
            //header
            writer.WriteLine(
                "Faction,Unit,Weight,Points,Additional,NormalizedMovement,ClashOffense,RangedOffense,NormalizedOffense," +
                "TotalDefense,OutputScore,OffenseEfficiency,DefenseEfficiency,Efficiency");

            foreach (var dataPoint in data)
            {
                writer.WriteLine(dataPoint.FullStandToCommaFormat());
            }

            writer.Flush();
            writer.Close();
        }
    }
}
