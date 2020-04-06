using System;
using System.Collections.Generic;
using System.Text;

namespace Id3Algorithm.Models
{
    [Serializable]
    public class TreeNodeModel
    {
        public int? Attribute { get; set; }
        public int? Decision { get; set; }
        public TreeNodeModel Parent { get; set; }
        public List<ChildNodeModel> ChildNodes { get; set; }
    }
}
