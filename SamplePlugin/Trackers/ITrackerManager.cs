using System;
using System.Collections.Generic;

namespace MentorRouletteCounter.Trackers
{
    internal interface ITrackerManager : IDisposable
    {
        void Initialize();
        IEnumerable<IDrawableTracker> GetTrackers();
    }
}
