using Id3Algorithm.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Id3Algorithm.Services
{
    public static class DataSerializationService
    {
        public static void SaveTreeToBinary(string path, TreeNodeModel data)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, data);
            }
        }

        public static TreeNodeModel ReadTreeFromBinary(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (TreeNodeModel)binaryFormatter.Deserialize(stream);
            }
        }

        public static List<int[]> GetCasesToMakeDecision(string path, string separator = ",")
        {
            var retVals = new List<int[]>();
            using (StreamReader sr = new StreamReader(path))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    var values = line.Split(separator);
                    int[] makeDecisionCase = new int[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (int.TryParse(values[i], out int res))                        
                            makeDecisionCase[i] = res;                        
                        else
                            makeDecisionCase[i] = 0;
                    }
                    retVals.Add(makeDecisionCase);
                }
            }

            return retVals;
        }
    }
}
