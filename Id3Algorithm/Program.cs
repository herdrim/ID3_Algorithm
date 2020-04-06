using Id3Algorithm.Services;
using System;
using System.Collections.Generic;

namespace Id3Algorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            RunType type = args[0].StartsWith("l", StringComparison.InvariantCultureIgnoreCase) ? RunType.Learning : (args[0].StartsWith("d", StringComparison.InvariantCultureIgnoreCase) ? RunType.DecisionMaking : RunType.None);

            if (type == RunType.Learning && args.Length >= 4 )
            {
                string dataPath = args[1];                
                string pathToSaveTree = args[2];
                string dataMissingValueCharacter = args[3];
                string dataSeparator = ",";
                if (args.Length >= 5)
                     dataSeparator = args[4];


                DataPreprocessingService dps = new DataPreprocessingService(dataPath, 9, dataMissingValueCharacter, dataSeparator);
                DecisionTreeBuilder dtb = new DecisionTreeBuilder(dps);
                var dataToSave = dtb.BuildTree();
                DataSerializationService.SaveTreeToBinary(pathToSaveTree, dataToSave);
            }
            else if (type == RunType.DecisionMaking && args.Length >= 3)
            {
                string pathToReadTree = args[1];
                List<int[]> dataToMakeDecision = DataSerializationService.GetCasesToMakeDecision(args[2], args[3]);

                var data = DataSerializationService.ReadTreeFromBinary(pathToReadTree);
                DecisionMakerService dms = new DecisionMakerService(data);
                dms.MakeDecisions(dataToMakeDecision);
            }
            else
            {
                Console.WriteLine("Błędne uruchomienie");
            }
            Console.ReadKey();
        }
    }


    public enum RunType
    {
        Learning,
        DecisionMaking,
        None
    }
}
