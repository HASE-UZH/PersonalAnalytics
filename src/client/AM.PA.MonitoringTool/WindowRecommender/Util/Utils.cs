using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowRecommender.Util
{
    internal static class Utils
    {
        /// <summary>
        /// Epsilon for double precision
        /// </summary>
        private const double Epsilon = 0.000001;

        /// <summary>
        /// Iterate over a an enumerable in pairs.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
        /// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to pair.</param>
        /// <param name="resultSelector">A function to create a result value from each pair.</param>
        /// <returns>A collection of elements of type <typeparamref name="TResult" /> where each element represents a projection over a pair.</returns>
        /// https://stackoverflow.com/a/1581482/1469028
        public static IEnumerable<TResult> Pairwise<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> resultSelector)
        {
            var previous = default(TSource);
            using (var it = source.GetEnumerator())
            {
                if (it.MoveNext())
                {
                    previous = it.Current;
                }
                while (it.MoveNext())
                {
                    yield return resultSelector(previous, previous = it.Current);
                }
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key or the default fo
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary whose value to return.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>When this method returns, the value associated with the specified key, if the key is found;
        /// otherwise, the default value for <typeparamref name="TValue" />.</returns>
        /// https://stackoverflow.com/a/538751/1469028
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            // Ignore return value
            dictionary.TryGetValue(key, out var ret);
            return ret;
        }

        internal static bool IsZero(this double value)
        {
            return Math.Abs(value) < Epsilon;
        }

        internal static IEnumerable<IntPtr> GetTopEntries(Dictionary<IntPtr, double> scores, int count)
        {
            return scores.OrderByDescending(x => x.Value).Select(x => x.Key).Take(count);
        }

        internal static double CosineSimilarity(IReadOnlyList<double> x, IReadOnlyList<double> y)
        {
            var num1 = 0.0;
            var d1 = 0.0;
            var d2 = 0.0;
            for (var index = 0; index < x.Count; ++index)
            {
                num1 += x[index] * y[index];
                d1 += x[index] * x[index];
                d2 += y[index] * y[index];
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (num1 != 0.0)
            {
                var num2 = Math.Sqrt(d1) * Math.Sqrt(d2);
                return num1 / num2;
            }
            return 0.0;
        }
    }
}
