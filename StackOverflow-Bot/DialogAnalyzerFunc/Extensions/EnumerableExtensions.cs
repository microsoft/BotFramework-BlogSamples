using System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DialogAnalyzerFunc.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<Tuple<TValue, TValue>> ToTuples<TValue>(this IEnumerable<TValue> values)
        {
            if (values.Count() % 2 != 0)
            {
                throw new ArgumentException("Values does not have an even number of items.");
            }

            List<Tuple<TValue, TValue>> results = new List<Tuple<TValue, TValue>>();

            if (values.Count() == 0)
            {
                return results;
            }

            for (int index = 0; index < values.Count(); index += 2)
            {
                results.Add(new Tuple<TValue, TValue>(values.ElementAt(index), values.ElementAt(index + 1)));
            }

            return results;
        }
    }
}
