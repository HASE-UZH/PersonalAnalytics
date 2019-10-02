using System;
using System.Collections.Generic;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender;
using WindowRecommender.Fakes;
using WindowRecommender.Models;
using WindowRecommenderTests.Tools;

namespace WindowRecommenderTests.Models
{
    [TestClass]
    public class DurationTest
    {
        /// <summary>
        /// Step set to a value that gives nice results on division by 60 in most cases
        /// </summary>
        private const int IntervalStep = 3;

        /// <summary>
        /// Starting timestamp for test
        /// </summary>
        private const string Timestamp = "2002-02-02T14:14:14.0000000Z";

        [TestMethod]
        public void IntervalSeconds()
        {
            // Dummy test to assure an interval is larger than two steps
            Assert.IsTrue(Settings.DurationIntervalSeconds > IntervalStep * 2);
        }

        [TestMethod]
        public void TestEmpty()
        {
            var windowEvents = new StubIWindowEvents();
            var duration = new Duration(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>());
            Assert.AreEqual(0, duration.GetScores().Count);
        }

        [TestMethod]
        public void TestOnInterval_Setup()
        {
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var intervalTime = setupTime.AddSeconds(Settings.DurationIntervalSeconds);

                var called = false;
                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);
                duration.ScoreChanged += (sender, args) => called = true;
                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>(), duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                System.Fakes.ShimDateTime.NowGet = () => intervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>
                {
                    { new IntPtr(1), Settings.DurationIntervalSeconds / (Settings.DurationTimeframeMinutes * 60D) }
                }, duration.GetScores());
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestOnInterval_Setup_Open()
        {
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var openEventTime = setupTime.AddSeconds(IntervalStep);
                var intervalTime = setupTime.AddSeconds(Settings.DurationIntervalSeconds);

                var called = false;
                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);
                duration.ScoreChanged += (sender, args) => called = true;
                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>(), duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                System.Fakes.ShimDateTime.NowGet = () => openEventTime;
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));

                System.Fakes.ShimDateTime.NowGet = () => intervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>
                {
                    { new IntPtr(1), IntervalStep / (Settings.DurationTimeframeMinutes * 60D) },
                    { new IntPtr(2), (Settings.DurationIntervalSeconds - IntervalStep) / (Settings.DurationTimeframeMinutes * 60D) }
                }, duration.GetScores());
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestOnInterval_Setup_Focus()
        {
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var focusEventTime = setupTime.AddSeconds(IntervalStep);
                var intervalTime = setupTime.AddSeconds(Settings.DurationIntervalSeconds);

                var called = false;
                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);
                duration.ScoreChanged += (sender, args) => called = true;
                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>(), duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                System.Fakes.ShimDateTime.NowGet = () => focusEventTime;
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));

                System.Fakes.ShimDateTime.NowGet = () => intervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>
                {
                    { new IntPtr(1), IntervalStep / (Settings.DurationTimeframeMinutes * 60D) },
                    { new IntPtr(2), (Settings.DurationIntervalSeconds - IntervalStep) / (Settings.DurationTimeframeMinutes * 60D) }
                }, duration.GetScores());
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestOnInterval_Setup_Focus_Focus()
        {
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var firstFocusEventTime = setupTime.AddSeconds(IntervalStep);
                var secondFocusEventTime = firstFocusEventTime.AddSeconds(IntervalStep);
                var intervalTime = setupTime.AddSeconds(Settings.DurationIntervalSeconds);

                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);
                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>(), duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                System.Fakes.ShimDateTime.NowGet = () => firstFocusEventTime;
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));

                System.Fakes.ShimDateTime.NowGet = () => secondFocusEventTime;
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(1)));

                System.Fakes.ShimDateTime.NowGet = () => intervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), (Settings.DurationIntervalSeconds - IntervalStep) / (Settings.DurationTimeframeMinutes * 60D)},
                    {new IntPtr(2), IntervalStep / (Settings.DurationTimeframeMinutes * 60D)},
                }, duration.GetScores(), new ScoreComparer());
            }
        }

        [TestMethod]
        public void TestOnInterval_Setup_MultipleInterval()
        {
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var firstIntervalTime = setupTime.AddSeconds(Settings.DurationIntervalSeconds);
                var secondIntervalTime = firstIntervalTime.AddSeconds(Settings.DurationIntervalSeconds);

                var called = false;
                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);
                duration.ScoreChanged += (sender, args) => called = true;
                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>(), duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                System.Fakes.ShimDateTime.NowGet = () => firstIntervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>
                {
                    { new IntPtr(1), Settings.DurationIntervalSeconds / (Settings.DurationTimeframeMinutes * 60D) }
                }, duration.GetScores());
                Assert.IsTrue(called);

                called = false;
                System.Fakes.ShimDateTime.NowGet = () => secondIntervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>
                {
                    { new IntPtr(1), Settings.DurationIntervalSeconds * 2 / (Settings.DurationTimeframeMinutes * 60D) }
                }, duration.GetScores());
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestOnInterval_Setup_Focus_MultipleInterval()
        {
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var focusTime = setupTime.AddSeconds(Settings.DurationIntervalSeconds - IntervalStep);
                var firstIntervalTime = setupTime.AddSeconds(Settings.DurationIntervalSeconds);
                var secondIntervalTime = firstIntervalTime.AddSeconds(Settings.DurationIntervalSeconds);

                var called = false;
                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);
                duration.ScoreChanged += (sender, args) => called = true;
                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>(), duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                System.Fakes.ShimDateTime.NowGet = () => focusTime;
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));

                System.Fakes.ShimDateTime.NowGet = () => firstIntervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), (Settings.DurationIntervalSeconds - IntervalStep) / (Settings.DurationTimeframeMinutes * 60D)},
                    {new IntPtr(2), IntervalStep / (Settings.DurationTimeframeMinutes * 60D)},
                }, duration.GetScores(), new ScoreComparer());
                Assert.IsTrue(called);
                called = false;

                System.Fakes.ShimDateTime.NowGet = () => secondIntervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), (Settings.DurationIntervalSeconds - IntervalStep) / (Settings.DurationTimeframeMinutes * 60D)},
                    {new IntPtr(2), (Settings.DurationIntervalSeconds + IntervalStep) / (Settings.DurationTimeframeMinutes * 60D)},
                }, duration.GetScores(), new ScoreComparer());
                Assert.IsTrue(called);
            }
        }

        [TestMethod]
        public void TestOnInterval_Remove()
        {
            // Start with window 1, focus on 2 within one interval
            // Continue until window 1 leaves the timeframe
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var focusTime = setupTime.AddSeconds(IntervalStep);
                var intervalTime = setupTime.AddSeconds(Settings.DurationIntervalSeconds);
                var durationTime = intervalTime.AddMinutes(Settings.DurationTimeframeMinutes);

                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);
                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>(), duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                System.Fakes.ShimDateTime.NowGet = () => focusTime;
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));

                System.Fakes.ShimDateTime.NowGet = () => intervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>
                {
                    { new IntPtr(1), IntervalStep / (Settings.DurationTimeframeMinutes * 60D) },
                    { new IntPtr(2), (Settings.DurationIntervalSeconds - IntervalStep) / (Settings.DurationTimeframeMinutes * 60D) }
                }, duration.GetScores());

                var nextIntervalTime = intervalTime.AddSeconds(Settings.DurationIntervalSeconds);
                while (nextIntervalTime < durationTime)
                {
                    var thisIntervalTime = nextIntervalTime;
                    System.Fakes.ShimDateTime.NowGet = () => thisIntervalTime;
                    duration.OnInterval(null, null);
                    nextIntervalTime = nextIntervalTime.AddSeconds(Settings.DurationIntervalSeconds);
                }

                System.Fakes.ShimDateTime.NowGet = () => durationTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(2), 1}
                }, duration.GetScores(), new ScoreComparer());
            }
        }

        [TestMethod]
        public void TestOnInterval_Timeframe()
        {
            // Start with window 1, continue until score doesn't change
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var intervalTime = setupTime;
                var durationTime = setupTime.AddMinutes(Settings.DurationTimeframeMinutes);

                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                while (intervalTime < durationTime)
                {
                    intervalTime = intervalTime.AddSeconds(Settings.DurationIntervalSeconds);
                    var thisIntervalTime = intervalTime;
                    System.Fakes.ShimDateTime.NowGet = () => thisIntervalTime;
                    duration.OnInterval(null, null);
                }

                Assert.AreEqual(intervalTime, durationTime);
                CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), 1}
                }, duration.GetScores(), new ScoreComparer());

                intervalTime = intervalTime.AddSeconds(Settings.DurationIntervalSeconds);
                //duration.ScoreChanged += (sender, e) => Assert.Fail();
                System.Fakes.ShimDateTime.NowGet = () => intervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), 1}
                }, duration.GetScores(), new ScoreComparer());
            }
        }

        [TestMethod]
        public void TestOnInterval_Closed()
        {
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var focusEventTime = setupTime.AddSeconds(IntervalStep);
                var interval1Time = setupTime.AddSeconds(Settings.DurationIntervalSeconds);
                var closedEventTime = interval1Time.AddSeconds(IntervalStep);
                var interval2Time = interval1Time.AddSeconds(Settings.DurationIntervalSeconds);

                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);
                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>(), duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                System.Fakes.ShimDateTime.NowGet = () => focusEventTime;
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));

                System.Fakes.ShimDateTime.NowGet = () => interval1Time;
                duration.OnInterval(null, null);

                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>
                {
                    { new IntPtr(1), IntervalStep / (Settings.DurationTimeframeMinutes * 60D) },
                    { new IntPtr(2), (Settings.DurationIntervalSeconds - IntervalStep) / (Settings.DurationTimeframeMinutes * 60D) }
                }, duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => closedEventTime;
                windowEvents.WindowClosedOrMinimizedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(1)));

                System.Fakes.ShimDateTime.NowGet = () => interval2Time;
                duration.OnInterval(null, null);

                CollectionAssert.AreEquivalent(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), Settings.DurationIntervalSeconds / (Settings.DurationTimeframeMinutes * 60D)}
                }, duration.GetScores());
            }
        }

        [TestMethod]
        public void TestOnInterval_New_Closed()
        {
            using (ShimsContext.Create())
            {
                var setupTime = DateTime.Parse(Timestamp);
                var focusEventTime = setupTime.AddSeconds(IntervalStep);
                var closedEventTime = focusEventTime.AddSeconds(IntervalStep);
                var intervalTime = setupTime.AddSeconds(Settings.DurationIntervalSeconds);

                var windowEvents = new StubIWindowEvents();
                var duration = new Duration(windowEvents);
                CollectionAssert.AreEqual(new Dictionary<IntPtr, double>(), duration.GetScores());

                System.Fakes.ShimDateTime.NowGet = () => setupTime;
                windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1))
                });

                System.Fakes.ShimDateTime.NowGet = () => focusEventTime;
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));

                System.Fakes.ShimDateTime.NowGet = () => closedEventTime;
                windowEvents.WindowClosedOrMinimizedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2)));
                windowEvents.WindowOpenedOrFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(1)));

                System.Fakes.ShimDateTime.NowGet = () => intervalTime;
                duration.OnInterval(null, null);

                CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), (Settings.DurationIntervalSeconds - IntervalStep) / (Settings.DurationTimeframeMinutes * 60D)}
                }, duration.GetScores(), new ScoreComparer());
            }
        }
    }
}
