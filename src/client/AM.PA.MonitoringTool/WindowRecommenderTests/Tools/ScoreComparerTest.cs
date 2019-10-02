using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowRecommenderTests.Tools
{
    [TestClass]
    public class ScoreComparerTest
    {
        private ScoreComparer _scoreComparer;

        [TestInitialize]
        public void Initialize()
        {
            _scoreComparer = new ScoreComparer();
        }

        [TestMethod]
        public void TestCompare_ArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => _scoreComparer.Compare(null, null));
            Assert.ThrowsException<ArgumentException>(() => _scoreComparer.Compare(1, null));
            Assert.ThrowsException<ArgumentException>(() => _scoreComparer.Compare(1, 2));
        }

        [TestMethod]
        public void TestCompare_Equal()
        {
            var x = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0.1);
            var y = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0.1);
            Assert.AreEqual(0, _scoreComparer.Compare(x, y));
        }

        [TestMethod]
        public void TestCompare_Equal_Delta()
        {
            var x = new KeyValuePair<IntPtr, double>(new IntPtr(1), 1);
            var y = new KeyValuePair<IntPtr, double>(new IntPtr(1), 1.000001);
            Assert.AreEqual(0, _scoreComparer.Compare(x, y));
        }

        [TestMethod]
        public void TestCompare_Smaller_Key()
        {
            var x = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0);
            var y = new KeyValuePair<IntPtr, double>(new IntPtr(3), 0);
            Assert.AreEqual(-2, _scoreComparer.Compare(x, y));
        }

        [TestMethod]
        public void TestCompare_Smaller_Value()
        {
            var x = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0);
            var y = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0.1);
            Assert.AreEqual(-1, _scoreComparer.Compare(x, y));
        }

        [TestMethod]
        public void TestCompare_Smaller_Value_Delta()
        {
            var x = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0);
            var y = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0.00009);
            Assert.AreEqual(-1, _scoreComparer.Compare(x, y));
        }

        [TestMethod]
        public void TestCompare_Larger_Key()
        {
            var x = new KeyValuePair<IntPtr, double>(new IntPtr(3), 0);
            var y = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0);
            Assert.AreEqual(2, _scoreComparer.Compare(x, y));
        }

        [TestMethod]
        public void TestCompare_Larger_Value()
        {
            var x = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0.1);
            var y = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0);
            Assert.AreEqual(1, _scoreComparer.Compare(x, y));
        }

        [TestMethod]
        public void TestCompare_Larger_Value_Delta()
        {
            var x = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0.00009);
            var y = new KeyValuePair<IntPtr, double>(new IntPtr(1), 0);
            Assert.AreEqual(1, _scoreComparer.Compare(x, y));
        }
    }
}
