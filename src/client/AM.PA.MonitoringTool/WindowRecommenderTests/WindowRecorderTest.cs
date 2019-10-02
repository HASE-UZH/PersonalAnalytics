using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using WindowRecommender;
using WindowRecommender.Data;
using WindowRecommender.Data.Fakes;
using WindowRecommender.Fakes;

namespace WindowRecommenderTests
{
    [TestClass]
    public class WindowRecorderTest
    {
        [TestMethod]
        public void TestSetup()
        {
            using (ShimsContext.Create())
            {
                var called = false;
                ShimQueries.SaveWindowEventsEventNameIEnumerableOfWindowEventRecord = (eventName, entries) =>
                {
                    called = true;
                    Assert.AreEqual(EventName.Initial, eventName);
                    CollectionAssert.AreEqual(new List<WindowEventRecord>
                    {
                        new WindowEventRecord(new IntPtr(1), "test_title1", "test_process1", 0),
                        new WindowEventRecord(new IntPtr(2), "test_title2", "test_process2", 1),
                    }, entries.ToList());
                };
                var windowEvents = new StubIWindowEvents();
                // ReSharper disable once ObjectCreationAsStatement
                new WindowRecorder(windowEvents, new WindowStack(windowEvents));
                windowEvents.SetupEvent(windowEvents, new List<WindowRecord>{
                    new WindowRecord(new IntPtr(1), "test_title1", "test_process1"),
                    new WindowRecord(new IntPtr(2), "test_title2", "test_process2"),
                });
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestOpen()
        {
            using (ShimsContext.Create())
            {
                var called = false;
                ShimQueries.SaveWindowEventEventNameWindowEventRecord = (eventName, entry) =>
                {
                    called = true;
                    Assert.AreEqual(EventName.Open, eventName);
                    Assert.AreEqual("1", entry.WindowHandle);
                    Assert.AreEqual("test_process", entry.ProcessName);
                    Assert.AreEqual("test_title", entry.WindowTitle);
                    Assert.AreEqual(0, entry.ZIndex);
                    Assert.AreEqual(-1, entry.Rank);
                    Assert.AreEqual(-1, entry.Score);
                };
                var windowEvents = new StubIWindowEvents();
                // ReSharper disable once ObjectCreationAsStatement
                new WindowRecorder(windowEvents, new WindowStack(windowEvents));
                var windowRecord = new WindowRecord(new IntPtr(1), "test_title", "test_process");
                windowEvents.WindowOpenedOrFocusedEvent(windowEvents, windowRecord);
                windowEvents.WindowOpenedEvent(windowEvents, windowRecord);
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestFocus()
        {
            using (ShimsContext.Create())
            {
                var called = false;
                ShimQueries.SaveWindowEventEventNameWindowEventRecord = (eventName, entry) =>
                {
                    called = true;
                    Assert.AreEqual(EventName.Focus, eventName);
                    Assert.AreEqual("2", entry.WindowHandle);
                    Assert.AreEqual("test_process", entry.ProcessName);
                    Assert.AreEqual("test_title", entry.WindowTitle);
                    Assert.AreEqual(1, entry.Rank);
                    Assert.AreEqual(0.8, entry.Score);
                    Assert.AreEqual(1, entry.ZIndex);
                };
                var windowEvents = new StubIWindowEvents();
                var windowStack = new ShimWindowStack
                {
                    GetZIndexWindowRecord = windowRecord => 1
                };
                var wr = new WindowRecorder(windowEvents, windowStack);
                wr.SetScores(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), 1},
                    {new IntPtr(2), 0.8},
                    {new IntPtr(3), 0.7}
                });
                wr.SetTopWindows(new List<IntPtr> { new IntPtr(1), new IntPtr(2), new IntPtr(3) });
                windowEvents.WindowFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2), "test_title", "test_process"));
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestFocus_Open()
        {
            using (ShimsContext.Create())
            {
                var called = false;
                ShimQueries.SaveWindowEventEventNameWindowEventRecord = (eventName, entry) =>
                {
                    called = true;
                    Assert.AreEqual(EventName.Open, eventName);
                    Assert.AreEqual("1", entry.WindowHandle);
                    Assert.AreEqual("test_process", entry.ProcessName);
                    Assert.AreEqual("test_title", entry.WindowTitle);
                    Assert.AreEqual(0, entry.ZIndex);
                    Assert.AreEqual(-1, entry.Rank);
                    Assert.AreEqual(-1, entry.Score);
                };
                var windowEvents = new StubIWindowEvents();
                // ReSharper disable once ObjectCreationAsStatement
                new WindowRecorder(windowEvents, new WindowStack(windowEvents));
                var windowRecord = new WindowRecord(new IntPtr(1), "test_title", "test_process");
                windowEvents.WindowOpenedOrFocusedEvent(windowEvents, windowRecord);
                windowEvents.WindowFocusedEvent(windowEvents, windowRecord);
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestClose()
        {
            using (ShimsContext.Create())
            {
                var called = false;
                ShimQueries.SaveWindowEventEventNameWindowEventRecord = (eventName, entry) =>
                {
                    called = true;
                    Assert.AreEqual(EventName.Close, eventName);
                    Assert.AreEqual("2", entry.WindowHandle);
                    Assert.AreEqual("test_process", entry.ProcessName);
                    Assert.AreEqual("test_title", entry.WindowTitle);
                    Assert.AreEqual(1, entry.Rank);
                    Assert.AreEqual(0.8, entry.Score);
                    Assert.AreEqual(-1, entry.ZIndex);
                };
                var windowEvents = new StubIWindowEvents();
                var wr = new WindowRecorder(windowEvents, new WindowStack(windowEvents));
                wr.SetScores(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), 1},
                    {new IntPtr(2), 0.8},
                    {new IntPtr(3), 0.7}
                });
                wr.SetTopWindows(new List<IntPtr> { new IntPtr(1), new IntPtr(2), new IntPtr(3) });

                windowEvents.WindowClosedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4), "test_title", "test_process"));
                Assert.IsFalse(called);

                windowEvents.WindowClosedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2), "test_title", "test_process"));
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestMinimize()
        {
            using (ShimsContext.Create())
            {
                var called = false;
                ShimQueries.SaveWindowEventEventNameWindowEventRecord = (eventName, entry) =>
                {
                    called = true;
                    Assert.AreEqual(EventName.Minimize, eventName);
                    Assert.AreEqual("2", entry.WindowHandle);
                    Assert.AreEqual("test_process", entry.ProcessName);
                    Assert.AreEqual("test_title", entry.WindowTitle);
                    Assert.AreEqual(1, entry.Rank);
                    Assert.AreEqual(0.8, entry.Score);
                    Assert.AreEqual(-1, entry.ZIndex);
                };
                var windowEvents = new StubIWindowEvents();
                var wr = new WindowRecorder(windowEvents, new WindowStack(windowEvents));
                wr.SetScores(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), 1},
                    {new IntPtr(2), 0.8},
                    {new IntPtr(3), 0.7}
                });
                wr.SetTopWindows(new List<IntPtr> { new IntPtr(1), new IntPtr(2), new IntPtr(3) });

                windowEvents.WindowMinimizedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4), "test_title", "test_process"));
                Assert.IsFalse(called);

                windowEvents.WindowMinimizedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2), "test_title", "test_process"));
                Assert.IsTrue(called);
            }
        }
    }
}
