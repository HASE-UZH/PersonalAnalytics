namespace WindowRecommender
{
    internal static class Settings
    {
        internal const string WindowEventTable = "window_recommender_events";
        internal const string ScoreChangeTable = "window_recommender_scores";
        internal const string DesktopEventTable = "window_recommender_windows";
        internal const string ScreenEventTable = "window_recommender_screens";
        internal const string PopupResponseTable = "window_recommender_popup";

        internal const string EnabledSettingDatabaseKey = "WindowRecommenderEnabled";
        internal const bool EnabledDefault = false;
        internal static bool Enabled = EnabledDefault;

        internal const string NumberOfWindowsSettingDatabaseKey = "WindowRecommenderNumberOfWindows";
        internal const int NumberOfWindowsDefault = 3;
        internal static int NumberOfWindows = NumberOfWindowsDefault;

        internal const string TreatmentModeSettingDatabaseKey = "WindowRecommenderTreatmentMode";
        internal const bool TreatmentModeDefault = false;
        internal static bool TreatmentMode = TreatmentModeDefault;

        internal const int OverlayAlpha = 64;
        internal const int FramesPerSecond = 10;

        internal const int DurationIntervalSeconds = 10;
        internal const int DurationTimeframeMinutes = 10;

        internal const int FrequencyIntervalSeconds = 10;
        internal const int FrequencyTimeframeMinutes = 10;

        public static bool ShowDebugWindow = false;
    }
}
