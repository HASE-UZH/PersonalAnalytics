// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2016-01-08
// 
// Licensed under the MIT License.

using FitbitTracker.Data;
using Shared;
using Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalAnalytics
{
    /// <summary>
    /// Global settings option (to open settings window and handle settings)
    /// TODO: de-couple
    /// </summary>
    public class TrackerSettings
    {
        private readonly List<ITracker> _trackers;

        public TrackerSettings(List<ITracker> trackers)
        {
            _trackers = trackers;
        }

        public void OpenSettings()
        {
            try
            {
                // Open Settings and get response
                var settings = GetCurrentSettings();

                // Update/Save Settings
                var updatedSettings = Retrospection.Handler.GetInstance().OpenSettings(settings);

                if (updatedSettings.PopUpEnabled.HasValue)
                {
                    if (GetUserEfficiencyTracker() != null) GetUserEfficiencyTracker().PopUpEnabled = updatedSettings.PopUpEnabled.Value;
                }
                if (updatedSettings.PopUpInterval.HasValue)
                {
                    if (GetUserEfficiencyTracker() != null) GetUserEfficiencyTracker().PopUpIntervalInMins = TimeSpan.FromMinutes(updatedSettings.PopUpInterval.Value);
                }

                if (updatedSettings.UserInputTrackerEnabled.HasValue)
                {
                    if (GetUserInputTracker() != null) GetUserInputTracker().UserInputTrackerEnabled = updatedSettings.UserInputTrackerEnabled.Value;
                }

                if (updatedSettings.TimeSpentShowEmailsEnabled.HasValue)
                {
                    if (GetTimeSpentVisualizerVisualizer() != null) GetTimeSpentVisualizerVisualizer().TimeSpentShowEmailsEnabled = updatedSettings.TimeSpentShowEmailsEnabled.Value;
                }
                if (updatedSettings.TimeSpentHideMeetingsWithoutAttendeesEnabled.HasValue)
                {
                    if (GetTimeSpentVisualizerVisualizer() != null) GetTimeSpentVisualizerVisualizer().TimeSpentHideMeetingsWithoutAttendeesEnabled = updatedSettings.TimeSpentHideMeetingsWithoutAttendeesEnabled.Value;
                }
                if (updatedSettings.TimeSpentShowProgramsEnabled.HasValue)
                {
                    if (GetTimeSpentVisualizerVisualizer() != null) GetTimeSpentVisualizerVisualizer().TimeSpentShowProgramsEnabled = updatedSettings.TimeSpentShowProgramsEnabled.Value;
                }

                if (updatedSettings.OpenRetrospectionInFullScreen.HasValue)
                {
                    Retrospection.Handler.GetInstance().OpenRetrospectionInFullScreen = updatedSettings.OpenRetrospectionInFullScreen.Value;
                }

                if (updatedSettings.Office365ApiEnabled.HasValue)
                {
                    if (GetMsOfficeTracker() != null) GetMsOfficeTracker().MsOfficeTrackerEnabled = updatedSettings.Office365ApiEnabled.Value;
                }

                if (updatedSettings.PolarTrackerEnabled.HasValue)
                {
                    if (GetPolarTracker() != null) GetPolarTracker().ChangeEnableState(updatedSettings.PolarTrackerEnabled);
                }

                if (updatedSettings.FitbitTrackerEnabled.HasValue)
                {
                    if (GetFitbitTracker() != null) GetFitbitTracker().ChangeEnabledState(updatedSettings.FitbitTrackerEnabled);
                }
                if (updatedSettings.FitbitTokenRevoked.HasValue)
                {
                    FitbitConnector.RevokeAccessToken(SecretStorage.GetAccessToken());
                }

                if (updatedSettings.WindowRecommenderEnabled.HasValue)
                {
                    var windowRecommender = GetWindowRecommender();
                    if (windowRecommender != null) windowRecommender.WindowRecommenderEnabled = updatedSettings.WindowRecommenderEnabled.Value;
                }
                if (updatedSettings.WindowRecommenderTreatmentMode.HasValue)
                {
                    var windowRecommender = GetWindowRecommender();
                    if (windowRecommender != null) windowRecommender.TreatmentMode = updatedSettings.WindowRecommenderTreatmentMode.Value;
                }
                if (updatedSettings.WindowRecommenderNumberOfWindows.HasValue)
                {
                    var windowRecommender = GetWindowRecommender();
                    if (windowRecommender != null) windowRecommender.NumberOfWindows = updatedSettings.WindowRecommenderNumberOfWindows.Value;
                }
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }
        }

        private SettingsDto GetCurrentSettings()
        {
            var dto = new SettingsDto();

            try
            {
                var userEfficiencyTracker = GetUserEfficiencyTracker();
                if (userEfficiencyTracker != null) dto.PopUpEnabled = userEfficiencyTracker.PopUpEnabled;
                if (userEfficiencyTracker != null) dto.PopUpInterval = (int)userEfficiencyTracker.PopUpIntervalInMins.TotalMinutes;

                var userInputTracker = GetUserInputTracker();
                if (userInputTracker != null) dto.UserInputTrackerEnabled = userInputTracker.UserInputTrackerEnabled;

                var timeSpentVisualizer = GetTimeSpentVisualizerVisualizer();
                if (timeSpentVisualizer != null)
                {
                    dto.TimeSpentShowEmailsEnabled = timeSpentVisualizer.TimeSpentShowEmailsEnabled;
                    dto.TimeSpentHideMeetingsWithoutAttendeesEnabled = timeSpentVisualizer.TimeSpentHideMeetingsWithoutAttendeesEnabled;
                    dto.TimeSpentShowProgramsEnabled = timeSpentVisualizer.TimeSpentShowProgramsEnabled;
                }

                dto.OpenRetrospectionInFullScreen = Retrospection.Handler.GetInstance().OpenRetrospectionInFullScreen;

                var msOfficeTracker = GetMsOfficeTracker();
                dto.Office365ApiEnabled = msOfficeTracker.MsOfficeTrackerEnabled;

                var polarTracker = GetPolarTracker();
                dto.PolarTrackerEnabled = polarTracker.IsEnabled();

                var fitbitTracker = GetFitbitTracker();
                dto.FitbitTrackerEnabled = fitbitTracker.IsEnabled();
                dto.FitbitTokenRevokeEnabled = SecretStorage.GetAccessToken() != null && fitbitTracker.IsEnabled();
                dto.FitbitTokenRevoked = dto.FitbitTokenRevokeEnabled;

                var windowRecommender = GetWindowRecommender();
                dto.WindowRecommenderEnabled = windowRecommender.IsEnabled();
                dto.WindowRecommenderTreatmentMode = windowRecommender.TreatmentMode;
                dto.WindowRecommenderNumberOfWindows = windowRecommender.NumberOfWindows;
            }
            catch (Exception e)
            {
                Logger.WriteToLogFile(e);
            }

            return dto;
        }

        private FitbitTracker.Deamon GetFitbitTracker()
        {
            try
            {
                var tracker = _trackers.Where(t => t is FitbitTracker.Deamon)
                    .Cast<FitbitTracker.Deamon>()
                    .FirstOrDefault();

                return tracker;
            }
            catch { return null; }
        }

        private PolarTracker.Deamon GetPolarTracker()
        {
            try
            {
                var tracker = _trackers.Where(t => t is PolarTracker.Deamon)
                    .Cast<PolarTracker.Deamon>()
                    .FirstOrDefault();

                return tracker;
            }
            catch { return null; }
        }

        private TimeSpentVisualizer.Visualizers.TimeSpentVisualizer GetTimeSpentVisualizerVisualizer()
        {
            try
            {
                var tracker = _trackers.Where(t => t is TimeSpentVisualizer.Visualizers.TimeSpentVisualizer)
                    .Cast<TimeSpentVisualizer.Visualizers.TimeSpentVisualizer>()
                    .FirstOrDefault();

                return tracker;
            }
            catch { return null; }
        }

        private MsOfficeTracker.Daemon GetMsOfficeTracker()
        {
            try
            {
                var tracker = _trackers.Where(t => t is MsOfficeTracker.Daemon)
                    .Cast<MsOfficeTracker.Daemon>()
                    .FirstOrDefault();

                return tracker;
            }
            catch { return null; }
        }

        private UserEfficiencyTracker.Daemon GetUserEfficiencyTracker()
        {
            try
            {
                var tracker = _trackers.Where(t => t is UserEfficiencyTracker.Daemon)
                    .Cast<UserEfficiencyTracker.Daemon>()
                    .FirstOrDefault();

                return tracker;
            }
            catch { return null; }
        }

        private UserInputTracker.Daemon GetUserInputTracker()
        {
            try
            {
                var tracker = _trackers.Where(t => t is UserInputTracker.Daemon)
                    .Cast<UserInputTracker.Daemon>()
                    .FirstOrDefault();

                return tracker;
            }
            catch { return null; }
        }

        private WindowRecommender.WindowRecommender GetWindowRecommender()
        {
            try
            {
                var tracker = _trackers.Where(t => t is WindowRecommender.WindowRecommender)
                    .Cast<WindowRecommender.WindowRecommender>()
                    .FirstOrDefault();

                return tracker;
            }
            catch { return null; }
        }

        public bool IsUserEfficiencyTrackerEnabled()
        {
            var tracker = GetUserEfficiencyTracker();
            return tracker != null && tracker.PopUpEnabled;
        }
    }
}
