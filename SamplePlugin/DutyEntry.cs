using System;
using System.Collections.Generic;
using System.Linq;

namespace SamplePlugin
{
    internal struct DutyEntry
    {
        public string Name { get; set; }
        public DutyType Type { get; set; }
        public int Count { get; set; }
        public List<TimeSpan> Times { get; set; }

        public DutyEntry(string name, DutyType type)
        {
            Name = name;
            Type = type;
            Count = 0;
            Times = new List<TimeSpan>();
        }

        public string AsCsv()
        {
            var averageTime = TimeSpan.Zero;
            if (Times.Any())
            {
                averageTime = new TimeSpan(Convert.ToInt64(Times.Average(t => t.Ticks)));
            }
            return $"{Name},{Type},{Count},{averageTime}";
        }
    }
}
