using System;

namespace MentorRouletteCounter.Trackers
{
    internal interface ITrackerManager : IDisposable
    {
        void Initialize();        
    }
}
