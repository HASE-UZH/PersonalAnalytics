using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender;
using WindowRecommender.Fakes;
using WindowRecommender.Models;
using WindowRecommenderTests.Tools;

namespace WindowRecommenderTests.Models
{
    [TestClass]
    public class TitleSimilarityTest
    {
        [TestMethod]
        public void TestEmpty()
        {
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>());
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>(), titleSimilarity.GetScores());
        }

        [TestMethod]
        public void TestScore_EmptyTitles()
        {
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "", ""),
                new WindowRecord(new IntPtr(2), "", ""),
            });
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>(), titleSimilarity.GetScores());
        }

        [TestMethod]
        public void TestScore_EqualTitles()
        {
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "Very nice title", ""),
                new WindowRecord(new IntPtr(2), "Very nice title", ""),
            });
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(2), 1},
            }, titleSimilarity.GetScores(), new ScoreComparer());
        }

        [TestMethod]
        public void TestScore_NoOverlap()
        {
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "Very nice title", ""),
                new WindowRecord(new IntPtr(2), "Not good text", ""),
            });
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>(), titleSimilarity.GetScores());
        }

        [TestMethod]
        public void TestScore_Subset()
        {
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "Very nice title", ""),
                new WindowRecord(new IntPtr(2), "Good title", ""),
                new WindowRecord(new IntPtr(3), "Nice title", ""),
            });
            var scores = titleSimilarity.GetScores();
            Assert.IsTrue(scores[new IntPtr(3)] > scores[new IntPtr(2)]);
        }

        [TestMethod]
        public void TestScore_DocumentLength()
        {
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "Very nice title", ""),
                new WindowRecord(new IntPtr(2), "Really good title", ""),
                new WindowRecord(new IntPtr(3), "Good title", ""),
            });
            var scores = titleSimilarity.GetScores();
            Assert.IsTrue(scores[new IntPtr(3)] > scores[new IntPtr(2)]);
        }

        [TestMethod]
        public void TestOnWindowClosedOrMinimized()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "Very nice title", ""),
                new WindowRecord(new IntPtr(2), "Very nice title", ""),
            });
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(2), 1},
            }, titleSimilarity.GetScores(), new ScoreComparer());

            windowEvents.WindowClosedOrMinimizedEvent(windowEvents, new WindowRecord(new IntPtr(2), "Very nice title", ""));
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>(), titleSimilarity.GetScores());
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestOnWindowClosedOrMinimized_NoChange()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "Very nice title", ""),
                new WindowRecord(new IntPtr(2), "Very nice title", ""),
                new WindowRecord(new IntPtr(3), "Some other text", ""),
            });
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(2), 1},
            }, titleSimilarity.GetScores(), new ScoreComparer());

            windowEvents.WindowClosedOrMinimizedEvent(windowEvents, new WindowRecord(new IntPtr(3), "Some other text", ""));
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                {new IntPtr(2), 1},
            }, titleSimilarity.GetScores(), new ScoreComparer());
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void TestOnWindowFocused()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "one two three", ""),
                new WindowRecord(new IntPtr(2), "two three four", ""),
                new WindowRecord(new IntPtr(3), "three four five", ""),
                new WindowRecord(new IntPtr(4), "four five six", ""),
            });
            var scores = titleSimilarity.GetScores();
            Assert.AreEqual(2, scores.Count);
            Assert.IsTrue(scores[new IntPtr(2)] > scores[new IntPtr(3)]);
            Assert.IsFalse(called);

            windowEvents.WindowFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4), "four five six", ""));
            scores = titleSimilarity.GetScores();
            Assert.AreEqual(2, scores.Count);
            Assert.IsTrue(scores[new IntPtr(3)] > scores[new IntPtr(2)]);
            Assert.IsTrue(called);
        }


        [TestMethod]
        public void TestOnWindowFocused_NoChange()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "one two three", ""),
                new WindowRecord(new IntPtr(2), "one two four five", ""),
                new WindowRecord(new IntPtr(3), "three six", ""),
                new WindowRecord(new IntPtr(4), "four five six", ""),
            });
            var scores = titleSimilarity.GetScores();
            Assert.AreEqual(2, scores.Count);
            Assert.IsTrue(scores[new IntPtr(2)] > scores[new IntPtr(3)]);
            Assert.IsFalse(called);

            windowEvents.WindowFocusedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4), "four five six", ""));
            scores = titleSimilarity.GetScores();
            Assert.AreEqual(2, scores.Count);
            Assert.IsTrue(scores[new IntPtr(2)] > scores[new IntPtr(3)]);
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void TestOnWindowOpened()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "one two three", ""),
                new WindowRecord(new IntPtr(2), "two three four", ""),
                new WindowRecord(new IntPtr(3), "three four five", ""),
            });
            var scores = titleSimilarity.GetScores();
            Assert.AreEqual(2, scores.Count);
            Assert.IsTrue(scores[new IntPtr(2)] > scores[new IntPtr(3)]);
            Assert.IsFalse(called);

            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4), "four five six", ""));
            scores = titleSimilarity.GetScores();
            Assert.AreEqual(2, scores.Count);
            Assert.IsTrue(scores[new IntPtr(3)] > scores[new IntPtr(2)]);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestOnWindowOpened_EmptyTitle()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "one two three", ""),
                new WindowRecord(new IntPtr(2), "two three four", ""),
                new WindowRecord(new IntPtr(3), "three four five", ""),
            });
            var scores = titleSimilarity.GetScores();
            Assert.AreEqual(2, scores.Count);
            Assert.IsTrue(scores[new IntPtr(2)] > scores[new IntPtr(3)]);
            Assert.IsFalse(called);

            windowEvents.WindowOpenedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(4), "", ""));
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>(), titleSimilarity.GetScores());
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestOnWindowRenamed_SameTitle()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "one two three", ""),
                new WindowRecord(new IntPtr(2), "two three four", ""),
                new WindowRecord(new IntPtr(3), "three four five", ""),
            });
            var initialScores = titleSimilarity.GetScores();
            Assert.AreEqual(2, initialScores.Count);
            Assert.IsTrue(initialScores[new IntPtr(2)] > initialScores[new IntPtr(3)]);
            Assert.IsFalse(called);

            windowEvents.WindowRenamedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2), "two three four", ""));
            var renamedScores = titleSimilarity.GetScores();
            Assert.AreEqual(2, renamedScores.Count);
            Assert.IsTrue(renamedScores[new IntPtr(2)] > renamedScores[new IntPtr(3)]);
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void TestOnWindowRenamed_Changed()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "one two three four", ""),
                new WindowRecord(new IntPtr(2), "two three four five", ""),
                new WindowRecord(new IntPtr(3), "three four five six", ""),
            });
            var initialScores = titleSimilarity.GetScores();
            Assert.AreEqual(2, initialScores.Count);
            Assert.IsTrue(initialScores[new IntPtr(2)] > initialScores[new IntPtr(3)]);
            Assert.IsFalse(called);

            windowEvents.WindowRenamedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2), "four five six seven", ""));
            var renamedScores = titleSimilarity.GetScores();
            Assert.AreEqual(2, renamedScores.Count);
            Assert.IsTrue(renamedScores[new IntPtr(3)] > renamedScores[new IntPtr(2)]);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestOnWindowRenamed_Empty()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "one two three four", ""),
                new WindowRecord(new IntPtr(2), "two three four five", ""),
                new WindowRecord(new IntPtr(3), "three four five six", ""),
            });
            var initialScores = titleSimilarity.GetScores();
            Assert.AreEqual(2, initialScores.Count);
            Assert.IsTrue(initialScores[new IntPtr(2)] > initialScores[new IntPtr(3)]);
            Assert.IsFalse(called);

            windowEvents.WindowRenamedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2), "", ""));
            var renamedScores = titleSimilarity.GetScores();
            Assert.AreEqual(1, renamedScores.Count);
            Assert.IsTrue(renamedScores.ContainsKey(new IntPtr(3)));
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestOnWindowRenamed_New()
        {
            var called = false;
            var windowEvents = new StubIWindowEvents();
            var titleSimilarity = new TitleSimilarity(windowEvents);
            titleSimilarity.ScoreChanged += (sender, args) => called = true;
            windowEvents.SetupEvent.Invoke(windowEvents, new List<WindowRecord>
            {
                new WindowRecord(new IntPtr(1), "one two three four", ""),
                new WindowRecord(new IntPtr(3), "three four five six", ""),
            });
            var initialScores = titleSimilarity.GetScores();
            Assert.AreEqual(1, initialScores.Count);
            Assert.IsTrue(initialScores.ContainsKey(new IntPtr(3)));
            Assert.IsFalse(called);

            windowEvents.WindowRenamedEvent.Invoke(windowEvents, new WindowRecord(new IntPtr(2), "two three four five", ""));
            var renamedScores = titleSimilarity.GetScores();
            Assert.AreEqual(2, renamedScores.Count);
            Assert.IsTrue(renamedScores[new IntPtr(2)] > initialScores[new IntPtr(3)]);
            Assert.IsTrue(called);
        }
    }
}
