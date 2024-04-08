using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorRouletteCounter.DutyTracking
{
    internal sealed class SummaryDutyEntry : IEquatable<SummaryDutyEntry>
    {
        public string Name { get; set; }
        public DutyType Type { get; set; }
        public int Count { get; set; }
        public IList<TimeSpan> AverageTime { get; set; }

        public SummaryDutyEntry(DutyType type, string name)
        {
            Name = name;
            Type = type;
            Count = 0;
            AverageTime = new List<TimeSpan>();
        }

        public static SummaryDutyEntry FromCsv(string[] csv)
        {
            var duty = new SummaryDutyEntry(Enum.Parse<DutyType>(csv[0]), csv[1]);
            duty.Count = int.Parse(csv[2]);
            duty.AverageTime = new List<TimeSpan>();
            if (TimeSpan.TryParse(csv[3], out var time))
            {
                duty.AverageTime.Add(time);
            }
            return duty;
        }

        public string AsCsv()
        {
            var averageTime = TimeSpan.Zero;
            if (AverageTime.Any())
            {
                averageTime = new TimeSpan(Convert.ToInt64(AverageTime.Average(t => t.Ticks)));
            }
            return $"{Type},{Name.Replace(",", ";")},{Count},{averageTime}";
        }

        public bool Equals(SummaryDutyEntry? other)
        {
            if (other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            return Name == other.Name && Type == other.Type;
        }
    }
}
