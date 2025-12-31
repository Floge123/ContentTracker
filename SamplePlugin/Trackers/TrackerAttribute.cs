using System;

namespace MentorRouletteCounter.Trackers
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class TrackerAttribute : Attribute
    {
        public Type TrackerType { get; }

        public TrackerAttribute(Type trackerType) 
        {
            TrackerType = trackerType;
        }
    }
}
