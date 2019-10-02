// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2015-10-20
//
// Licensed under the MIT License.
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using PersonalAnalytics.Views;
using Shared;
using Shared.Data;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PersonalAnalytics
{
    public class TrackerManager
    {
        private static TrackerManager _manager; // singleton instance
        private static TrackerSettings _settings;
        private readonly List<ITracker> _trackers = new List<ITracker>();
        private TaskbarIcon _taskbarIcon;

        private DispatcherTimer _taskbarIconTimer;
        private DispatcherTimer _remindToContinueTrackerTimer;
        private DispatcherTimer _remindToShareStudyDataTimer;
        private DispatcherTimer _checkForUpdatesTimer;

        private MenuItem _pauseContinueMenuItem;
        private string _publishedAppVersion;
        private bool _isPaused;

        #region Initialize & Handle TrackerManager

        /// <summary>
        /// Singleton
        /// </summary>
        /// <returns></returns>
        public static TrackerManager GetInstance()
        {
            return _manager ?? (_manager = new TrackerManager());
        }

        /// <summary>
        /// Register trackers for the TrackerManager (i.e. monitoring tool)
        /// (add a new tracker here to make it being integrated into the monitoring tool and retrospection)
        /// </summary>
        public List<ITracker> RegisterTrackers()
        {
            Register(new WindowsActivityTracker.Daemon());
            Register(new TimeSpentVisualizer.Visualizers.TimeSpentVisualizer());
            Register(new UserEfficiencyTracker.Daemon());
            Register(new UserInputTracker.Daemon());
            Register(new MsOfficeTracker.Daemon());
            Register(new PolarTracker.Deamon());
            Register(new FitbitTracker.Deamon());
            Register(new WindowRecommender.WindowRecommender());

#if Dev
            //Register(new PeopleVisualizer.PeopleVisualizer()); // disabled, as it's not finished and pretty slow
            //Register(new WindowsContextTracker.Daemon();); // implementation not finished

#elif TestPilot1
             // if something is only required in the standard deployment

#endif

            return _trackers;
        }

        /// <summary>
        /// Initialize the TrackerManager
        /// (prepares the settings, starts every tracker, creates a connection to the database, etc.)
        /// </summary>
        public void Start()
        {
            // show unified first start screens
            ShowFirstStartScreens();

            // Start all registered trackers. Create db tables only for trackers that are running. This implies that trackers that are turned on have to verify that the tables are created.
            foreach (var tracker in _trackers.Where(t => t.IsEnabled()))
            {
                // if tracker is disabled - don't start it
                if (!tracker.IsEnabled()) continue;

                // else: create tables & start tracker
                tracker.CreateDatabaseTablesIfNotExist();
                tracker.Start();
            }

            // run database updates for trackers
            PerformDatabaseUpdatesIfNecessary();

            // Communication
            var trackersString = string.Join(", ", _trackers.Where(t => t.IsRunning).ToList().ConvertAll(t => t.Name + " (" + t.GetVersion() + ")").ToArray());
            Database.GetInstance().LogInfo(string.Format(CultureInfo.InvariantCulture, "TrackerManager (V{0}) started with {1} trackers: {2}.", _publishedAppVersion, _trackers.Where(t => t.IsRunning).ToList().Count, trackersString));
            SetTaskbarIconTooltip("Tracker started");
            Application.Current.Dispatcher.Invoke(() => _taskbarIcon.ShowBalloonTip(Dict.ToolName, Dict.ToolName + " is running with " + _trackers.Where(t => t.IsRunning).ToList().Count + " data trackers.", BalloonIcon.Info));

            // Initialize & start the timer to update the taskbaricon toolitp
            _taskbarIconTimer = new DispatcherTimer
            {
                Interval = Settings.TooltipIconUpdateInterval
            };
            _taskbarIconTimer.Tick += UpdateTaskbarIconTooltip;
            _taskbarIconTimer.Start();

            SystemEvents.DisplaySettingsChanged += UpdateTaskbarIconIcon;

            // Initialize & start the timer to check for updates
            _checkForUpdatesTimer = new DispatcherTimer
            {
                Interval = Settings.CheckForToolUpdatesInterval
            };
            _checkForUpdatesTimer.Tick += UpdateApplicationIfNecessary;
            _checkForUpdatesTimer.Start();

            // Initialize & start the timer to remind to share study data
            if (Settings.IsUploadEnabled && Settings.IsUploadReminderEnabled)
            {
                _remindToShareStudyDataTimer = new DispatcherTimer
                {
                    Interval = Settings.CheckForStudyDataSharedReminderInterval
                };
                _remindToShareStudyDataTimer.Tick += CheckForStudyDataSharedReminder;
                _remindToShareStudyDataTimer.Start();
            }

            // track time zone changes
            Database.GetInstance().CreateTimeZoneTable();
            SaveCurrentTimeZone(null, null);
            SystemEvents.TimeChanged += SaveCurrentTimeZone;

            // update taskbar icons (e.g. menu items that are added later)
            SetContextMenuOptions();
        }

        /// <summary>
        /// show unified first start screens for tool and each tracker (where necessary)
        /// </summary>
        private void ShowFirstStartScreens()
        {
            var startScreens = new List<IFirstStartScreen>();

            // add first start screen for tool if not yet shown
            if (!Database.GetInstance().HasSetting("FirstStartWindowShown"))
            {
                startScreens.Add(new FirstStartWindow());
            }

            // add first start screen of each tracker where not yet shown
            foreach (var tracker in _trackers.Where(t => t.IsFirstStart && t.IsEnabled()))
            {
                startScreens.AddRange(tracker.GetStartScreens());
            }

            // if there is any start screens: show them
            if (startScreens.Count > 0)
            {
                var container = new StartScreenContainer(_publishedAppVersion, startScreens);
                container.ShowDialog();
            }
        }

        /// <summary>
        /// In case the current database version != the targeted database version,
        /// perform an incremental udpate to the database (for each tracker)
        /// </summary>
        private void PerformDatabaseUpdatesIfNecessary()
        {
            var currentVersion = Database.GetInstance().GetDbPragmaVersion();
            var targetVersion = Settings.DatabaseVersion;
            if (currentVersion != targetVersion)
            {
                // run update commands for each version separately
                while (currentVersion < targetVersion)
                {
                    currentVersion++; // increment database version
                    foreach (var tracker in _trackers)
                    {
                        tracker.UpdateDatabaseTables(currentVersion);
                    }
                }
                // update database version
                Database.GetInstance().UpdateDbPragmaVersion(targetVersion);
            }
        }

        /// <summary>
        /// Sets the current published app version
        /// </summary>
        /// <param name="v"></param>
        public void SetAppVersion(string v)
        {
            _publishedAppVersion = v;
        }

        /// <summary>
        /// Method to get the dll versions of the dlls which are used
        /// in external projects.
        /// </summary>
        private void GetDllVersions()
        {
            var path = @"C:\DATA\DEV\UZH\PA\Tool_Git\PersonalAnalytics\documentation\dlls";
            var dllsToCheckVersion = new List<string> { "Shared.dll", "UserInputTracker.dll", "WindowsActivityTracker.dll", "FocusLightTracker.dll" };

            foreach (var dll in dllsToCheckVersion)
            {
                var dllPath = Path.Combine(path, dll);
                var dllAssembly = Assembly.LoadFrom(dllPath);
                Version dllVersion = dllAssembly.GetName().Version;
                Console.WriteLine("{0}\t{1}", dll, dllVersion);
            }
        }

        /// <summary>
        /// Saves the current (updated) time zone to the database
        /// (temporary workaround)
        /// </summary>
        private void SaveCurrentTimeZone(object sender, EventArgs e)
        {
            CultureInfo.CurrentCulture.ClearCachedData();
            var lastEntryTimeZone = Database.GetInstance().GetLastTimeZoneEntry();
            var currentTimeZone = TimeZoneInfo.Local;

            if (lastEntryTimeZone != null && lastEntryTimeZone.Id == currentTimeZone.Id) return;
            Database.GetInstance().LogTimeZoneChange(currentTimeZone);
        }

        /// <summary>
        /// stops the TrackerManager (stops every tracker, saves entries, dismisses connection to the database, etc.)
        /// </summary>
        public void Stop(bool stoppedManually = false)
        {
            // stop trackers
            foreach (var tracker in _trackers)
            {
                tracker.Stop();
            }

            // close the retrospection window
            Retrospection.Handler.GetInstance().CloseRetrospection();

            // shutdown the visualization server
            Retrospection.Handler.GetInstance().Stop();

            // stop timers & unregister
            if (_taskbarIconTimer != null) _taskbarIconTimer.Stop();
            if (_remindToContinueTrackerTimer != null) _remindToContinueTrackerTimer.Stop();
            if (_checkForUpdatesTimer != null) _checkForUpdatesTimer.Stop();
            _taskbarIcon.TrayBalloonTipClicked -= TrayBalloonTipClicked;
            _taskbarIcon.TrayMouseDoubleClick -= (o, i) => TrayMouseDoubleClicked();
            SystemEvents.DisplaySettingsChanged -= UpdateTaskbarIconIcon;
            SystemEvents.TimeChanged -= SaveCurrentTimeZone;

            // sometimes the icon doesn't go away unless we manually dispose it
            _taskbarIcon.Dispose();

            // log shutdown & disconnect from database
            Database.GetInstance().LogInfo("The tracker was shut down.");
            Database.GetInstance().Disconnect();

            // kill the process
            if (stoppedManually)
            {
                ShutdownApplication();
            }
        }

        /// <summary>
        /// stops all trackers
        /// starts a reminder to remind the user to resume it
        /// </summary>
        public void Pause()
        {
            foreach (var tracker in _trackers)
            {
                tracker.Stop();
            }
            _isPaused = true;

            if (_remindToContinueTrackerTimer == null)
            {
                _remindToContinueTrackerTimer = new DispatcherTimer
                {
                    Interval = Settings.RemindToResumeToolInterval
                };
                _remindToContinueTrackerTimer.Tick += ((s, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // show the popup (already registered for the click event)
                        _taskbarIcon.ShowBalloonTip("Reminder", "PersonalAnalytics is still paused. Click here to resume it.", BalloonIcon.Warning);
                    });
                });
            }
            _remindToContinueTrackerTimer.IsEnabled = true;
            _remindToContinueTrackerTimer.Start();

            Database.GetInstance().LogInfo("The participant paused the trackers.");
        }

        /// <summary>
        /// continues all trackers (that are enabled)
        /// </summary>
        public void Continue()
        {
            // disable the reminder
            if (_remindToContinueTrackerTimer != null)
            {
                _remindToContinueTrackerTimer.Stop();
                _remindToContinueTrackerTimer.IsEnabled = false;
            }

            // continue the trackers
            foreach (var tracker in _trackers)
            {
                if (!tracker.IsEnabled()) continue;
                tracker.Start();
            }
            _isPaused = false;

            Database.GetInstance().LogInfo("The participant resumed the trackers.");
        }

        /// <summary>
        /// Tracker registers its service to the TrackerManager
        /// </summary>
        /// <param name="t"></param>
        public void Register(ITracker t)
        {
            _trackers.Add(t);
        }

        /// <summary>
        /// Initialize/Prepare Settings
        /// </summary>
        public void PrepareSettings()
        {
            _settings = new TrackerSettings(_trackers);
        }

        public List<ITracker> GetTrackers()
        {
            return _trackers;
        }

        #endregion

        #region Taskbar Icon Options

        /// <summary>
        /// Creates a taskbar icon to modify its tooltip and create the context menu options
        /// </summary>
        public void InitializeTaskBarIcon()
        {
            _taskbarIcon = new TaskbarIcon();
            _taskbarIcon.TrayMouseDoubleClick += (o, i) => TrayMouseDoubleClicked();
            _taskbarIcon.TrayBalloonTipClicked += TrayBalloonTipClicked;
            SetTaskbarIconTooltip("PersonalAnalytics starting up...");
            SetTaskbarIconIcon();
            SetContextMenuOptions();
        }

        /// <summary>
        /// Click on a TaskbarIcon Balloon to resume the tracker
        /// todo: as soon as there are other balloons shown, we need to be able to distinguish which one
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void TrayBalloonTipClicked(object s, RoutedEventArgs e)
        {
            // if tracker is currently paused, the user probably wants to continue the tracking
            if (_isPaused) PauseContinueTracker(_pauseContinueMenuItem);
        }

        /// <summary>
        /// Called when the user double clicks the system tray icon
        /// Open the retrospection if it is enabled
        /// </summary>
        private void TrayMouseDoubleClicked()
        {
            if (Retrospection.Settings.IsEnabled) OpenRetrospection();
            // else: do nothing
        }

        /// <summary>
        /// Manually add ContextMenuOptions
        /// </summary>
        private void SetContextMenuOptions()
        {
            var cm = new ContextMenu();
            _taskbarIcon.MenuActivation = PopupActivationMode.RightClick;
            _taskbarIcon.ContextMenu = cm;

            if (Settings.IsUploadEnabled)
            {
                var m8 = new MenuItem { Header = "Upload collected data" };
                m8.Click += (o, i) => UploadTrackedData();
                _taskbarIcon.ContextMenu.Items.Add(m8);
            }

            // Menu Entry to open the WindowDimmer Share Dialog
            // var m9 = new MenuItem { Header = "Share Results" };
            // m9.Click += (o, i) => WindowRecommender.WindowRecommender.ShareResults();
            // _taskbarIcon.ContextMenu.Items.Add(m9);

            var m4 = new MenuItem { Header = "Open collected data" };
            m4.Click += (o, i) => OpenDataExportDirectory();
            _taskbarIcon.ContextMenu.Items.Add(m4);

            var m2 = new MenuItem { Header = "Answer pop-up now" };
            m2.Click += (o, i) => ManuallyStartUserSurvey();
            if (_settings.IsUserEfficiencyTrackerEnabled()) _taskbarIcon.ContextMenu.Items.Add(m2);

            var m1 = new MenuItem { Header = "Show Retrospection" };
            m1.Click += (o, i) => Retrospection.Handler.GetInstance().OpenRetrospection();
            if (Retrospection.Settings.IsEnabled) _taskbarIcon.ContextMenu.Items.Add(m1);

#if DEBUG
            var m5 = new MenuItem { Header = "Show Retrospection (in browser)" };
            m5.Click += (o, i) => Retrospection.Handler.GetInstance().OpenRetrospectionInBrowser();
            if (Retrospection.Settings.IsEnabled) _taskbarIcon.ContextMenu.Items.Add(m5);
#endif

            var m6 = new MenuItem { Header = "Settings" };
            m6.Click += (o, i) => OpenSettings();
            _taskbarIcon.ContextMenu.Items.Add(m6);

            var m3 = new MenuItem { Header = "Pause " + Dict.ToolName };
            m3.Click += (o, i) => PauseContinueTracker(m3);
            _taskbarIcon.ContextMenu.Items.Add(m3);
            _pauseContinueMenuItem = m3;

            var m7 = new MenuItem { Header = "Shutdown " + Dict.ToolName };
            m7.Click += (o, i) => Stop(true);
            _taskbarIcon.ContextMenu.Items.Add(m7);

            // Styling
            //var converter = new System.Windows.Media.BrushConverter();
            //var brush = (System.Windows.Media.Brush)converter.ConvertFromString("#FFFFFF90");
            //cm.Background = brush;
            //cm.Width = 200;
            //cm.Height = 100;

            //var style = App.Current.TryFindResource("SysTrayMenu");
            //_taskbarIcon.ContextMenu = (System.Windows.Controls.ContextMenu)style;
        }

        /// <summary>
        /// depending on the variable _isPaused, the tracker is
        /// paused or continued
        /// </summary>
        /// <param name="item"></param>
        private void PauseContinueTracker(MenuItem item)
        {
            // continue
            if (_isPaused)
            {
                Continue();
                item.Header = "Pause " + Dict.ToolName;
            }
            // pause
            else
            {
                Pause();
                item.Header = "Resume " + Dict.ToolName;
            }
        }

        /// <summary>
        /// Opens the UI specified for the current retrospection
        /// </summary>
        private static void OpenRetrospection()
        {
            Retrospection.Handler.GetInstance().OpenRetrospection();
        }

        /// <summary>
        /// Opens the directory where all the data is saved
        /// </summary>
        private static void OpenDataExportDirectory()
        {
            var path = Settings.ExportFilePath;
            Process.Start(path);
        }

        /// <summary>
        /// Starts the upload wizard
        /// </summary>
        private static void UploadTrackedData(bool isManually = true)
        {
            // log
            if (isManually)
            {
                Database.GetInstance().LogInfo("The participant manually opened the upload wizard.");
            }
            else
            {
                Database.GetInstance().LogInfo("The participant opened the upload wizard after the upload reminder prompt.");
            }

            // show pop-up
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
            () =>
            {
                var uploaderWindow = new Upload.UploadWizard();
                uploaderWindow.ShowDialog();
            }));
        }

        /// <summary>
        /// Opens the browser to modify the settings
        /// </summary>
        private static void OpenSettings()
        {
            _settings.OpenSettings();
        }

        /// <summary>
        /// gets the current instance of the userefficiency tracker and manually starts
        /// the survey there
        /// todo: ugly, hardcoded
        /// </summary>
        private void ManuallyStartUserSurvey()
        {
            try
            {
                var userEfficiencyTracker =
                    _trackers.Where(t => t.GetType() == typeof(UserEfficiencyTracker.Daemon))
                        .Cast<UserEfficiencyTracker.Daemon>()
                        .FirstOrDefault();
                if (userEfficiencyTracker == null) return;
                userEfficiencyTracker.ManualTakeSurveyNow();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Updates the taskbar icon tooltip text based on the timer and
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTaskbarIconTooltip(object sender, EventArgs e)
        {
            // Update Taskbar Icon Tooltip
            var text = _trackers.Where(t => t.IsRunning).Aggregate(string.Empty, (current, tracker) => current + tracker.GetStatus() + "\n");
            text += "\n" + _trackers.Where(t => !t.IsRunning).Aggregate(string.Empty, (current, tracker) => current + tracker.GetStatus() + "\n");
            text += "\nVersion: " + _publishedAppVersion;
            SetTaskbarIconTooltip(text);
        }

        /// <summary>
        /// helper method to change the tooltip text
        /// </summary>
        /// <param name="message"></param>
        private void SetTaskbarIconTooltip(string message)
        {
            if (_taskbarIcon == null) return;
            _taskbarIcon.ToolTipText = message;
        }

        /// <summary>
        /// Updates the TaskbarIcon's Icon on change of screen resolution.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTaskbarIconIcon(object sender, EventArgs e)
        {
            SetTaskbarIconIcon();
        }

        /// <summary>
        /// Helper method to change the TaskbarIcon's Icon
        /// </summary>
        private void SetTaskbarIconIcon()
        {
            if (_taskbarIcon == null) return;
            _taskbarIcon.Icon = new Icon("Assets/icon.ico");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// On the first workday of the week, remind the user ONCE to share
        /// the collected study data with us.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForStudyDataSharedReminder(object sender, EventArgs e)
        {
            // only show reminder if the upload is enabled (i.e. during a study)
            if (!Settings.IsUploadEnabled || !Settings.IsUploadReminderEnabled) return;

            var lastTimeShown = Database.GetInstance().GetSettingsDate("LastTimeUploadReminderShown", DateTimeOffset.MinValue);
            var databaseSince = Database.GetInstance().GetSettingsDate("SettingsTableCreatedDate", DateTimeOffset.MinValue);
            var today = DateTime.Now.Date;

            // check if the reminder should be shown
            if (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday || // do not show on weekends
                 (today - databaseSince).Days < 10 || //  only if there is at least 10 days since the tool was installed
                 (today - lastTimeShown).Days < 7) // only show once a week
                return;

            // log when reminder was shown
            Database.GetInstance().LogInfo("Shown the user the study data upload reminder.");
            Database.GetInstance().SetSettings("LastTimeUploadReminderShown", today.Date.ToShortDateString());

            // show the reminder
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
            () =>
            {
                var reminderWindow = new Upload.UploadReminder();
                if (reminderWindow.ShowDialog() == true) // show pop-up, handle response
                {
                    if (reminderWindow.UserSelectedShareData)
                    {
                        UploadTrackedData(false);
                    }
                    else
                    {
                        Database.GetInstance().LogInfo("The participant didn't want to share the study data when the upload reminder prompt was shown.");
                    }
                }
            }));
        }

        /// <summary>
        /// Called in a regular interval to try and update the tool.
        /// Asks the user for consent prior to installing the update. In case the installation was successful, the tool will restart.
        ///
        /// source: https://msdn.microsoft.com/en-us/library/ms404263.aspx
        /// </summary>
        private void UpdateApplicationIfNecessary(object sender, EventArgs e)
        {
            UpdateCheckInfo info = null;

            // if not connected to the internet (could also ping to our deployment server)
            if (!IsConnectedToTheInternet())
            {
                Database.GetInstance().LogWarning("Cannot check for updates, no internet connection available.");
                return;
            }

            // can only update if it is network deployed
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                Database.GetInstance().LogError("Failed to check for updates for the application. IsNetworkDeployed is false.");
                return;
            }

            var ad = ApplicationDeployment.CurrentDeployment;

            try
            {
                info = ad.CheckForDetailedUpdate();
            }
            catch (DeploymentDownloadException dde)
            {
                // may rarely happen
                Database.GetInstance().LogError(string.Format(CultureInfo.InvariantCulture, "Failed to install the newest version of the application. This might be a network issue (error: DeploymentDownloadException, details: {0}).", dde.Message));
                return;
            }
            catch (InvalidDeploymentException ide)
            {
                // shouldn't happen
                Database.GetInstance().LogError(string.Format(CultureInfo.InvariantCulture, "Failed to install the newest version of the application. The ClickOnce Deployment is corrupt (error: InvalidDeploymentException, details: {0}).", ide.Message));
                MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Please contact us in case this problem persists or you have any questions.",
                    Dict.ToolName + ": Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            catch (InvalidOperationException ioe)
            {
                // shouldn't happen
                Database.GetInstance().LogError(string.Format(CultureInfo.InvariantCulture, "Failed to install the newest version of the application. It is likely not a ClickOnce application (error: InvalidOperationException, details: {0}).", ioe.Message));
                return;
            }
            catch (Exception ex)
            {
                // some unknown error happened
                Logger.WriteToLogFile(ex);
                return;
            }

            if (info.UpdateAvailable)
            {
                var doUpdate = true;

                if (!info.IsUpdateRequired)
                {
                    var dr = MessageBox.Show("An update is available (from your current version to version " + info.AvailableVersion + "). Would you like to update the application now?",
                        Dict.ToolName + ": Update Available",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Question);
                    if (!(MessageBoxResult.OK == dr))
                    {
                        doUpdate = false;
                        Database.GetInstance().LogError("The user didn't want to update the tool.");
                    }
                }
                else
                {
                    // Display a message that the app MUST reboot. Display the minimum required version.
                    MessageBox.Show("A mandatory update is available (from your current version to version " + info.AvailableVersion + "). The application will now install the update and restart.",
                        Dict.ToolName + ": Mandatory Update Available",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                if (doUpdate)
                {
                    try
                    {
                        ad.Update();
                        Database.GetInstance().LogInfo(string.Format(CultureInfo.InvariantCulture, "Successfully updated tool to version {0}.", info.AvailableVersion));
                        MessageBox.Show("The application has been upgraded, and will now restart.",
                            Dict.ToolName + ": Successfully Updated",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // the only way the restart with a click once work and IsNetworkDeployed is not false after the restart is
                        // the following (according to this: http://blachniet.com/blog/how-not-to-restart-a-clickonce-application/)
                        // and https://robrelyea.wordpress.com/2007/07/24/application-restart-for-wpf/

                        Stop(false); // stop the application (restart stuff below)
                        System.Windows.Forms.Application.Restart();
                        Application.Current.Shutdown();
                    }
                    catch (DeploymentDownloadException dde)
                    {
                        Database.GetInstance().LogError(string.Format(CultureInfo.InvariantCulture, "Updating the tool to version {0} failed (error: InvalidOperationException, see errors.log for details).", info.AvailableVersion));
                        Logger.WriteToLogFile(dde);
                        MessageBox.Show("Cannot install the latest version of the application. Please check your network connection and try again later. Please contact us in case this problem persists.",
                            Dict.ToolName + ": Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                }
            }
        }

        #region Check for Internet Connection

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);

        public static bool IsConnectedToTheInternet()
        {
            return InternetGetConnectedState(out _, 0);
        }

        #endregion

        /// <summary>
        /// Shutdown the application only if the state is saved, database disconnected, etc.
        /// </summary>
        private void ShutdownApplication()
        {
            Environment.Exit(0); // this is not clean, otherwise, the httpserver is not shut down and then in some strange inconsistent state
            //App.Current.Shutdown(); //todo: not sure if this is the right way to do
            //Application.Current.Shutdown();
        }


        /*   #region User Consent

           private bool _applicationHasUserConsent;
           public bool ApplicationHasUserConsent
           {
               get
               {
                   _applicationHasUserConsent = Database.GetInstance().GetSettingsBool("ApplicationHasUserConsent", false); //default: no user consent
                   return _applicationHasUserConsent;
               }
               set
               {
                   var updatedHasConsent = value;

                   // only update if settings changed
                   if (updatedHasConsent == _applicationHasUserConsent) return;

                   // update settings
                   Database.GetInstance().SetSettings("ApplicationHasUserConsent", value);

                   // log
                   Database.GetInstance().LogInfo("The participant updated the setting 'ApplicationHasUserConsent' to " + updatedHasConsent);
               }
           }

           internal bool UserConsentsToUseApplication()
           {
               // application already has consent
               if (ApplicationHasUserConsent) return true;

               // if not, ask for consent

               return true;
           }

#endregion */

        #endregion
    }
}
