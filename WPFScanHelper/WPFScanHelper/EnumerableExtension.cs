using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFScanHelper
{
    public static class EnumerableExtension
    {
        public static IEnumerable<IEnumerable<T>> EnumerateSubsequencesStartingWithFirstElement<T>(
            this IEnumerable<T> sequence)
        {
            var subsequence = new List<T>();

            foreach (var element in sequence)
            {
                subsequence.Add(element);
                yield return subsequence.ToArray();
            }
        }
    }
}
