using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace WindowRecommender.Models
{
    internal abstract class BaseModel : IModel
    {
        public event EventHandler ScoreChanged;

        public string Name => GetType().Name;

        internal BaseModel(IWindowEvents windowEvents)
        {
            windowEvents.Setup += (sender, e) => Setup(e);
        }

        public abstract ImmutableDictionary<IntPtr, double> GetScores();

        protected abstract void Setup(List<WindowRecord> windowRecords);

        protected void InvokeScoreChanged()
        {
            ScoreChanged?.Invoke(this, null);
        }
    }
}
