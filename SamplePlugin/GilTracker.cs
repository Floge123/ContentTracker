using FFXIVClientStructs.FFXIV.Client.Game;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MentorRouletteCounter
{
    internal class GilTracker
    {
        private static readonly string ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\GilTrack.txt";
        private Timer _timer;
        private TimeSpan _interval;
        private IList<GilEntry> _entries;

        public GilTracker(TimeSpan interval)
        {
            _interval = interval;
            ReadEntries();
            FillMissingEntries();
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
                long currentGil = GetRetainerGil() + GetCharGil();
                _entries.Add(new GilEntry(RoundDownToMinute(DateTime.Now), currentGil));
                FillMissingEntries();
                ExportGil();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        private void ExportGil()
        {
            PathHelper.EnsurePathExists(ExportPath);

            using StreamWriter writer = new StreamWriter(ExportPath);
            foreach (var entry in _entries)
            {
                writer.WriteLine(entry.AsCsv());               
            }
        }

        private unsafe long GetRetainerGil()
        {
            RetainerManager manager = *RetainerManager.Instance();
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
            _entries = new List<GilEntry>();
            PathHelper.EnsurePathExists(ExportPath);
            using TextFieldParser parser = new TextFieldParser(ExportPath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");           
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                var readDuty = GilEntry.FromCsv(fields);
                _entries.Add(readDuty);
            }
        }

        private void FillMissingEntries()
        {
            var lowestTime = _entries.FirstOrDefault()?.Time;
            if (lowestTime == null)
                return;

            var latestTime = _entries.LastOrDefault()?.Time;
            if (latestTime == null)
                return;

            int i = 0;
            GilEntry current;
            for (DateTime? date = lowestTime; date != latestTime; date = date.Value.AddMinutes(1))
            {
                current = _entries[i];
                if (RoundDownToMinute(current.Time) == RoundDownToMinute(date.Value))
                {
                    //Already tracked
                    i++;
                    continue;
                }

                _entries.Add(new GilEntry(RoundDownToMinute(date.Value), current.Gil));
            }
            _entries = _entries.OrderBy(x => x.Time).ToList();
        }

        private DateTime RoundDownToMinute(DateTime dateTime)
        {
            // Round down to the nearest minute by setting seconds and milliseconds to zero
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
        }
    }
}
