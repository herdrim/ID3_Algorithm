using Id3Algorithm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Id3Algorithm.Services
{
    public class DecisionMakerService
    {
        private TreeNodeModel _decisionTree;

        public DecisionMakerService(TreeNodeModel decisionTree)
        {
            _decisionTree = decisionTree;
        }

        public int MakeDecision(int[] dataToCheck)
        {
            bool hasDecision = false;
            int decision = 0;
            var node = _decisionTree;

            while (!hasDecision)
            {
                if (node.Decision != null)
                {
                    hasDecision = true;
                    decision = node.Decision ?? 0;
                }
                else
                {
                    if (node != null && node.Attribute != null && dataToCheck.Length > node.Attribute)
                    {
                        int val = dataToCheck[node.Attribute ?? 0];
                        node = node.ChildNodes.FirstOrDefault(x => x.Key == val)?.Child;
                    }
                    else
                        return int.MinValue;
                }
            }

            return decision;
        }

        public void MakeDecisions(List<int[]> dataToCheck)
        {
            int count = 1;
            foreach (var line in dataToCheck)
            {
                var decision = MakeDecision(line);

                string s = "";
                foreach (var val in line)
                {
                    if (String.IsNullOrEmpty(s))
                        s += val;
                    else
                        s += ", " + val;
                }
                Console.WriteLine($"Przypadek {count}:");
                Console.WriteLine(s);
                Console.WriteLine($"Decyzja: {decision}");
                Console.WriteLine();
            }
        }
    }
}
