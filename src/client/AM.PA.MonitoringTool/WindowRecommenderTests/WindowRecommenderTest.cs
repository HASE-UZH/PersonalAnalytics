using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using WindowRecommender;

namespace WindowRecommenderTests
{
    [TestClass]
    public class WindowRecommenderTest
    {
        [TestMethod]
        public void TestGetScoredWindows_BothEmpty()
        {
            var topWindows = new List<IntPtr>();
            var windowStack = new List<WindowRecord>();
            var expectedScoredWindows = new List<(WindowRecord windowRecord, bool show)>();
            var actualScoredWindows = WindowRecommender.WindowRecommender.GetScoredWindows(topWindows, windowStack).ToList();
            CollectionAssert.AreEqual(expectedScoredWindows, actualScoredWindows);
        }

        [TestMethod]
        public void TestGetScoredWindows_ScoresEmpty()
        {
            var topWindows = new List<IntPtr>();
            var windowStack = new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
                new WindowRecord(new IntPtr(2)),
            };
            var expectedScoredWindows = new List<(WindowRecord windowRecord, bool show)>
            {
                (windowRecord: new WindowRecord(new IntPtr(1)), show: true),
            };
            var actualScoredWindows = WindowRecommender.WindowRecommender.GetScoredWindows(topWindows, windowStack).ToList();
            CollectionAssert.AreEqual(expectedScoredWindows, actualScoredWindows);
        }

        [TestMethod]
        public void TestGetScoredWindows_StackEmpty()
        {
            var topWindows = new List<IntPtr>
            {
                new IntPtr(1),
                new IntPtr(2),
            };
            var windowStack = new List<WindowRecord>();
            var expectedScoredWindows = new List<(WindowRecord windowRecord, bool show)>();
            var actualScoredWindows = WindowRecommender.WindowRecommender.GetScoredWindows(topWindows, windowStack).ToList();
            CollectionAssert.AreEqual(expectedScoredWindows, actualScoredWindows);
        }

        [TestMethod]
        public void TestGetScoredWindows()
        {
            var topWindows = new List<IntPtr>
            {
                new IntPtr(1),
                new IntPtr(2),
            };
            var windowStack = new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
                new WindowRecord(new IntPtr(2)),
            };
            var expectedScoredWindows = new List<(WindowRecord windowRecord, bool show)>
            {
                (windowRecord: new WindowRecord(new IntPtr(1)), show: true),
                (windowRecord: new WindowRecord(new IntPtr(2)), show: true),
            };
            var actualScoredWindows = WindowRecommender.WindowRecommender.GetScoredWindows(topWindows, windowStack).ToList();
            CollectionAssert.AreEqual(expectedScoredWindows, actualScoredWindows);
        }

        [TestMethod]
        public void TestGetScoredWindows_Hidden()
        {
            var topWindows = new List<IntPtr>
            {
                new IntPtr(3),
            };
            var windowStack = new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
                new WindowRecord(new IntPtr(2)),
                new WindowRecord(new IntPtr(3)),
            };
            var expectedScoredWindows = new List<(WindowRecord windowRecord, bool show)>
            {
                (windowRecord: new WindowRecord(new IntPtr(1)), show: true),
                (windowRecord: new WindowRecord(new IntPtr(2)), show: false),
                (windowRecord: new WindowRecord(new IntPtr(3)), show: true),
            };
            var actualScoredWindows = WindowRecommender.WindowRecommender.GetScoredWindows(topWindows, windowStack).ToList();
            CollectionAssert.AreEqual(expectedScoredWindows, actualScoredWindows);
        }

        [TestMethod]
        public void TestGetScoredWindows_NumberOfWindows()
        {
            // Check Settings as test depends on value of 3
            Assert.AreEqual(3, Settings.NumberOfWindows);

            var topWindows = new List<IntPtr>
            {
                new IntPtr(1),
                new IntPtr(2),
                new IntPtr(3),
            };
            var windowStack = new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
                new WindowRecord(new IntPtr(2)),
                new WindowRecord(new IntPtr(3)),
                new WindowRecord(new IntPtr(4)),
                new WindowRecord(new IntPtr(5)),
            };
            var expectedScoredWindows = new List<(WindowRecord windowRecord, bool show)>
            {
                (windowRecord: new WindowRecord(new IntPtr(1)), show: true),
                (windowRecord: new WindowRecord(new IntPtr(2)), show: true),
                (windowRecord: new WindowRecord(new IntPtr(3)), show: true),
            };
            var actualScoredWindows = WindowRecommender.WindowRecommender.GetScoredWindows(topWindows, windowStack).ToList();
            CollectionAssert.AreEqual(expectedScoredWindows, actualScoredWindows);
        }

        [TestMethod]
        public void TestGetScoredWindows_NumberOfWindows_WithoutForeground()
        {
            // Check Settings as test depends on value of 3
            Assert.AreEqual(3, Settings.NumberOfWindows);

            var topWindows = new List<IntPtr>
            {
                new IntPtr(4),
                new IntPtr(3),
                new IntPtr(2),
            };
            var windowStack = new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
                new WindowRecord(new IntPtr(2)),
                new WindowRecord(new IntPtr(3)),
                new WindowRecord(new IntPtr(4)),
                new WindowRecord(new IntPtr(5)),
            };
            var expectedScoredWindows = new List<(WindowRecord windowRecord, bool show)>
            {
                (windowRecord: new WindowRecord(new IntPtr(1)), show: true),
                (windowRecord: new WindowRecord(new IntPtr(2)), show: false),
                (windowRecord: new WindowRecord(new IntPtr(3)), show: true),
                (windowRecord: new WindowRecord(new IntPtr(4)), show: true),
            };
            var actualScoredWindows = WindowRecommender.WindowRecommender.GetScoredWindows(topWindows, windowStack).ToList();
            CollectionAssert.AreEqual(expectedScoredWindows, actualScoredWindows);
        }
    }
}
