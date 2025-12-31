using System;

namespace MentorRouletteCounter.Trackers
{
    internal interface ITracker : IDisposable
    {
        void Initialize();
    }
}
