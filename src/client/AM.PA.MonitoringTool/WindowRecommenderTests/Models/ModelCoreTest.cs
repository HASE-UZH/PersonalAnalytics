using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender.Models;
using WindowRecommender.Models.Fakes;

namespace WindowRecommenderTests.Models
{
    [TestClass]
    public class ModelCoreTest
    {
        [TestMethod]
        public void TestStart()
        {
            var stubModel = new StubIModel
            {
                NameGet = () => "M1",
                GetScores = () => new Dictionary<IntPtr, double> {
                    { new IntPtr(1), 1 },
                }.ToImmutableDictionary()
            };

            var modelCore = new ModelCore(new (IModel, double)[]
            {
                (stubModel, 1)
            });
            modelCore.ScoreChanged += (sender, scores) =>
            {
                CollectionAssert.AreEqual(new Dictionary<string, double>
                {
                    {ModelCore.MergedScoreName, 1},
                    {"M1", 1},
                }, scores[new IntPtr(1)]);
            };
            modelCore.Start();
        }

        [TestMethod]
        public void TestEmptyEvent()
        {
            var stubModel = new StubIModel
            {
                NameGet = () => "M1",
                GetScores = () => new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), 0.1},
                }.ToImmutableDictionary()
            };
            var modelCore = new ModelCore(new (IModel, double)[]
            {
                (stubModel, 1),
            });
            modelCore.Start();
        }

        [TestMethod]
        public void TestScoreCalculation()
        {
            var stubModel = new StubIModel
            {
                NameGet = () => "M1",
                GetScores = () => new Dictionary<IntPtr, double>
                    {
                        {new IntPtr(1), 1},
                        {new IntPtr(2), 1},
                        {new IntPtr(3), 0},
                        {new IntPtr(4), 0}
                    }.ToImmutableDictionary()
            };

            var stubModel2 = new StubIModel
            {
                NameGet = () => "M2",
                GetScores = () => new Dictionary<IntPtr, double>
                    {
                        {new IntPtr(1), 4},
                        {new IntPtr(2), 0},
                        {new IntPtr(3), 1}
                    }.ToImmutableDictionary()
            };

            var modelCore = new ModelCore(new (IModel, double)[]
            {
                    (stubModel, 1),
                    (stubModel2, 2),
            });
            modelCore.ScoreChanged += (sender, scores) =>
            {
                CollectionAssert.AreEqual(new Dictionary<string, double>
                {
                    {ModelCore.MergedScoreName, 0.7},
                    {"M1", 0.5},
                    {"M2", 0.8},
                }, scores[new IntPtr(1)]);
                CollectionAssert.AreEqual(new Dictionary<string, double>
                {
                    {ModelCore.MergedScoreName, 0.5 / 3},
                    {"M1", 0.5},
                    {"M2", 0},
                }, scores[new IntPtr(2)]);
                CollectionAssert.AreEqual(new Dictionary<string, double>
                {
                    {ModelCore.MergedScoreName, 0.4 / 3},
                    {"M1", 0},
                    {"M2", 0.2},
                }, scores[new IntPtr(3)]);
                CollectionAssert.AreEqual(new Dictionary<string, double>
                {
                    {ModelCore.MergedScoreName, 0},
                    {"M1", 0},
                }, scores[new IntPtr(4)]);
            };
            modelCore.Start();
        }

        [TestMethod]
        public void TestOnScoreChanged()
        {
            var stubModel = new StubIModel
            {
                NameGet = () => "M1",
                GetScores = () => new Dictionary<IntPtr, double>
                    {
                        {new IntPtr(1), 1},
                        {new IntPtr(2), 1},
                        {new IntPtr(3), 0},
                        {new IntPtr(4), 0}
                    }.ToImmutableDictionary()
            };

            var stubModel2 = new StubIModel
            {
                NameGet = () => "M2",
                GetScores = () => new Dictionary<IntPtr, double>
                    {
                        {new IntPtr(1), 4},
                        {new IntPtr(2), 0},
                        {new IntPtr(3), 1}
                    }.ToImmutableDictionary()
            };

            var modelCore = new ModelCore(new (IModel, double)[]
            {
                    (stubModel, 0.5),
                    (stubModel2, 1),
            });
            modelCore.ScoreChanged += (sender, scores) =>
            {
                CollectionAssert.AreEqual(new Dictionary<string, double>
                {
                    {ModelCore.MergedScoreName, 0.7},
                    {"M1", 0.5},
                    {"M2", 0.8},
                }, scores[new IntPtr(1)]);
                CollectionAssert.AreEqual(new Dictionary<string, double>
                {
                    {ModelCore.MergedScoreName, 0.5 / 3},
                    {"M1", 0.5},
                    {"M2", 0},
                }, scores[new IntPtr(2)]);
                CollectionAssert.AreEqual(new Dictionary<string, double>
                {
                    {ModelCore.MergedScoreName, 0.4 / 3},
                    {"M1", 0},
                    {"M2", 0.2},
                }, scores[new IntPtr(3)]);
                CollectionAssert.AreEqual(new Dictionary<string, double>
                {
                    {ModelCore.MergedScoreName, 0},
                    {"M1", 0},
                }, scores[new IntPtr(4)]);
            };
            stubModel.ScoreChangedEvent.Invoke(stubModel, null);
        }

        [TestMethod]
        public void TestOnScoreChanged_NoChange()
        {
            var windowChanged = false;
            void OnScoreChanged(object sender, Dictionary<IntPtr, Dictionary<string, double>> scores)
            {
                if (!windowChanged)
                {
                    CollectionAssert.AreEqual(new Dictionary<string, double>
                    {
                        {ModelCore.MergedScoreName, 0.6},
                        {"M1", 0.6},
                    }, scores[new IntPtr(1)]);
                    CollectionAssert.AreEqual(new Dictionary<string, double>
                    {
                        {ModelCore.MergedScoreName, 0.4},
                        {"M1", 0.4},
                    }, scores[new IntPtr(2)]);
                    windowChanged = true;
                }
                else
                {
                    Assert.Fail();
                }
            }

            var firstCall = true;
            ImmutableDictionary<IntPtr, double> GetScores()
            {
                if (firstCall)
                {
                    firstCall = false;
                    return new Dictionary<IntPtr, double>
                    {
                        {new IntPtr(1), 0.6},
                        {new IntPtr(2), 0.4},
                    }.ToImmutableDictionary();
                }
                return new Dictionary<IntPtr, double>
                {
                    {new IntPtr(2), 0.4},
                    {new IntPtr(1), 0.6},
                }.ToImmutableDictionary();
            }

            var stubModel = new StubIModel
            {
                NameGet = () => "M1",
                GetScores = GetScores,
            };
            var modelCore = new ModelCore(new (IModel, double)[]
            {
                (stubModel, 1),
            });

            modelCore.ScoreChanged += OnScoreChanged;
            modelCore.Start();

            stubModel.ScoreChangedEvent.Invoke(stubModel, null);
            CollectionAssert.AreEqual(new List<IntPtr>
            {
                new IntPtr(1),
                new IntPtr(2),
            }, modelCore.GetTopWindows());
        }

        [TestMethod]
        public void TestOnWindowsChanged()
        {
            var windowChanged = false;
            void OnWindowsChange(object sender, List<IntPtr> topWindows)
            {
                if (!windowChanged)
                {
                    CollectionAssert.AreEqual(new List<IntPtr>
                    {
                        new IntPtr(1),
                        new IntPtr(2),
                    }, topWindows);
                    windowChanged = true;
                }
                else
                {
                    Assert.Fail();
                }
            }

            var firstCall = true;
            ImmutableDictionary<IntPtr, double> GetScores()
            {
                if (firstCall)
                {
                    firstCall = false;
                    return new Dictionary<IntPtr, double>
                    {
                        {new IntPtr(1), 0.6},
                        {new IntPtr(2), 0.4},
                    }.ToImmutableDictionary();
                }
                return new Dictionary<IntPtr, double>
                {
                    {new IntPtr(1), 0.6},
                    {new IntPtr(2), 0.5},
                }.ToImmutableDictionary();
            }

            var stubModel = new StubIModel
            {
                NameGet = () => "M1",
                GetScores = GetScores,
            };
            var modelCore = new ModelCore(new (IModel, double)[]
            {
                (stubModel, 1),
            });

            modelCore.WindowsChanged += OnWindowsChange;
            modelCore.Start();

            stubModel.ScoreChangedEvent.Invoke(stubModel, null);
            CollectionAssert.AreEqual(new List<IntPtr>
            {
                new IntPtr(1),
                new IntPtr(2),
            }, modelCore.GetTopWindows());
        }

        [TestMethod]
        public void TestNormalizeScores_Empty()
        {
            var scores = ImmutableDictionary<IntPtr, double>.Empty;
            CollectionAssert.AreEqual(scores, ModelCore.NormalizeScores(scores));
        }

        [TestMethod]
        public void TestNormalizeScores_ZeroSum()
        {
            var scores = new Dictionary<IntPtr, double>
            {
                { new IntPtr(1), 0 }
            }.ToImmutableDictionary();
            CollectionAssert.AreEqual(scores, ModelCore.NormalizeScores(scores));
        }

        [TestMethod]
        public void TestNormalizeScores()
        {
            CollectionAssert.AreEqual(new Dictionary<IntPtr, double>
            {
                { new IntPtr(1), 0.2 },
                { new IntPtr(2), 0.4 },
                { new IntPtr(3), 0.4 }
            }, ModelCore.NormalizeScores(new Dictionary<IntPtr, double>
            {
                { new IntPtr(1), 0.1 },
                { new IntPtr(2), 0.2 },
                { new IntPtr(3), 0.2 }
            }.ToImmutableDictionary()));
        }
    }
}
