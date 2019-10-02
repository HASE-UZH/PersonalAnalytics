// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2015-10-20
//
// Licensed under the MIT License.

using System;

namespace UserInputTracker
{
    public static class Settings
    {
        public static bool IsEnabledByDefault = true;

        // enable/disable the detailed, per-event data collection of user input
        // hint: only enable this for studies, and not for too long (as it's resources and storage intensive!)
#if PilotManu_March17
        public const bool IsDetailedCollectionEnabled = true;
#else
        public const bool IsDetailedCollectionEnabled = false;
#endif

        #region Timer Intervals

        public const int UserInputVisMinutesInterval = 10; // interval for visualizations
        private const int MouseSnapshotIntervalInSeconds = 1;
        public static TimeSpan MouseSnapshotInterval = TimeSpan.FromSeconds(MouseSnapshotIntervalInSeconds);

        internal const int UserInputAggregationIntervalInSeconds = 60; // save one entry every 60 seconds into the database => if changed, make sure to change tsStart and tsEnd in SaveToDatabaseTick
        internal static TimeSpan UserInputAggregationInterval = TimeSpan.FromSeconds(UserInputAggregationIntervalInSeconds);

        #endregion

        #region Database Tables

        public const string DbTableUserInput_v2 = "user_input"; // aggregate of user inputs per minute (use this, not the *_v1 ones if possible!)
        public const string DbTableKeyboard_v1 = "user_input_keyboard"; // for old deployments & in case a study needs more detailed data
        public const string DbTableMouseClick_v1 = "user_input_mouse_click"; // for old deployments & in case a study needs more detailed data
        public const string DbTableMouseScrolling_v1 = "user_input_mouse_scrolling"; // for old deployments & in case a study needs more detailed data
        public const string DbTableMouseMovement_v1 = "user_input_mouse_movement"; // for old deployments & in case a study needs more detailed data

        #endregion

        #region user input level weighting

        // Keystroke Ratio = 1 (base unit)
        public const int MouseClickKeyboardRatio = 3;
        public const double MouseMovementKeyboardRatio = 0.0028;
        public const double MouseScrollingKeyboardRatio = 1.55;
        //public const double MouseScrollingKeyboardRatio = 0.016;

        #endregion
    }
}
