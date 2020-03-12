using System;
using System.Collections.Generic;
using System.Text;

namespace Id3Algorithm.Models
{
    public class EntropyInfoModel
    {
        public int AttributeId { get; set; }
        public double Entropy { get; set; }
        public int DistinctValuesCount { get; set; }
    }
}
