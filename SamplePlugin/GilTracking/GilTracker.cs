using FFXIVClientStructs.FFXIV.Client.Game;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MentorRouletteCounter.GilTracking
{
    internal class GilTracker
    {
        private static readonly string ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\GilTrack.txt";
        private Timer _timer;
        private TimeSpan _interval;
        private IList<GilEntry> _entries = new List<GilEntry>();

        public GilTracker(TimeSpan interval)
        {
            _interval = interval;
            ExportEntries();
        }

        public void Start()
        {
            _timer = new Timer(Track, null, TimeSpan.Zero, _interval);
        }

        public void Stop()
        {
            _timer.Dispose();
        }


        private void Track(object? state)
        {
            try
            {
                var currentGil = GetRetainerGil() + GetCharGil();
                _entries.Add(new GilEntry(RoundDownToMinute(DateTime.Now), Service.Client.LocalPlayer.Name.TextValue, currentGil));
                ExportEntries();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        private void ExportGil()
        {
            PathHelper.EnsurePathExists(ExportPath);

            using var writer = new StreamWriter(ExportPath);
            foreach (var entry in _entries)
            {
                writer.WriteLine(entry.AsCsv());
            }
        }

        private unsafe long GetRetainerGil()
        {
            var manager = *RetainerManager.Instance();
            var retainerCount = manager.GetRetainerCount();
            if (retainerCount < 2)
            {
                throw new ArgumentException("Retainers not initialized.");
            }

            long gil = 0;
            for (uint i = 0; i < retainerCount; i++)
            {
                var retainer = *manager.GetRetainerBySortedIndex(i);
                gil += retainer.Gil;
            }
            return gil;
        }

        private unsafe long GetCharGil()
        {
            return InventoryManager.Instance()->GetGil();
        }

        private void ReadEntries()
        {
            PathHelper.EnsurePathExists(ExportPath);
            using var parser = new TextFieldParser(ExportPath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                var readDuty = GilEntry.FromCsv(fields);
                _entries.Add(readDuty);
            }
        }

        private void ExportEntries()
        {
            if (Service.Client.LocalPlayer?.Name == null)
                return;

            ReadEntries();

            var currentCharacterEntires = _entries.Where(e => e.CharacterName == Service.Client.LocalPlayer.Name.TextValue).ToArray();

            var lowestTime = currentCharacterEntires.FirstOrDefault()?.Time;
            if (lowestTime == null)
                return;

            var latestTime = currentCharacterEntires.LastOrDefault()?.Time;
            if (latestTime == null)
                return;

            var i = 0;
            GilEntry current;
            for (DateTime? date = lowestTime; date < latestTime; date = date.Value.AddMinutes(5))
            {
                current = currentCharacterEntires[i];
                if (RoundDownToMinute(current.Time) <= RoundDownToMinute(date.Value))
                {
                    //Already tracked
                    i++;                   
                    continue;
                }

                _entries.Add(new GilEntry(RoundDownToMinute(date.Value), Service.Client.LocalPlayer.Name.TextValue, current.Gil));
            }
            _entries = currentCharacterEntires.Concat(_entries).DistinctBy(x => new { x.Time, x.CharacterName }).OrderBy(x => x.Time).ToList();

            ExportGil();

            _entries.Clear();
        }

        private DateTime RoundDownToMinute(DateTime dateTime)
        {
            // Round down to the nearest minute by setting seconds and milliseconds to zero
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
        }
    }
}
