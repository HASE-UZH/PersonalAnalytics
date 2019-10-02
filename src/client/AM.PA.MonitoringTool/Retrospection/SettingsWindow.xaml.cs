// Created by André Meyer at MSR
// Created: 2016-01-03
// 
// Licensed under the MIT License. 

using Shared.Data;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Retrospection
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    /// <inheritdoc cref="Window" />
    public partial class SettingsWindow
    {
        private const string MinutesStr = " minutes";

        private bool _defaultPopUpIsEnabled;
        private int _defaultPopUpInterval;
        private bool _defaultOffice365ApiEnabled;
        private bool _defaultUserInputTrackerEnabled;
        private bool _defaultOpenRetrospectionInFullScreen;
        private bool _defaultTimeSpentShowProgramsEnabled;
        private bool _defaultTimeSpentShowEmailsEnabled;
        private bool _defaultTimeSpentHideMeetingsWithoutAttendeesEnabled;
        private bool _defaultPolarTrackerEnabled;
        private bool _defaultFitbitTrackerEnabled;
        private bool _defaultFitbitTokenRemoveEnabled;
        private bool _defaultWindowRecommenderEnabled;
        private bool _defaultWindowRecommenderTreatmentMode;
        private int _defaultWindowRecommenderNumberOfWindows;

        public SettingsDto UpdatedSettingsDto;

        public SettingsWindow(SettingsDto dto, string version)
        {
            InitializeComponent();
            TbVersion.Text = "Version: " + version;
            SetDefaultValues(dto);
        }

        private void SetDefaultValues(SettingsDto dto)
        {
            // get defaults
            _defaultPopUpIsEnabled = dto.PopUpEnabled.Value;
            _defaultPopUpInterval = dto.PopUpInterval.Value;
            _defaultOffice365ApiEnabled = dto.Office365ApiEnabled.Value;
            _defaultUserInputTrackerEnabled = dto.UserInputTrackerEnabled.Value;
            _defaultOpenRetrospectionInFullScreen = dto.OpenRetrospectionInFullScreen.Value;
            _defaultTimeSpentShowProgramsEnabled = dto.TimeSpentShowProgramsEnabled.Value;
            _defaultTimeSpentHideMeetingsWithoutAttendeesEnabled = dto.TimeSpentHideMeetingsWithoutAttendeesEnabled.Value;
            _defaultTimeSpentShowEmailsEnabled = dto.TimeSpentShowEmailsEnabled.Value;
            _defaultPolarTrackerEnabled = dto.PolarTrackerEnabled.Value;
            _defaultFitbitTrackerEnabled = dto.FitbitTrackerEnabled.Value;
            _defaultFitbitTokenRemoveEnabled = dto.FitbitTokenRevokeEnabled.Value;
            _defaultWindowRecommenderEnabled = dto.WindowRecommenderEnabled.Value;
            _defaultWindowRecommenderTreatmentMode = dto.WindowRecommenderTreatmentMode.Value;
            _defaultWindowRecommenderNumberOfWindows = dto.WindowRecommenderNumberOfWindows.Value;

            // no changes yet, disable buttons by default
            SaveButtonsEnabled(false);

            // set previous values & add event handlers
            CbPopUpsEnabled.IsChecked = _defaultPopUpIsEnabled;
            CbPopUpsEnabled.Checked += CbPopUpsEnabled_Checked;
            CbPopUpsEnabled.Unchecked += CbPopUpsEnabled_Checked;

            CbPopUpInterval.SelectedValue = _defaultPopUpInterval + MinutesStr;

            CbOfficeApiEnabled.IsChecked = _defaultOffice365ApiEnabled;
            CbOfficeApiEnabled.Checked += CbChecked_Update;
            CbOfficeApiEnabled.Unchecked += CbChecked_Update;

            CbOpenRetrospectionInFullScreen.IsChecked = _defaultOpenRetrospectionInFullScreen;
            CbOpenRetrospectionInFullScreen.Checked += CbChecked_Update;
            CbOpenRetrospectionInFullScreen.Unchecked += CbChecked_Update;

            CbTimeSpentShowProgramsEnabled.IsChecked = _defaultTimeSpentShowProgramsEnabled;
            CbTimeSpentShowProgramsEnabled.Checked += CbChecked_Update;
            CbTimeSpentShowProgramsEnabled.Unchecked += CbChecked_Update;

            CbTimeSpentHideMeetingsWithoutAttendeesEnabled.IsChecked = _defaultTimeSpentHideMeetingsWithoutAttendeesEnabled;
            CbTimeSpentHideMeetingsWithoutAttendeesEnabled.Checked += CbChecked_Update;
            CbTimeSpentHideMeetingsWithoutAttendeesEnabled.Unchecked += CbChecked_Update;

            CbTimeSpentShowEmailsEnabled.IsChecked = _defaultTimeSpentShowEmailsEnabled;
            CbTimeSpentShowEmailsEnabled.Checked += CbChecked_Update;
            CbTimeSpentShowEmailsEnabled.Unchecked += CbChecked_Update;

            CbUserInputTrackerEnabled.IsChecked = _defaultUserInputTrackerEnabled;
            CbUserInputTrackerEnabled.Checked += CbChecked_Update;
            CbUserInputTrackerEnabled.Unchecked += CbChecked_Update;

            if (CbPopUpsEnabled.IsChecked.Value)
            {
                CbPopUpInterval.IsEnabled = true;
            }
            CbPopUpInterval.SelectionChanged += CbSelectionChanged;

            PolarEnabled.IsChecked = _defaultPolarTrackerEnabled;
            PolarEnabled.Checked += CbChecked_Update;
            PolarEnabled.Unchecked += CbChecked_Update;

            FitbitEnabled.IsChecked = _defaultFitbitTrackerEnabled;
            FitbitEnabled.Checked += CbChecked_Update;
            FitbitEnabled.Unchecked += CbChecked_Update;

            FitbitRevoke.IsEnabled = _defaultFitbitTokenRemoveEnabled;

            WindowRecommenderEnabled.IsChecked = _defaultWindowRecommenderEnabled;
            WindowRecommenderEnabled.Checked += CbChecked_Update;
            WindowRecommenderEnabled.Unchecked += CbChecked_Update;

            WindowRecommenderTrackingMode.IsChecked = !_defaultWindowRecommenderTreatmentMode;
            WindowRecommenderTrackingMode.Checked += CbChecked_Update;

            WindowRecommenderTreatmentMode.IsChecked = _defaultWindowRecommenderTreatmentMode;
            WindowRecommenderTreatmentMode.Checked += CbChecked_Update;

            WindowRecommenderNumberOfWindows.SelectedValue = _defaultWindowRecommenderNumberOfWindows;
            WindowRecommenderNumberOfWindows.SelectionChanged += CbSelectionChanged;
        }

        #region User Changed Values

        private void CbPopUpsEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (CbPopUpsEnabled.IsChecked != null) CbPopUpInterval.IsEnabled = CbPopUpsEnabled.IsChecked.Value;
            UpdateSettingsChanged();
        }

        private void CbChecked_Update(object sender, RoutedEventArgs e)
        {
            UpdateSettingsChanged();
        }

        private void CbSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSettingsChanged();
        }

        private void UpdateSettingsChanged()
        {
            try
            {
                if (_defaultPopUpIsEnabled != CbPopUpsEnabled.IsChecked.Value
                 || _defaultPopUpInterval + MinutesStr != CbPopUpInterval.SelectedValue.ToString()
                 || _defaultOffice365ApiEnabled != CbOfficeApiEnabled.IsChecked.Value
                 || _defaultUserInputTrackerEnabled != CbUserInputTrackerEnabled.IsChecked.Value
                 || _defaultOpenRetrospectionInFullScreen != CbOpenRetrospectionInFullScreen.IsChecked.Value
                 || _defaultTimeSpentShowEmailsEnabled != CbTimeSpentShowEmailsEnabled.IsChecked.Value
                 || _defaultTimeSpentHideMeetingsWithoutAttendeesEnabled != CbTimeSpentHideMeetingsWithoutAttendeesEnabled.IsChecked.Value
                 || _defaultTimeSpentShowProgramsEnabled != CbTimeSpentShowProgramsEnabled.IsChecked.Value
                 || _defaultPolarTrackerEnabled != PolarEnabled.IsChecked.Value
                 || _defaultFitbitTrackerEnabled != FitbitEnabled.IsChecked.Value
                 || _defaultWindowRecommenderEnabled != WindowRecommenderEnabled.IsChecked.Value
                 || _defaultWindowRecommenderTreatmentMode != WindowRecommenderTreatmentMode.IsChecked.Value
                 || _defaultWindowRecommenderNumberOfWindows != int.Parse(WindowRecommenderNumberOfWindows.SelectedValue.ToString())
                 )
                {
                    SaveButtonsEnabled(true);
                }
                else
                {
                    SaveButtonsEnabled(false);
                }
            }
            catch
            {
                SaveButtonsEnabled(false);
            }
        }

        private void SaveButtonsEnabled(bool isEnabled)
        {
            BtnSave.IsEnabled = isEnabled;
        }

        #endregion

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            var dto = new SettingsDto();

            try
            {
                if (CbPopUpsEnabled.IsChecked != null && _defaultPopUpIsEnabled != CbPopUpsEnabled.IsChecked.Value)
                {
                    dto.PopUpEnabled = CbPopUpsEnabled.IsChecked.Value;
                }
                else { dto.PopUpEnabled = null; }

                if (_defaultPopUpInterval + MinutesStr != CbPopUpInterval.SelectedValue.ToString())
                {
                    var intervalString = CbPopUpInterval.SelectedValue.ToString().Replace(MinutesStr, "");
                    dto.PopUpInterval = int.Parse(intervalString, CultureInfo.InvariantCulture);
                }
                else { dto.PopUpInterval = null; }

                if (CbOfficeApiEnabled.IsChecked != null && _defaultOffice365ApiEnabled != CbOfficeApiEnabled.IsChecked.Value)
                {
                    dto.Office365ApiEnabled = CbOfficeApiEnabled.IsChecked.Value;
                }
                else { dto.Office365ApiEnabled = null; }

                if (CbOpenRetrospectionInFullScreen.IsChecked != null && _defaultOpenRetrospectionInFullScreen != CbOpenRetrospectionInFullScreen.IsChecked.Value)
                {
                    dto.OpenRetrospectionInFullScreen = CbOpenRetrospectionInFullScreen.IsChecked.Value;
                }
                else { dto.OpenRetrospectionInFullScreen = null; }

                if (CbTimeSpentShowEmailsEnabled.IsChecked != null && _defaultTimeSpentShowEmailsEnabled != CbTimeSpentShowEmailsEnabled.IsChecked.Value)
                {
                    dto.TimeSpentShowEmailsEnabled = CbTimeSpentShowEmailsEnabled.IsChecked.Value;
                }
                else { dto.TimeSpentShowEmailsEnabled = null; }

                if (CbTimeSpentHideMeetingsWithoutAttendeesEnabled.IsChecked != null && _defaultTimeSpentHideMeetingsWithoutAttendeesEnabled != CbTimeSpentHideMeetingsWithoutAttendeesEnabled.IsChecked.Value)
                {
                    dto.TimeSpentHideMeetingsWithoutAttendeesEnabled = CbTimeSpentHideMeetingsWithoutAttendeesEnabled.IsChecked.Value;
                }
                else { dto.TimeSpentHideMeetingsWithoutAttendeesEnabled = null; }

                if (CbTimeSpentShowProgramsEnabled.IsChecked != null && _defaultTimeSpentShowProgramsEnabled != CbTimeSpentShowProgramsEnabled.IsChecked.Value)
                {
                    dto.TimeSpentShowProgramsEnabled = CbTimeSpentShowProgramsEnabled.IsChecked.Value;
                }
                else { dto.TimeSpentShowProgramsEnabled = null; }

                if (CbUserInputTrackerEnabled.IsChecked != null && _defaultUserInputTrackerEnabled != CbUserInputTrackerEnabled.IsChecked.Value)
                {
                    dto.UserInputTrackerEnabled = CbUserInputTrackerEnabled.IsChecked.Value;
                }
                else { dto.UserInputTrackerEnabled = null; }

                if (_defaultPolarTrackerEnabled != PolarEnabled.IsChecked.Value)
                {
                    dto.PolarTrackerEnabled = PolarEnabled.IsChecked.Value;
                }
                else { dto.PolarTrackerEnabled = null; }

                if (_defaultFitbitTrackerEnabled != FitbitEnabled.IsChecked.Value)
                {
                    dto.FitbitTrackerEnabled = FitbitEnabled.IsChecked.Value;
                }
                else { dto.FitbitTrackerEnabled = null; }

                if (_defaultWindowRecommenderEnabled != WindowRecommenderEnabled.IsChecked.Value)
                {
                    dto.WindowRecommenderEnabled = WindowRecommenderEnabled.IsChecked.Value;
                }
                else { dto.WindowRecommenderEnabled = null; }

                if (_defaultWindowRecommenderTreatmentMode != WindowRecommenderTreatmentMode.IsChecked.Value)
                {
                    dto.WindowRecommenderTreatmentMode = WindowRecommenderTreatmentMode.IsChecked.Value;
                }
                else { dto.WindowRecommenderTreatmentMode = null; }

                if (_defaultWindowRecommenderNumberOfWindows != int.Parse(WindowRecommenderNumberOfWindows.SelectedValue.ToString()))
                {
                    dto.WindowRecommenderNumberOfWindows = int.Parse(WindowRecommenderNumberOfWindows.SelectedValue.ToString());
                }
                else { dto.WindowRecommenderNumberOfWindows = null; }
            }
            catch { }

            UpdatedSettingsDto = dto;
            DialogResult = true;
            Close();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Feedback_Clicked(object sender, EventArgs e)
        {
            Handler.GetInstance().SendFeedback();
        }

        private void PrivacyStatement_Clicked(object sender, RoutedEventArgs e)
        {
            Handler.GetInstance().OpenPrivacyStatement();
        }

        private void FitbitRevoke_Click(object sender, RoutedEventArgs e)
        {
            FitbitRevoke.IsEnabled = false;

            UpdatedSettingsDto = new SettingsDto
            {
                FitbitTokenRevoked = true
            };

            DialogResult = true;
            Close();
        }
    }
}
