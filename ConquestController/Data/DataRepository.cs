using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ConquestController.Models;
using ConquestController.Models.Input;

namespace ConquestController.Data
{
    public class DataRepository
    {
        private static readonly string DATAFILE_NULL = "[]";
        /// <summary>
        /// Loads a file and returns back a list of the model contained within
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"/>
        public static List<T> GetInputFromFileToList<T>(string filePath) where T: new()
        {
            var models = new List<T>();

            using (var rdr = new StreamReader(filePath))
            {
                var header = rdr.ReadLine().Split(',');

                while (!rdr.EndOfStream)
                {
                    var line = rdr.ReadLine().Split(',');

                    if (header.Length != line.Length)
                        throw new InvalidOperationException("The header and the data line do not match length of field count");

                    models.Add(ProcessModel<T>(header, line));
                }
            }

            return models;
        }

        /// <summary>
        /// Given a list of models already populated, pull their options and populate their option collection
        /// </summary>
        /// <param name="models">The list of models to assign the options to</param>
        /// <param name="filePath">The file path of the options file</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"/>
        public static void AssignUnitOptionsToModelsFromFile(List<IConquestRegimentInput> models, string filePath)
        {
            using var rdr = new StreamReader(filePath);

            if (rdr == null) throw new InvalidOperationException("The file path passed resulted in no viable data to pull");

            var header = rdr.ReadLine()?.Split(',');

            if (header == null) throw new InvalidOperationException("There was no header line within the file passed");

            while (!rdr.EndOfStream)
            {
                var line = rdr.ReadLine()?.Split(',');

                if (line == null) throw new InvalidOperationException("Null data found within the file passed");

                if (header.Length != line.Length)
                    throw new InvalidOperationException("The header and the data line do not match length of field count");

                var option = ProcessModel<UnitOptionModel>(header, line);
                var model = models.FirstOrDefault(x => x.Faction == option.Faction && x.Unit == option.Unit);

                model?.Options.Add(option);
            }
        }

        public static void AssignDelimitedPropertyToList(List<string> list, string data, char delimiter = '|')
        {
            var splitData = data.Split(delimiter);

            foreach (var element in splitData)
            {
                if (element == string.Empty) continue;
                list.Add(element);
            }
        }

        /// <summary>
        /// Processes the line passed to it and returns the individual model back when complete
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="header"></param>
        /// <param name="nibble"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"/>
        private static T ProcessModel<T>(string[] header, string[] nibble) where T: new()
        {
            T model = new T();
            var modelType = typeof(T);

            for (int i = 0; i < header.Length; i++)
            {
                var property = modelType.GetProperty(header[i]);
                if (property != null)
                {
                    SetValue(property, header[i], model, nibble[i]);
                }
            }

            return model;
        }

        private static void SetValue<T>(PropertyInfo property, string fieldName, T model, string value)
        {
            if (property.PropertyType == typeof(string))
            {
                if (value == DATAFILE_NULL) property.SetValue(model, string.Empty);
                else property.SetValue(model, value);
            }
            else if (property.PropertyType == typeof(int))
            {
                if (value == DATAFILE_NULL) return;
                if (!int.TryParse(value, out int output))
                    throw new InvalidOperationException($"Field {fieldName} was passed and a number expected, but the value {value} was given");

                property.SetValue(model, output);
            }
        }
    }
}
