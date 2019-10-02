using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender;
using WindowRecommender.Util;

namespace WindowRecommenderTests.Util
{
    [TestClass]
    public class UtilsTest
    {
        /// <summary>
        /// Delta for comparing double values
        /// </summary>
        private const double Delta = 0.000001;

        [TestMethod]
        public void TestPairwise()
        {
            var source = new[] { "a", "b", "c", "d" };
            var result = source.Pairwise((one, two) => one + two);
            CollectionAssert.AreEqual(new[] { "ab", "bc", "cd" }, result.ToArray());
        }

        [TestMethod]
        public void TestPairwise_Empty()
        {
            var source = new string[0];
            var result = source.Pairwise((one, two) => one + two);
            CollectionAssert.AreEqual(new string[0], result.ToArray());
        }

        [TestMethod]
        public void TestPairwise_Single()
        {
            var source = new[] { "a" };
            var result = source.Pairwise((one, two) => one + two);
            CollectionAssert.AreEqual(new string[0], result.ToArray());
        }

        [TestMethod]
        public void TestGetTopEntries_Empty()
        {
            var scores = new Dictionary<IntPtr, double>();
            var topWindows = Utils.GetTopEntries(scores, 3);
            Assert.IsTrue(Enumerable.Empty<IntPtr>().SequenceEqual(topWindows));
        }

        [TestMethod]
        public void TestGetTopEntries_Count()
        {
            const int count = 3;
            var scores = new Dictionary<IntPtr, double>();
            for (var i = 1; i < count + 2; i++)
            {
                scores[new IntPtr(i)] = 1;
            }
            var topWindows = Utils.GetTopEntries(scores, count);
            Assert.AreEqual(Settings.NumberOfWindows, topWindows.Count());
        }

        [TestMethod]
        public void TestCosineSimilarity()
        // Values from https://github.com/compute-io/cosine-similarity/blob/90c19d27cb306696a6591f1e7a047cddd78b5724/test/test.js#L103-L115
        {
            const double expectedSimilarity = 1 - 0.04397873;
            var x = new double[] { 2, 4, 5, 3, 8, 2 };
            var y = new double[] { 3, 1, 5, 3, 7, 2 };
            var actualSimilarity = Utils.CosineSimilarity(x, y);
            Assert.AreEqual(actualSimilarity, expectedSimilarity, Delta);
        }

        [TestMethod]
        public void TestCosineSimilarity2()
        // Values from https://stackoverflow.com/a/1750187/1469028
        {
            const double expectedSimilarity = 0.821583;
            var x = new double[] { 2, 0, 1, 1, 0, 2, 1, 1 };
            var y = new double[] { 2, 1, 1, 0, 1, 1, 1, 1 };
            var actualSimilarity = Utils.CosineSimilarity(x, y);
            Assert.AreEqual(actualSimilarity, expectedSimilarity, Delta);
        }

        [TestMethod]
        public void TestCosineSimilarity_Empty()
        {
            var x = new double[0];
            var y = new double[0];
            var actualSimilarity = Utils.CosineSimilarity(x, y);
            Assert.AreEqual(actualSimilarity, 0);
        }
    }
}
