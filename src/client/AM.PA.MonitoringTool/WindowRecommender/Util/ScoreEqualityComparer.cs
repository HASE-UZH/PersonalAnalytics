using System;
using System.Collections.Generic;

namespace WindowRecommender.Util
{
    internal class ScoreEqualityComparer : IEqualityComparer<KeyValuePair<IntPtr, double>>
    {
        public bool Equals(KeyValuePair<IntPtr, double> x, KeyValuePair<IntPtr, double> y)
        {
            return x.Key == y.Key && (x.Value - y.Value).IsZero();
        }

        public int GetHashCode(KeyValuePair<IntPtr, double> obj)
        {
            return obj.GetHashCode();
        }
    }
}
