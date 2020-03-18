using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Id3Algorithm.Extensions
{
    public static class ArrayExtension
    {
        public static int[] GetIndexesOfElement(this int[] array, int element)
        {
            return array.Select((el, i) => el == element ? i : -1).Where(i => i != -1).ToArray();
        }

        public static int[] GetIndexesOpositeToElement(this int[] array, int element)
        {
            return array.Select((el, i) => el != element ? i : -1).Where(i => i != -1).ToArray();
        }

        public static int[] GetValuesByIndexes(this int[] array, int[] indexes)
        {
            return array.Where((el, i) => indexes.Contains(i)).ToArray();
        }
    }
}
