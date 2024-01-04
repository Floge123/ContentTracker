using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplePlugin
{
    internal class DutyTracker
    {
        private IList<DutyEntry> _doneDuties;
        private DateTime _currentStartTime;
        private DateTime _currentEndTime;

        public DutyTracker()
        {
            _doneDuties = ContentRepository.GetBlankDutyEntyList();
            //TODO: Read from serialized file
        }

        public void Start()
        {
            _currentStartTime = DateTime.Now;
        }

        public void End(ContentFinderCondition content)
        {
            _currentEndTime = DateTime.Now;
            var duty = _doneDuties.First(d => d.Name.Equals(content.Name, StringComparison.OrdinalIgnoreCase));
            duty.Count++;
            duty.Times.Add(_currentEndTime - _currentStartTime);
        }

        public void ExportAsCsv(string path)
        {
            string directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Check if the file exists, create it if it doesn't.
            if (!File.Exists(path))
            {
                using (File.Create(path)) { } // Create the file and immediately close it.
            }

            using StreamWriter writer = new StreamWriter(path);
            foreach (var item in _doneDuties)
            {
                writer.WriteLine(item.AsCsv());
            }
        }
    }
}
