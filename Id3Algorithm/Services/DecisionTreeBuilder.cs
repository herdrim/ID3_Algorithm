using Id3Algorithm.Extensions;
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
        private List<int> _selectedAttributes;
        private TreeNodeModel rootNode;
        private List<AttributeInfoModel> _attributesInfo;

        public DecisionTreeBuilder(DataPreprocessingService data)
        {
            var tmpData = data.FormattedData.ToArray();
            _data = new int[tmpData[0].Length][];
            for (int i = 0; i < tmpData[0].Length; i++)
            {
                var oldColumn = tmpData.Select(x => x[i]);
                _data[i] = new int[oldColumn.Count()];
                int j = 0;
                foreach (var val in oldColumn)
                {
                    _data[i][j] = val;
                    j++;
                }
            }
            _decisions = data.Decisions;

            // Dane testowe
            //_data = new int[][]
            //{
            //    new int[] { 1, 1, 3, 1, 1 },
            //    new int[] { 1, 2, 2, 2, 2 },
            //    new int[] { 1, 1, 1, 1, 3 },
            //    new int[] { 2, 2, 1, 3, 4 },
            //    new int[] { 2, 2, 2, 3, 2 },
            //    new int[] { 2, 2, 3, 3, 4 },
            //    new int[] { 2, 2, 2, 2, 2 },
            //    new int[] { 1, 2, 1, 2, 1 }
            //};
            //_decisions = new int[] { 0, 0, 0, 0, 1, 1, 1, 1 };


            _selectedAttributes = new List<int>();
            _attributesInfo = new List<AttributeInfoModel>();
            for (int i = 0; i < _data[0].Length; i++)
            {
                _attributesInfo.Add(new AttributeInfoModel
                {
                    AttributeId = i,
                    AttributeLabel = i.ToString(),
                    DistinctValuesCount = _data.Select(x => x[i]).Distinct().Count()
                });
            }
        }

        public TreeNodeModel BuildTree()
        {            
            
            var entropyTable = CreateEntropyTable(_data);
            int selectedAttr = GetSelectedAttributeByEntropyTable(entropyTable);
            rootNode = CreateRootNode(selectedAttr);

            return rootNode;
        }

        private TreeNodeModel CreateSubNode(TreeNodeModel parent, int[][] data)
        {
            Random r = new Random();
            var nodesToProcess = new List<TreeNodeProcessModel>();
            var mainNode = new TreeNodeModel()
            {
                Parent = parent
            };
            nodesToProcess.Add(new TreeNodeProcessModel()
            {
                Node = mainNode,
                UpdatedData = data
            });            

            do
            {
                var currentNode = nodesToProcess.FirstOrDefault();
                var indexes = currentNode.UpdatedData.Select(x => x[0]).ToArray().GetIndexesOpositeToElement(-1);
                var decisions = _decisions.GetValuesByIndexes(indexes);
                if (decisions.Distinct().Count() == 1)
                {
                    currentNode.Node.Attribute = null;
                    currentNode.Node.Decision = decisions.FirstOrDefault();                    
                }
                else
                {
                    var entropyTable = CreateEntropyTable(currentNode.UpdatedData);
                    if (!entropyTable.Any())
                    {
                        var groupedDecisions = decisions.GroupBy(x => x).Select(x => new { key = x.Key, count = x.Count() });
                        currentNode.Node.Decision = groupedDecisions.FirstOrDefault(x => x.count == groupedDecisions.Max(x => x.count)).key;
                    }
                    else
                    {
                        int selectedAttr = GetSelectedAttributeByEntropyTable(entropyTable);

                        currentNode.Node.Attribute = selectedAttr;
                        currentNode.Node.Decision = null;

                        var children = new List<ChildNodeModel>();// new Dictionary<int, TreeNodeModel>();
                        var values = currentNode.UpdatedData.Where(x => x[0] != -1).Select(x => x[selectedAttr]).ToArray();
                        var distinctValues = values.Distinct();
                        if (distinctValues.Count() > 1)
                        {
                            foreach (var distinctVal in distinctValues)
                            {
                                int[][] updatedData = new int[currentNode.UpdatedData.Length][];

                                for (int i = 0; i < currentNode.UpdatedData.Length; i++)
                                {
                                    int[] tmpData = new int[currentNode.UpdatedData[i].Length];
                                    if (currentNode.UpdatedData[i][selectedAttr] == distinctVal)
                                        Array.Copy(currentNode.UpdatedData[i], 0, tmpData, 0, _data[i].Length);
                                    else
                                    {
                                        for (int j = 0; j < tmpData.Length; j++)
                                            tmpData[j] = -1;
                                    }
                                    updatedData[i] = tmpData;

                                }
                                TreeNodeModel subNode = new TreeNodeModel()
                                {
                                    Parent = currentNode.Node
                                };
                                children.Add(new ChildNodeModel() 
                                { 
                                    Key = distinctVal, 
                                    Child = subNode 
                                });
                                nodesToProcess.Add(new TreeNodeProcessModel()
                                {
                                    Node = subNode,
                                    UpdatedData = updatedData
                                });
                            }
                        }
                        else
                        {
                            var groupedDecisions = decisions.GroupBy(x => x).Select(x => new { key = x.Key, count = x.Count() });
                            children.Add(new ChildNodeModel()
                            {
                                Key = distinctValues.FirstOrDefault(),
                                Child = new TreeNodeModel()
                                {
                                    Parent = currentNode.Node,
                                    Decision = groupedDecisions.FirstOrDefault(x => x.count == groupedDecisions.Max(x => x.count)).key
                                }
                            });
                        }
                        currentNode.Node.ChildNodes = children;
                    }
                }
                nodesToProcess.Remove(currentNode);
            } while (nodesToProcess.Any());
            
            return mainNode;
        }

        private TreeNodeModel CreateRootNode(int attrId)
        {
            TreeNodeModel rootNode = new TreeNodeModel
            {
                Attribute = attrId,
                Decision = null,
                Parent = null
            };

            var children = new List<ChildNodeModel>();// new Dictionary<int, TreeNodeModel>();
            var values = _data.Select(x => x[attrId]).ToArray();
            var distinctValues = values.Distinct();


            foreach (var distinctVal in distinctValues)
            {
                int[][] updatedData = new int[_data.Length][];

                for (int i = 0; i < _data.Length; i++)
                {
                    int[] tmpData = new int[_data[i].Length];
                    if (_data[i][attrId] == distinctVal)
                        Array.Copy(_data[i], 0, tmpData, 0, _data[i].Length);
                    else
                    {
                        for (int j = 0; j < tmpData.Length; j++)
                            tmpData[j] = -1;
                    }
                    updatedData[i] = tmpData;

                }
                children.Add(new ChildNodeModel()
                {
                    Key = distinctVal,
                    Child = CreateSubNode(rootNode, updatedData)
                });
                
            }
            rootNode.ChildNodes = children;
            return rootNode;
        }

        private int GetSelectedAttributeByEntropyTable(List<EntropyInfoModel> entropyTable)
        {
            // Wybieramy atrybut z największą ilością informacji
            var minEntropy = entropyTable.Min(x => x.Entropy);
            var tmpMinEntropies = entropyTable.Where(x => x.Entropy == minEntropy);
            int selectedAttr;
            if (tmpMinEntropies.Count() > 1)
            {
                var minValuesCount = tmpMinEntropies.Min(x => x.DistinctValuesCount);
                var minEntriopiesAndMinDistValues = entropyTable.Where(x => x.Entropy == minEntropy && x.DistinctValuesCount == minValuesCount).Select(x => x.AttributeId);
                if (minEntriopiesAndMinDistValues.Count() > 1)
                {
                    var attributesSelectedBefore = _selectedAttributes.Intersect(minEntriopiesAndMinDistValues);
                    if (attributesSelectedBefore.Count() > 0)
                        return attributesSelectedBefore.FirstOrDefault();
                    else
                        return minEntriopiesAndMinDistValues.FirstOrDefault();
                }                    
                else
                    selectedAttr = minEntriopiesAndMinDistValues.FirstOrDefault();
            }
            else            
                selectedAttr = tmpMinEntropies.FirstOrDefault().AttributeId;

            _selectedAttributes.Add(selectedAttr);
            return selectedAttr;            
        }

        private List<EntropyInfoModel> CreateEntropyTable(int[][] updatedData)
        {
            // Tworzy tabele zawierającą pozycje atrybutu, jego entropie i liczbe unikalnych wartości
            List<EntropyInfoModel> entropyInfos = new List<EntropyInfoModel>();

            for (int i = 0; i < updatedData[0].Length; i++)
            {
                if (!_selectedAttributes.Any(x => x == i))
                {
                    // Wyliczamy entropie dla każdego atrybutu
                    var values = updatedData
                        .Select(x => x[0] == -1 ? -1 : x[i])
                        .ToArray();
                    var valuesWithoutRemoved = updatedData
                        .Where(x => x[0] != -1)
                        .Select(x => x[i])
                        .ToArray();
                    int allCount = valuesWithoutRemoved.Length;
                    var distinctValues = valuesWithoutRemoved.Distinct();
                    double entropy = 0.0;

                    foreach (var distVal in distinctValues)
                    {
                        int valuesCount = values.Count(x => x == distVal);
                        var indexes = values.GetIndexesOfElement(distVal);
                        var decisionsForVal = _decisions.GetValuesByIndexes(indexes);
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
                        DistinctValuesCount = _attributesInfo.FirstOrDefault(x => x.AttributeId == i).DistinctValuesCount
                    });
                }
            }

            return entropyInfos;
        }
    }
}
