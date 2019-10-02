using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender;
using WindowRecommender.Fakes;
using WindowRecommender.Models;

namespace WindowRecommenderTests.Models
{
    [TestClass]
    public class MostRecentlyActiveTest
    {
        [TestMethod]
        public void TestEmpty()
        {
            var windowEvents = new StubIWindowEvents();
            var mra = new MostRecentlyActive(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>());
            Assert.AreEqual(0, mra.GetScores().Count);
        }

        [TestMethod]
        public void TestDefault()
        {
            // Add Settings.NumberOfWindows plus one
            var windowEvents = new StubIWindowEvents();
            var mra = new MostRecentlyActive(windowEvents);
            var windowList = new List<WindowRecord>();
            for (var i = 1; i < Settings.NumberOfWindows + 2; i++)
            {
                windowList.Add(new WindowRecord(new IntPtr(i)));
            }
            windowEvents.SetupEvent.Invoke(windowEvents, windowList);

            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(1), 1 },
            }, mra.GetScores());
        }

        [TestMethod]
        public void TestOpenedEvent()
        {
            // Check configured number of windows as test relies on value
            Assert.AreEqual(3, Settings.NumberOfWindows);

            var windowEvents = new StubIWindowEvents();
            var mra = new MostRecentlyActive(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
            });

            var changed = false;
            mra.ScoreChanged += (sender, args) => changed = true;

            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));
            Assert.IsTrue(changed);
            changed = false;
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(1), 1 },
                {new IntPtr(2), 1 },
            }, mra.GetScores());

            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(3)));
            Assert.IsTrue(changed);
            changed = false;
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(1), 1 },
                {new IntPtr(2), 1 },
                {new IntPtr(3), 1 },
            }, mra.GetScores());

            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4)));
            Assert.IsTrue(changed);
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(2), 1 },
                {new IntPtr(3), 1 },
                {new IntPtr(4), 1 },
            }, mra.GetScores());
        }


        [TestMethod]
        public void TestFocusedEvent_new()
        {
            // Check configured number of windows as test relies on value
            Assert.AreEqual(3, Settings.NumberOfWindows);

            var windowEvents = new StubIWindowEvents();
            var mra = new MostRecentlyActive(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
            });

            var changed = false;
            mra.ScoreChanged += (sender, args) => changed = true;

            windowEvents.WindowFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));
            Assert.IsTrue(changed);
            changed = false;
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(1), 1 },
                {new IntPtr(2), 1 },
            }, mra.GetScores());

            windowEvents.WindowFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(3)));
            Assert.IsTrue(changed);
            changed = false;
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(1), 1 },
                {new IntPtr(2), 1 },
                {new IntPtr(3), 1 },
            }, mra.GetScores());

            windowEvents.WindowFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4)));
            Assert.IsTrue(changed);
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(2), 1 },
                {new IntPtr(3), 1 },
                {new IntPtr(4), 1 },
            }, mra.GetScores());
        }

        [TestMethod]
        public void TestFocusedEvent()
        {
            // Check configured number of windows as test relies on value
            Assert.AreEqual(3, Settings.NumberOfWindows);

            var windowEvents = new StubIWindowEvents();
            var mra = new MostRecentlyActive(windowEvents);

            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(1)));
            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));
            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(3)));
            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4)));

            var changed = false;
            mra.ScoreChanged += (sender, args) => changed = true;

            // Focus on second window
            windowEvents.WindowFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(3)));
            Assert.IsFalse(changed);

            // Focus on last window
            windowEvents.WindowFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(1)));
            Assert.IsTrue(changed);

            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(1), 1 },
                {new IntPtr(3), 1 },
                {new IntPtr(4), 1 },
            }, mra.GetScores());
        }

        [TestMethod]
        public void TestClosedEvent()
        {
            // Check configured number of windows as test relies on value
            Assert.AreEqual(3, Settings.NumberOfWindows);

            var windowEvents = new StubIWindowEvents();
            var mra = new MostRecentlyActive(windowEvents);

            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(1)));
            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));
            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(3)));
            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4)));

            var changed = false;
            mra.ScoreChanged += (sender, args) => changed = true;

            // Remove last window
            windowEvents.WindowClosedOrMinimizedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(1)));
            Assert.IsFalse(changed);

            // Remove first window
            windowEvents.WindowClosedOrMinimizedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4)));
            Assert.IsTrue(changed);

            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(2), 1 },
                {new IntPtr(3), 1 },
            }, mra.GetScores());
        }
    }
}
