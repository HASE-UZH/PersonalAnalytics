using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WindowRecommender;
using WindowRecommender.Fakes;

namespace WindowRecommenderTests
{
    [TestClass]
    public class WindowStackTest
    {
        [TestMethod]
        public void TestGet_Set()
        {
            var windowEvents = new StubIWindowEvents();
            var windowStack = new WindowStack(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1))
            });
            CollectionAssert.AreEqual(new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
            }, windowStack.WindowRecords);
        }

        [TestMethod]
        public void TestGet_OpenOrFocus()
        {
            var windowEvents = new StubIWindowEvents();
            var windowStack = new WindowStack(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
                new WindowRecord(new IntPtr(2)),
            });
            windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(3)));
            CollectionAssert.AreEqual(new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(3)),
                new WindowRecord(new IntPtr(1)),
                new WindowRecord(new IntPtr(2)),
            }, windowStack.WindowRecords);
        }

        [TestMethod]
        public void TestGet_CloseOrMinimize()
        {
            var windowEvents = new StubIWindowEvents();
            var windowStack = new WindowStack(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
                new WindowRecord(new IntPtr(2)),
            });
            windowEvents.WindowClosedOrMinimizedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(1)));
            CollectionAssert.AreEqual(new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(2)),
            }, windowStack.WindowRecords);
        }

        [TestMethod]
        public void TestGetZIndex()
        {
            var windowEvents = new StubIWindowEvents();
            var windowStack = new WindowStack(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1)),
                new WindowRecord(new IntPtr(2)),
            });
            Assert.AreEqual(1, windowStack.GetZIndex(new WindowRecord(new IntPtr(2))));
        }
    }
}
