using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender;

namespace WindowRecommenderTests
{
    [TestClass]
    public class WindowRecordTest
    {
        [TestMethod]
        public void TestEqual()
        {
            Assert.AreEqual(new WindowRecord(new IntPtr(1)), new WindowRecord(new IntPtr(1)));
            Assert.AreEqual(new WindowRecord(new IntPtr(1), "a", "a"), new WindowRecord(new IntPtr(1), "b", "b"));
            Assert.AreNotEqual(new WindowRecord(new IntPtr(1)), new WindowRecord(new IntPtr(2)));
        }

        [TestMethod]
        public void TestEqualObj()
        {
            Assert.AreEqual(new WindowRecord(new IntPtr(1)), (object)new WindowRecord(new IntPtr(1)));
            Assert.AreEqual(new WindowRecord(new IntPtr(1), "a", "a"), (object)new WindowRecord(new IntPtr(1), "b", "b"));

            Assert.IsFalse(new WindowRecord(new IntPtr(1)).Equals(null));
            Assert.AreNotEqual(new WindowRecord(new IntPtr(1)), "aaa");
            Assert.AreNotEqual(new WindowRecord(new IntPtr(1)), (object)new WindowRecord(new IntPtr(2)));
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestEqEq()
        {
            Assert.IsTrue(new WindowRecord(new IntPtr(1)) == new WindowRecord(new IntPtr(1)));
            Assert.IsTrue(new WindowRecord(new IntPtr(1), "a", "a") == new WindowRecord(new IntPtr(1), "b", "b"));
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void TestNotEq()
        {
            Assert.IsTrue(new WindowRecord(new IntPtr(1)) != new WindowRecord(new IntPtr(2)));
            Assert.IsTrue(new WindowRecord(new IntPtr(1), "a", "a") != new WindowRecord(new IntPtr(2), "a", "a"));
        }

        [TestMethod]
        public void TestHashcode()
        {
            var dictionary = new Dictionary<WindowRecord, bool>
            {
                {new WindowRecord(new IntPtr(1)), true}
            };
            Assert.IsTrue(dictionary.ContainsKey(new WindowRecord(new IntPtr(1))));
            Assert.IsTrue(dictionary.ContainsKey(new WindowRecord(new IntPtr(1), "a", "b")));
        }
    }
}
