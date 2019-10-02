using System;
using System.Collections.Immutable;

namespace WindowRecommender.Models
{
    internal interface IModel
    {
        event EventHandler ScoreChanged;

        string Name { get; }

        ImmutableDictionary<IntPtr, double> GetScores();
    }
}
