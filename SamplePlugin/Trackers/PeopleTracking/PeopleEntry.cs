using System;

namespace MentorRouletteCounter.Trackers.PeopleTracking
{
    internal record PeopleEntry(string Name, string Duty, DateTime Time)
    {
        public string AsCsv() => $"{Time},{Name},{Duty}";
    }
}
