using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Data;
using Shared.Data.Fakes;
using System;
using System.Collections.Generic;
using System.IO;
using WindowRecommender;
using WindowRecommender.Data;
using WindowRecommender.Graphics;
using WindowRecommender.Models;

namespace WindowRecommenderTests.Data
{
    [TestClass]
    public class QueriesTest
    {
        private DatabaseImplementation _db;
        private string _exportFilePath;

        [TestInitialize]
        public void Initialize()
        {
            // Set file path to temp to keep tests messages out of log
            _exportFilePath = Shared.Settings.ExportFilePath;
            Shared.Settings.ExportFilePath = Path.GetTempPath();
            _db = new DatabaseImplementation(":memory:");
            _db.Connect();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _db.Disconnect();
            Shared.Settings.ExportFilePath = _exportFilePath;
        }

        [TestMethod]
        public void TestCreateTables()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Assert.IsFalse(_db.HasTable(Settings.WindowEventTable));
                Queries.CreateTables();
                Assert.IsTrue(_db.HasTable(Settings.WindowEventTable));
            }
        }

        [TestMethod]
        public void TestCreateTables_Again()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Assert.IsFalse(_db.HasTable(Settings.WindowEventTable));
                Queries.CreateTables();
                Assert.IsTrue(_db.HasTable(Settings.WindowEventTable));
                Queries.CreateTables();
                Assert.IsTrue(_db.HasTable(Settings.WindowEventTable));
            }
        }

        [TestMethod]
        public void TestDropTables()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Assert.IsFalse(_db.HasTable(Settings.WindowEventTable));
                Queries.CreateTables();
                Assert.IsTrue(_db.HasTable(Settings.WindowEventTable));
                Queries.DropTables();
                Assert.IsFalse(_db.HasTable(Settings.WindowEventTable));
            }
        }

        [TestMethod]
        public void TestSaveEvent_Focus()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Queries.CreateTables();
                Assert.AreEqual(0L, _db.ExecuteScalar2($@"SELECT COUNT(*) FROM {Settings.WindowEventTable};"));
                Queries.SaveWindowEvent(EventName.Focus, new WindowEventRecord(new IntPtr(1), "test_title", "test_process", 1, 2, 0.5));
                var dataTable = _db.ExecuteReadQuery($@"SELECT * FROM {Settings.WindowEventTable};");
                Assert.AreEqual(1, dataTable.Rows.Count);
                Assert.AreEqual("1", dataTable.Rows[0]["windowHandle"]);
                Assert.AreEqual("Focus", dataTable.Rows[0]["event"]);
                Assert.AreEqual("test_process", dataTable.Rows[0]["processName"]);
                Assert.AreEqual("test_title", dataTable.Rows[0]["windowTitle"]);
                Assert.AreEqual(2L, dataTable.Rows[0]["rank"]);
                Assert.AreEqual(0.5, dataTable.Rows[0]["score"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["zIndex"]);
            }
        }

        [TestMethod]
        public void TestSaveEvent_Open()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Queries.CreateTables();
                Assert.AreEqual(0L, _db.ExecuteScalar2($@"SELECT COUNT(*) FROM {Settings.WindowEventTable};"));
                Queries.SaveWindowEvent(EventName.Open, new WindowEventRecord(new IntPtr(1), "test_title", "test_process", 0));
                var dataTable = _db.ExecuteReadQuery($@"SELECT * FROM {Settings.WindowEventTable};");
                Assert.AreEqual(1, dataTable.Rows.Count);
                Assert.AreEqual("1", dataTable.Rows[0]["windowHandle"]);
                Assert.AreEqual("Open", dataTable.Rows[0]["event"]);
                Assert.AreEqual("test_process", dataTable.Rows[0]["processName"]);
                Assert.AreEqual("test_title", dataTable.Rows[0]["windowTitle"]);
                Assert.AreEqual(-1L, dataTable.Rows[0]["rank"]);
                Assert.AreEqual(-1D, dataTable.Rows[0]["score"]);
                Assert.AreEqual(0L, dataTable.Rows[0]["zIndex"]);
            }
        }

        [TestMethod]
        public void TestSaveEvent_Close()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Queries.CreateTables();
                Assert.AreEqual(0L, _db.ExecuteScalar2($@"SELECT COUNT(*) FROM {Settings.WindowEventTable};"));
                Queries.SaveWindowEvent(EventName.Close, new WindowEventRecord(new IntPtr(1), "test_title", "test_process", 1, 2, 0.5));
                var dataTable = _db.ExecuteReadQuery($@"SELECT * FROM {Settings.WindowEventTable};");
                Assert.AreEqual(1, dataTable.Rows.Count);
                Assert.AreEqual("1", dataTable.Rows[0]["windowHandle"]);
                Assert.AreEqual("Close", dataTable.Rows[0]["event"]);
                Assert.AreEqual("test_process", dataTable.Rows[0]["processName"]);
                Assert.AreEqual("test_title", dataTable.Rows[0]["windowTitle"]);
                Assert.AreEqual(2L, dataTable.Rows[0]["rank"]);
                Assert.AreEqual(0.5, dataTable.Rows[0]["score"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["zIndex"]);
            }
        }

        [TestMethod]
        public void TestSaveEvent_Minimize()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Queries.CreateTables();
                Assert.AreEqual(0L, _db.ExecuteScalar2($@"SELECT COUNT(*) FROM {Settings.WindowEventTable};"));
                Queries.SaveWindowEvent(EventName.Minimize, new WindowEventRecord(new IntPtr(1), "test_title", "test_process", 1, 2, 0.5));
                var dataTable = _db.ExecuteReadQuery($@"SELECT * FROM {Settings.WindowEventTable};");
                Assert.AreEqual(1, dataTable.Rows.Count);
                Assert.AreEqual("1", dataTable.Rows[0]["windowHandle"]);
                Assert.AreEqual("Minimize", dataTable.Rows[0]["event"]);
                Assert.AreEqual("test_process", dataTable.Rows[0]["processName"]);
                Assert.AreEqual("test_title", dataTable.Rows[0]["windowTitle"]);
                Assert.AreEqual(2L, dataTable.Rows[0]["rank"]);
                Assert.AreEqual(0.5, dataTable.Rows[0]["score"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["zIndex"]);
            }
        }

        [TestMethod]
        public void TestSaveEvents()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Queries.CreateTables();
                Assert.AreEqual(0L, _db.ExecuteScalar2($@"SELECT COUNT(*) FROM {Settings.WindowEventTable};"));
                Queries.SaveWindowEvents(EventName.Initial, new[]
                {
                    new WindowEventRecord(new IntPtr(1), "test_title1", "test_process1", 1),
                    new WindowEventRecord(new IntPtr(2), "test_title2", "test_process2", 2)
                });
                var dataTable = _db.ExecuteReadQuery($@"SELECT * FROM {Settings.WindowEventTable};");
                Assert.AreEqual(2, dataTable.Rows.Count);

                Assert.AreEqual("1", dataTable.Rows[0]["windowHandle"]);
                Assert.AreEqual("Initial", dataTable.Rows[0]["event"]);
                Assert.AreEqual("test_process1", dataTable.Rows[0]["processName"]);
                Assert.AreEqual("test_title1", dataTable.Rows[0]["windowTitle"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["zIndex"]);
                Assert.AreEqual(-1L, dataTable.Rows[0]["rank"]);
                Assert.AreEqual(-1D, dataTable.Rows[0]["score"]);

                Assert.AreEqual("2", dataTable.Rows[1]["windowHandle"]);
                Assert.AreEqual("Initial", dataTable.Rows[1]["event"]);
                Assert.AreEqual("test_process2", dataTable.Rows[1]["processName"]);
                Assert.AreEqual("test_title2", dataTable.Rows[1]["windowTitle"]);
                Assert.AreEqual(2L, dataTable.Rows[1]["zIndex"]);
                Assert.AreEqual(-1L, dataTable.Rows[0]["rank"]);
                Assert.AreEqual(-1D, dataTable.Rows[0]["score"]);
            }
        }

        [TestMethod]
        public void TestSaveScoreChange()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Queries.CreateTables();
                Assert.AreEqual(0L, _db.ExecuteScalar2($@"SELECT COUNT(*) FROM {Settings.ScoreChangeTable};"));
                Queries.SaveScoreChange(new[]
                {
                    new ScoreRecord(new IntPtr(1), new Dictionary<string, double>
                    {
                        {ModelCore.MergedScoreName, 0.1},
                        {nameof(Duration), 0.1},
                        {nameof(Frequency), 0.1},
                        {nameof(MostRecentlyActive), 0.1},
                        {nameof(TitleSimilarity), 0.1},
                    }),
                    new ScoreRecord(new IntPtr(2), new Dictionary<string, double>
                    {
                        {ModelCore.MergedScoreName, 1},
                        {nameof(Duration), 1},
                        {nameof(Frequency), 1},
                    }),
                });
                var dataTable = _db.ExecuteReadQuery($@"SELECT * FROM {Settings.ScoreChangeTable};");
                Assert.AreEqual(2, dataTable.Rows.Count);

                Assert.AreEqual("1", dataTable.Rows[0]["windowHandle"]);
                Assert.AreEqual(0.1, dataTable.Rows[0]["mergedScore"]);
                Assert.AreEqual(0.1, dataTable.Rows[0]["durationScore"]);
                Assert.AreEqual(0.1, dataTable.Rows[0]["frequencyScore"]);
                Assert.AreEqual(0.1, dataTable.Rows[0]["mraScore"]);
                Assert.AreEqual(0.1, dataTable.Rows[0]["titleScore"]);

                Assert.AreEqual("2", dataTable.Rows[1]["windowHandle"]);
                Assert.AreEqual(1D, dataTable.Rows[1]["mergedScore"]);
                Assert.AreEqual(1D, dataTable.Rows[1]["durationScore"]);
                Assert.AreEqual(1D, dataTable.Rows[1]["frequencyScore"]);
                Assert.AreEqual(0D, dataTable.Rows[1]["mraScore"]);
                Assert.AreEqual(0D, dataTable.Rows[1]["titleScore"]);
            }
        }

        [TestMethod]
        public void TestSaveDesktopEvents()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Queries.CreateTables();
                Assert.AreEqual(0L, _db.ExecuteScalar2($@"SELECT COUNT(*) FROM {Settings.DesktopEventTable};"));
                Queries.SaveDesktopEvents(new[]
                {
                    new DesktopWindowRecord(new IntPtr(1), false, 0, new Rectangle(1,1,1,1)),
                    new DesktopWindowRecord(new IntPtr(2), true, 1, new Rectangle(2,2,2,2)),
                });
                var dataTable = _db.ExecuteReadQuery($@"SELECT * FROM {Settings.DesktopEventTable};");
                Assert.AreEqual(2, dataTable.Rows.Count);

                Assert.AreEqual("1", dataTable.Rows[0]["windowHandle"]);
                Assert.AreEqual(0L, dataTable.Rows[0]["zIndex"]);
                Assert.AreEqual(0L, dataTable.Rows[0]["hazed"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["left"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["top"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["right"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["bottom"]);

                Assert.AreEqual("2", dataTable.Rows[1]["windowHandle"]);
                Assert.AreEqual(1L, dataTable.Rows[1]["zIndex"]);
                Assert.AreEqual(1L, dataTable.Rows[1]["hazed"]);
                Assert.AreEqual(2L, dataTable.Rows[1]["left"]);
                Assert.AreEqual(2L, dataTable.Rows[1]["top"]);
                Assert.AreEqual(2L, dataTable.Rows[1]["right"]);
                Assert.AreEqual(2L, dataTable.Rows[1]["bottom"]);
            }
        }

        [TestMethod]
        public void TestSaveScreenEvents()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Queries.CreateTables();
                Assert.AreEqual(0L, _db.ExecuteScalar2($@"SELECT COUNT(*) FROM {Settings.ScreenEventTable};"));
                Queries.SaveScreenEvents(new[]
                {
                    new Rectangle(1,1,1,1),
                    new Rectangle(2,2,2,2),
                });
                var dataTable = _db.ExecuteReadQuery($@"SELECT * FROM {Settings.ScreenEventTable};");
                Assert.AreEqual(2, dataTable.Rows.Count);

                Assert.AreEqual(0L, dataTable.Rows[0]["screenId"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["left"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["top"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["right"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["bottom"]);

                Assert.AreEqual(1L, dataTable.Rows[1]["screenId"]);
                Assert.AreEqual(2L, dataTable.Rows[1]["left"]);
                Assert.AreEqual(2L, dataTable.Rows[1]["top"]);
                Assert.AreEqual(2L, dataTable.Rows[1]["right"]);
                Assert.AreEqual(2L, dataTable.Rows[1]["bottom"]);
            }
        }

        [TestMethod]
        public void TestSavePopupResponses()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Queries.CreateTables();
                Assert.AreEqual(0L, _db.ExecuteScalar2($@"SELECT COUNT(*) FROM {Settings.PopupResponseTable};"));
                Queries.SavePopupResponses(new[]
                {
                    (handle: "1", relevant: true, hazed: false),
                    (handle: "2", relevant: false, hazed: true),
                });
                var dataTable = _db.ExecuteReadQuery($@"SELECT * FROM {Settings.PopupResponseTable};");
                Assert.AreEqual(2, dataTable.Rows.Count);

                Assert.AreEqual("1", dataTable.Rows[0]["windowHandle"]);
                Assert.AreEqual(1L, dataTable.Rows[0]["relevant"]);
                Assert.AreEqual(0L, dataTable.Rows[0]["hazed"]);

                Assert.AreEqual("2", dataTable.Rows[1]["windowHandle"]);
                Assert.AreEqual(0L, dataTable.Rows[1]["relevant"]);
                Assert.AreEqual(1L, dataTable.Rows[1]["hazed"]);
            }
        }

        [TestMethod]
        public void TestEnabledSettings()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Assert.IsFalse(_db.HasSetting(Settings.EnabledSettingDatabaseKey));
                Assert.AreEqual(Settings.EnabledDefault, Queries.GetEnabledSetting());
                Queries.SetEnabledSetting(!Settings.EnabledDefault);
                Assert.AreNotEqual(Settings.EnabledDefault, Queries.GetEnabledSetting());
            }
        }

        [TestMethod]
        public void TestTreatmentModeSettings()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Assert.IsFalse(_db.HasSetting(Settings.TreatmentModeSettingDatabaseKey));
                Assert.AreEqual(Settings.TreatmentModeDefault, Queries.GetTreatmentModeSetting());
                Queries.SetTreatmentModeSetting(!Settings.TreatmentModeDefault);
                Assert.AreNotEqual(Settings.TreatmentModeDefault, Queries.GetTreatmentModeSetting());
            }
        }

        [TestMethod]
        public void TestNumberOfWindowsSettings()
        {
            using (ShimsContext.Create())
            {
                ShimDatabase.GetInstance = () => _db;
                Assert.IsFalse(_db.HasSetting(Settings.NumberOfWindowsSettingDatabaseKey));
                Assert.AreEqual(Settings.NumberOfWindowsDefault, Queries.GetNumberOfWindowsSetting());
                Queries.SetNumberOfWindowsSetting(Settings.NumberOfWindowsDefault + 1);
                Assert.AreEqual(Settings.NumberOfWindowsDefault + 1, Queries.GetNumberOfWindowsSetting());
            }
        }
    }
}
