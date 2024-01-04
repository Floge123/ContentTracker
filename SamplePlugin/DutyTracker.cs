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
        private const string ExportPath = "D:\\Bibliothek\\Dokumente\\MentorRoulette\\Export.txt";
        private const string ExportMentorRoulettePath = "D:\\Bibliothek\\Dokumente\\MentorRoulette\\Export_Mentor.txt";

        private IList<DutyEntry> _doneDuties;
        private IList<DutyEntry> _doneMentorDuties;
        private DateTime _currentStartTime;
        private DateTime _currentEndTime;

        public DutyTracker()
        {
            _doneDuties = ContentRepository.GetBlankDutyEntyList();
            _doneMentorDuties = ContentRepository.GetBlankDutyEntyList();
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
            var duty = _doneDuties.First(d => d.Name.Equals(content.Name, StringComparison.OrdinalIgnoreCase));
            duty.Count++;
            duty.Times.Add(elapsedTime);
            Logger.Log($"Finished duty '{duty.Name}' for the '{duty.Count}' time in '{elapsedTime}'");

            if (content.MentorRoulette)
            {
                duty = _doneMentorDuties.First(d => d.Name.Equals(content.Name, StringComparison.OrdinalIgnoreCase));
                duty.Count++;
                duty.Times.Add(elapsedTime);
                Logger.Log($"Finished duty '{duty.Name}' in mentor roulette for the '{duty.Count}' time in '{elapsedTime}'");
            }
        }

        public void ExportAsCsv()
        {
            Export(ExportPath, _doneDuties);
            Export(ExportMentorRoulettePath, _doneMentorDuties);
        }

        private void ReadExportedStates()
        {
            ReadDuties(ExportPath, _doneDuties);
            ReadDuties(ExportMentorRoulettePath, _doneMentorDuties);
        }

        private void ReadDuties(string path, IList<DutyEntry> duties)
        {
            PathHelper.EnsurePathExists(path);
            using TextFieldParser parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData) 
            {
                var fields = parser.ReadFields();
                var readDuty = DutyEntry.FromCsv(fields);
                var duty = duties.First(d => d.Name.Equals(readDuty.Name));
                duty.Count = readDuty.Count;
                duty.Times = readDuty.Times;
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
