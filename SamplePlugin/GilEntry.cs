using System;

namespace MentorRouletteCounter
{
    internal sealed class GilEntry
    {
        public DateTime Time { get; set; }
        public long Gil { get; set; }
        public string CharacterName { get; set; }

        public GilEntry(DateTime time, string characterName, long gil)
        {
            Time = time;
            Gil = gil;
            CharacterName = characterName;
        }

        public static GilEntry FromCsv(string[] csv)
        {
            var timestamp = DateTime.Now;
            if (DateTime.TryParse(csv[0], out DateTime dt))
            {
                timestamp = dt;
            }
            return new GilEntry(timestamp, csv[1], long.Parse(csv[2]));
        }

        public string AsCsv()
        {
            return $"{Time},{CharacterName},{Gil}";
        }
    }
}
