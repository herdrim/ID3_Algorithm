using Id3Algorithm.Services;
using System;

namespace Id3Algorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            DataPreprocessingService dps = new DataPreprocessingService(@".\data.csv", 9, "?");
            DecisionTreeBuilder dtb = new DecisionTreeBuilder(dps);
            dtb.BuildTree();
            
        }
    }
}
