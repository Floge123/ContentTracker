using System;

namespace MentorRouletteCounter.Trackers.DutyTracking
{
    internal class DutyEntry : IEquatable<DutyEntry>
    {
        public DateTime TimeStamp { get; set; }
        public uint RowId { get; set; }
        public string Name { get; set; }
        public DutyType Type { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public string JobName { get; set; }
        public bool AsMentor { get; set; }

        public DutyEntry(DutyType type, string name, uint id)
        {
            Name = name;
            Type = type;
            RowId = id;
        }

        public DutyEntry(DateTime timeStamp, DutyType type, string name, TimeSpan time, string jobName, bool asMentor)
        {
            TimeStamp = timeStamp;
            Name = name;
            Type = type;
            ElapsedTime = time;
            JobName = jobName;
            AsMentor = asMentor;
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
            bool asMentor = false;
            if (csv.Length > 5 && bool.TryParse(csv[5], out var b))
            {
                asMentor = b;
            }
            return new DutyEntry(timestamp, Enum.Parse<DutyType>(csv[1]), csv[2], time, csv[4], asMentor);
        }

        public string AsCsv() => $"{TimeStamp:s},{Type},{Name.Replace(",", ";")},{ElapsedTime},{JobName},{AsMentor}";

        public bool Equals(DutyEntry? other)
        {
            if (other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            return Name == other.Name && Type == other.Type && ElapsedTime == other.ElapsedTime && JobName == other.JobName && TimeStamp == other.TimeStamp;
        }
    }
}
