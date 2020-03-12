using Id3Algorithm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Id3Algorithm.Services
{
    public class DecisionTreeBuilder
    {
        private int[][] _data;
        private int[] _decisions;

        public DecisionTreeBuilder()
        {
            //_data = data.FormattedData;
            //_decisions = data.Decisions;
            
            // Dane testowe
            _data = new int[][]
            {
                new int[] { 1, 1, 3, 1, 1 },
                new int[] { 1, 2, 2, 2, 2 },
                new int[] { 1, 1, 1, 1, 3 },
                new int[] { 2, 2, 1, 3, 4 },
                new int[] { 2, 2, 2, 3, 2 },
                new int[] { 2, 2, 3, 3, 4 },
                new int[] { 2, 2, 2, 2, 2 },
                new int[] { 1, 2, 1, 2, 1 }
            };
            _decisions = new int[] { 0, 0, 0, 0, 1, 1, 1, 1 };            
        }

        public void BuildTree()
        {            
            
            var entropyTable = CreateEntropyTable();
            int selectedAttr = GetSelectedAttributeByEntropyTable(entropyTable);

            // TO DO ZBUDOWAĆ DRZEWO I ZAPISAĆ DANE


        }

        private int GetSelectedAttributeByEntropyTable(List<EntropyInfoModel> entropyTable)
        {
            // Wybieramy atrybut z największą ilością informacji
            var minEntropy = entropyTable.Min(x => x.Entropy);
            var tmpMinEntropies = entropyTable.Where(x => x.Entropy == minEntropy);
            if (tmpMinEntropies.Count() > 1)
            {
                var minValuesCount = tmpMinEntropies.Min(x => x.DistinctValuesCount);
                return entropyTable.FirstOrDefault(x => x.Entropy == minEntropy && x.DistinctValuesCount == minValuesCount).AttributeId;
            }
            else
                return tmpMinEntropies.FirstOrDefault().AttributeId;
        }

        private List<EntropyInfoModel> CreateEntropyTable()
        {
            // Tworzy tabele zawierającą pozycje atrybutu, jego entropie i liczbe unikalnych wartości
            List<EntropyInfoModel> entropyInfos = new List<EntropyInfoModel>();

            for (int i = 0; i < _data[0].Length; i++)
            {
                // Wyliczamy entropie dla każdego atrybutu
                var values = _data.Select(x => x[i]).ToArray();
                int allCount = values.Length;
                var distinctValues = values.Distinct();
                double entropy = 0.0;

                foreach (var distVal in distinctValues)
                {
                    int valuesCount = values.Count(x => x == distVal);
                    var indexes = GetIndexesOfElement(values, distVal);
                    var decisionsForVal = _decisions.Where((e, i) => indexes.Contains(i));
                    var distDecisions = decisionsForVal.Distinct();

                    double subtotal = 0.0;

                    foreach (var distDec in distDecisions)
                    {
                        int decCount = decisionsForVal.Count(x => x == distDec);
                        double tmp = decCount / (double)valuesCount;
                        subtotal += tmp * Math.Log2(tmp);
                    }

                    entropy += valuesCount / (double)allCount * subtotal;
                }

                entropyInfos.Add(new EntropyInfoModel
                {
                    AttributeId = i,
                    Entropy = entropy * -1,
                    DistinctValuesCount = distinctValues.Count()
                });
            }

            return entropyInfos;
        }

        private int[] GetIndexesOfElement(int[] array, int element)
        {
            return array.Select((el, i) => el == element ? i : -1).Where(i => i != -1).ToArray();
        }
    }
}
