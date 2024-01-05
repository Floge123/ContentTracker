using Lumina.Excel.GeneratedSheets;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MentorRouletteCounter
{
    internal sealed class DutyTracker
    {
        private static readonly string ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\Export.txt";
        private static readonly string ExportFlatPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\Export_all.txt";
        private static readonly string ExportMentorRoulettePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\Export_Mentor.txt";
        private static readonly string ExportFlatMentorRoulettePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\Export_Mentor_all.txt";

        private IList<SummaryDutyEntry> _doneDuties;
        private IList<SummaryDutyEntry> _doneMentorDuties;
        private IList<DutyEntry> _flatDoneDuties;
        private IList<DutyEntry> _flatDoneMentorDuties;
        private DateTime _currentStartTime;
        private DateTime _currentEndTime;

        public DutyTracker()
        {
            _doneDuties = ContentRepository.GetBlankDutyEntyList();
            _doneMentorDuties = ContentRepository.GetBlankDutyEntyList();
            _flatDoneDuties = new List<DutyEntry>();
            _flatDoneMentorDuties = new List<DutyEntry>();
            ReadExportedStates();
        }

        public void Start()
        {
            _currentStartTime = DateTime.Now;
        }

        public void End(ContentFinderCondition content)
        {
            _currentEndTime = DateTime.Now;
            var elapsedTime = _currentEndTime - _currentStartTime;
            StoreDoneDuty(content, elapsedTime);

            //Check if the current player is a mentor and add this duty to the mentor duties
            if (Service.Client.LocalPlayer.OnlineStatus.GameData.Name.RawString.Contains("Mentor", StringComparison.OrdinalIgnoreCase))
            {
                StoreDoneMentorDuty(content, elapsedTime);
            }
        }

        private void StoreDoneMentorDuty(ContentFinderCondition content, TimeSpan elapsedTime)
        {
            var duty = _doneMentorDuties.First(d => d.Name.Equals(content.Name, StringComparison.OrdinalIgnoreCase));
            duty.Count++;
            duty.AverageTime.Add(elapsedTime);
            string jobName = Service.Client.LocalPlayer.ClassJob.GameData.Name;
            Logger.Log($"Finished duty '{duty.Name}' in mentor roulette for the '{duty.Count}' time in '{elapsedTime}' as '{jobName}'");
            _flatDoneMentorDuties.Add(new DutyEntry(DateTime.Now, duty.Type, duty.Name, elapsedTime, jobName));
        }

        private void StoreDoneDuty(ContentFinderCondition content, TimeSpan elapsedTime)
        {
            var duty = _doneDuties.First(d => d.Name.Equals(content.Name, StringComparison.OrdinalIgnoreCase));
            duty.Count++;
            duty.AverageTime.Add(elapsedTime);
            string jobName = Service.Client.LocalPlayer.ClassJob.GameData.Name;
            Logger.Log($"Finished duty '{duty.Name}' for the '{duty.Count}' time in '{elapsedTime}' as '{jobName}'");
            _flatDoneDuties.Add(new DutyEntry(DateTime.Now, duty.Type, duty.Name, elapsedTime, jobName));
        }

        public void ExportAsCsv()
        {
            Export(ExportPath, _doneDuties);
            Export(ExportFlatPath, _flatDoneDuties);
            Export(ExportMentorRoulettePath, _doneMentorDuties);
            Export(ExportFlatMentorRoulettePath, _flatDoneMentorDuties);
        }

        private void ReadExportedStates()
        {
            ReadDuties(ExportPath, _doneDuties);
            ReadFlatDuties(ExportFlatPath, _flatDoneDuties);
            ReadDuties(ExportMentorRoulettePath, _doneMentorDuties);
            ReadFlatDuties(ExportFlatMentorRoulettePath, _flatDoneMentorDuties);
        }

        private void ReadDuties(string path, IList<SummaryDutyEntry> duties)
        {
            PathHelper.EnsurePathExists(path);
            using TextFieldParser parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData) 
            {
                var fields = parser.ReadFields();
                var readDuty = SummaryDutyEntry.FromCsv(fields);
                var duty = duties.First(d => d.Name.Equals(readDuty.Name));
                duty.Count = readDuty.Count;
                duty.AverageTime = readDuty.AverageTime;
            }
        }

        private void ReadFlatDuties(string path, IList<DutyEntry> duties)
        {
            PathHelper.EnsurePathExists(path);
            using TextFieldParser parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                var readDuty = DutyEntry.FromCsv(fields);
                duties.Add(readDuty);
            }
        }

        private void Export(string path, IList<SummaryDutyEntry> duties)
        {
            PathHelper.EnsurePathExists(path);
            Logger.Log($"Exporting to {path}");

            using StreamWriter writer = new StreamWriter(path);
            foreach (var item in duties)
            {
                writer.WriteLine(item.AsCsv());
            }
        }

        private void Export(string path, IList<DutyEntry> duties)
        {
            PathHelper.EnsurePathExists(path);
            Logger.Log($"Exporting to {path}");

            using StreamWriter writer = new StreamWriter(path);
            foreach (var item in duties)
            {
                writer.WriteLine(item.AsCsv());
            }
        }
    }
}
