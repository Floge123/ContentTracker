using System;

namespace MentorRouletteCounter.PeopleTracking
{
    internal record PeopleEntry(string Name, string Duty, DateTime Time)
    {
        public string AsCsv()
        {
            return $"{Time},{Name},{Duty}";
        }
    }
}
