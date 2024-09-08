using System;

namespace MentorRouletteCounter.DutyTracking
{
    internal class DutyEntry : IEquatable<DutyEntry>
    {
        public DateTime TimeStamp { get; set; }
        public string Name { get; set; }
        public DutyType Type { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public string JobName { get; set; }

        public DutyEntry(DutyType type, string name)
        {
            Name = name;
            Type = type;
        }

        public DutyEntry(DateTime timeStamp, DutyType type, string name, TimeSpan time, string jobName)
        {
            TimeStamp = timeStamp;
            Name = name;
            Type = type;
            ElapsedTime = time;
            JobName = jobName;
        }

        public static DutyEntry FromCsv(string[] csv)
        {
            var timestamp = DateTime.Now;
            if (DateTime.TryParse(csv[0], out var dt))
            {
                timestamp = dt;
            }
            var time = TimeSpan.Zero;
            if (TimeSpan.TryParse(csv[3], out var t))
            {
                time = t;
            }
            return new DutyEntry(timestamp, Enum.Parse<DutyType>(csv[1]), csv[2], time, csv[4]);
        }

        public string AsCsv()
        {
            return $"{TimeStamp},{Type},{Name.Replace(",", ";")},{ElapsedTime},{JobName}";
        }

        public bool Equals(DutyEntry? other)
        {
            if (other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            return Name == other.Name && Type == other.Type && ElapsedTime == other.ElapsedTime && JobName == other.JobName && TimeStamp == other.TimeStamp;
        }
    }
}
