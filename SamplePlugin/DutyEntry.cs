using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorRouletteCounter
{
    internal sealed class DutyEntry
    {
        public string Name { get; set; }
        public DutyType Type { get; set; }
        public int Count { get; set; }
        public List<TimeSpan> Times { get; set; }

        public DutyEntry(DutyType type, string name)
        {
            Name = name;
            Type = type;
            Count = 0;
            Times = new List<TimeSpan>();
        }

        public static DutyEntry FromCsv(string[] csv)
        {
            var duty = new DutyEntry(Enum.Parse<DutyType>(csv[0]), csv[1]);
            duty.Count = int.Parse(csv[2]);
            duty.Times = new List<TimeSpan>();
            if (TimeSpan.TryParse(csv[3], out TimeSpan time))
            {
                duty.Times.Add(time);
            }
            return duty;
        }

        public string AsCsv()
        {
            var averageTime = TimeSpan.Zero;
            if (Times.Any())
            {
                averageTime = new TimeSpan(Convert.ToInt64(Times.Average(t => t.Ticks)));
            }
            return $"{Type},{Name},{Count},{averageTime}";
        }
    }
}
