using System;

namespace MentorRouletteCounter
{
    internal sealed class GilEntry
    {
        public DateTime Time { get; set; }
        public long Gil { get; set; }

        public GilEntry(DateTime time, long gil)
        {
            Time = time;
            Gil = gil;
        }

        public static GilEntry FromCsv(string[] csv)
        {
            var timestamp = DateTime.Now;
            if (DateTime.TryParse(csv[0], out DateTime dt))
            {
                timestamp = dt;
            }
            return new GilEntry(timestamp, long.Parse(csv[1]));
        }

        public string AsCsv()
        {
            return $"{Time},{Gil}";
        }
    }
}
