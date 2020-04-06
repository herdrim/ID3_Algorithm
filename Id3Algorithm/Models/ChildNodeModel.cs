using System;
using System.Collections.Generic;
using System.Text;

namespace Id3Algorithm.Models
{
    [Serializable]
    public class ChildNodeModel
    {
        public int Key { get; set; }
        public TreeNodeModel Child { get; set; }
    }
}
