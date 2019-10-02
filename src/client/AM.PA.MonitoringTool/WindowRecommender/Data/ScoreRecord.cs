using System;
using System.Collections.Generic;
using WindowRecommender.Util;

namespace WindowRecommender.Data
{
    internal struct ScoreRecord
    {
        internal readonly string WindowHandle;
        internal readonly double Merged;
        internal readonly double Duration;
        internal readonly double Frequency;
        internal readonly double MostRecentlyActive;
        internal readonly double TitleSimilarity;

        public ScoreRecord(IntPtr windowHandle, IDictionary<string, double> scores)
        {
            WindowHandle = windowHandle.ToString();
            Merged = scores.GetValueOrDefault(nameof(Merged));
            Duration = scores.GetValueOrDefault(nameof(Duration));
            Frequency = scores.GetValueOrDefault(nameof(Frequency));
            MostRecentlyActive = scores.GetValueOrDefault(nameof(MostRecentlyActive));
            TitleSimilarity = scores.GetValueOrDefault(nameof(TitleSimilarity));
        }
    }
}