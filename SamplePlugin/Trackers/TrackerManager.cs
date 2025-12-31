using System;
using System.Collections.Generic;
using System.Reflection;

namespace MentorRouletteCounter.Trackers
{
    internal class TrackerManager : ITrackerManager
    {
        private readonly List<ITracker> _trackers = [];

        public TrackerManager()
        {
        }

        public void Dispose()
        {
            foreach (var item in _trackers)
            {
                item.Dispose();
            }
        }

        public void Initialize()
        {
            RegisterTrackers();

            foreach (var item in _trackers)
            {
                item.Initialize();
            }
        }

        private void RegisterTrackers()
        {
            var attributes = GetType().Assembly.GetCustomAttributes<TrackerAttribute>();

            foreach (var item in attributes)
            {
                var tracker = Activator.CreateInstance(item.TrackerType) as ITracker;
                if (tracker != null)
                    _trackers.Add(tracker);
            }
        }
    }
}
