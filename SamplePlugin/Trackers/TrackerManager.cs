using System;
using System.Collections.Generic;
using System.Reflection;

namespace MentorRouletteCounter.Trackers
{
    internal class TrackerManager : ITrackerManager
    {
        private readonly List<IDrawableTracker> _trackers = [];

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

        public IEnumerable<IDrawableTracker> GetTrackers() => _trackers;

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
                var tracker = Activator.CreateInstance(item.TrackerType) as IDrawableTracker;
                if (tracker != null)
                    _trackers.Add(tracker);
            }
        }
    }
}
