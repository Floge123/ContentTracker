using Lumina.Excel.GeneratedSheets;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MentorRouletteCounter.DutyTracking
{
    internal sealed class DutyTracker
    {
        private static readonly string ExportFlatPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\Export_all.txt";
        private static readonly string ExportFlatMentorRoulettePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\Export_Mentor_all.txt";

        private IList<DutyEntry> _flatDoneDuties;
        private IList<DutyEntry> _flatDoneMentorDuties;
        private DateTime _currentStartTime;
        private DateTime _currentEndTime;

        public DutyTracker()
        {
            _flatDoneDuties = new List<DutyEntry>();
            _flatDoneMentorDuties = new List<DutyEntry>();
            ReadExportedStates();
        }

        public void Start()
        {
            Logger.Log("Duty started");
            _currentStartTime = DateTime.Now;
        }

        public void End(ContentFinderCondition content)
        {
            Logger.Log($"Done Duty {content.Name} ({content.RowId})");
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
            var duty = ContentRepository.GetBlankDutyEntyList().First(d => d.RowId == content.RowId);
            string jobName = Service.Client.LocalPlayer.ClassJob.GameData.Name;
            Logger.Log($"Finished duty '{duty.Name}' in mentor roulette in '{elapsedTime}' as '{jobName}'");
            _flatDoneMentorDuties.Add(new DutyEntry(DateTime.Now, duty.Type, duty.Name, elapsedTime, jobName));
        }

        private void StoreDoneDuty(ContentFinderCondition content, TimeSpan elapsedTime)
        {
            var duty = ContentRepository.GetBlankDutyEntyList().First(d => d.RowId == content.RowId);
            string jobName = Service.Client.LocalPlayer.ClassJob.GameData.Name;
            Logger.Log($"Finished duty '{duty.Name}' in '{elapsedTime}' as '{jobName}'");
            _flatDoneDuties.Add(new DutyEntry(DateTime.Now, duty.Type, duty.Name, elapsedTime, jobName));
        }

        public void ExportAsCsv()
        {
            Export(ExportFlatPath, _flatDoneDuties);
            Export(ExportFlatMentorRoulettePath, _flatDoneMentorDuties);
        }

        private void ReadExportedStates()
        {
            ReadFlatDuties(ExportFlatPath, _flatDoneDuties);
            ReadFlatDuties(ExportFlatMentorRoulettePath, _flatDoneMentorDuties);
        }

        private void ReadFlatDuties(string path, IList<DutyEntry> duties)
        {
            PathHelper.EnsurePathExists(path);
            using var parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                var readDuty = DutyEntry.FromCsv(fields);
                duties.Add(readDuty);
            }
        }

        private void Export(string path, IList<DutyEntry> duties)
        {
            PathHelper.EnsurePathExists(path);
            Logger.Log($"Exporting to {path}");

            using var writer = new StreamWriter(path);
            foreach (var item in duties)
            {
                writer.WriteLine(item.AsCsv());
            }
        }
    }
}
