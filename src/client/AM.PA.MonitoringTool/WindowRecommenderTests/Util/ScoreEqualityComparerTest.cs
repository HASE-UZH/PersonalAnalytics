using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender.Util;

namespace WindowRecommenderTests.Util
{
    [TestClass]
    public class ScoreEqualityComparerTest
    {
        [TestMethod]
        public void TestGetHashCode()
        {
            var expected = new KeyValuePair<IntPtr, double>(new IntPtr(1), 1).GetHashCode();
            var actual = new ScoreEqualityComparer().GetHashCode(new KeyValuePair<IntPtr, double>(new IntPtr(1), 1));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestEquals_Equal()
        {
            Assert.IsTrue(new ScoreEqualityComparer().Equals(
                new KeyValuePair<IntPtr, double>(new IntPtr(1), 1),
                new KeyValuePair<IntPtr, double>(new IntPtr(1), 1)
            ), "Same");
            Assert.IsTrue(new ScoreEqualityComparer().Equals(
                new KeyValuePair<IntPtr, double>(new IntPtr(1), 0),
                new KeyValuePair<IntPtr, double>(new IntPtr(1), 0.0000009)
            ), "Less than Delta");
        }

        [TestMethod]
        public void TestEquals_Unequal()
        {
            Assert.IsFalse(new ScoreEqualityComparer().Equals(
                new KeyValuePair<IntPtr, double>(new IntPtr(1), 1),
                new KeyValuePair<IntPtr, double>(new IntPtr(2), 1)
            ), "Key");
            Assert.IsFalse(new ScoreEqualityComparer().Equals(
                new KeyValuePair<IntPtr, double>(new IntPtr(1), 1),
                new KeyValuePair<IntPtr, double>(new IntPtr(1), 2)
            ), "Value");
            Assert.IsFalse(new ScoreEqualityComparer().Equals(
                new KeyValuePair<IntPtr, double>(new IntPtr(1), 0),
                new KeyValuePair<IntPtr, double>(new IntPtr(1), 0.000001)
            ), "Exactly Delta");
        }
    }
}
