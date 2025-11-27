using Dalamud.Game.Command;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Data.Parsing;
using Lumina.Excel.Sheets;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            AddCommand();           
        }

        private void AddCommand()
        {
            Service.Commands.AddHandler("/duty", new CommandInfo((command, arguments) =>
            {
                if (string.IsNullOrEmpty(arguments))
                {
                    Service.Chat.PrintError("No duty name provided to /duty.");
                    return;
                }

                try
                {
                    Logger.Log(arguments);
                    foreach (var item in GetMatchingDuties(ContentRepository.All.Values.Distinct().ToList(), arguments))
                    {
                        Service.Chat.Print($"Duty Info for {item}");
                        PrintDutyInfo(item);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }
            })
            {
                HelpMessage = "Print info about provided duty.",
                ShowInHelp = true,                
            });
        }

        public void Start(ContentFinderCondition? content)
        {
            if (content != null)
            {
                Logger.Log($"Duty started {content?.Name} ({content?.RowId})");
                PrintDutyInfo(content!.Value.Name.ToString());
            }
            _currentStartTime = DateTime.Now;
        }

        private List<string> GetMatchingDuties(ICollection<string> duties, string query)
        {
            return [.. duties.Where(d => d.Contains(query, StringComparison.OrdinalIgnoreCase))];
        }

        private void PrintDutyInfo(string duty)
        {           
            var duties = _flatDoneDuties.Where(d => d.Name.Equals(duty, StringComparison.OrdinalIgnoreCase)).ToList();
            var mentorDuties = _flatDoneMentorDuties.Where(d => d.Name.Equals(duty, StringComparison.OrdinalIgnoreCase)).ToList();
            //Service.Chat.Print($"Duty started {content?.Name}");
            Service.Chat.Print($"\tTotal: {duties.Count} | Time: {new TimeSpan(duties.Sum(d => d.ElapsedTime.Ticks)):hh':'mm':'ss}");
            Service.Chat.Print($"\tMentor: {mentorDuties.Count} | Time: {new TimeSpan(mentorDuties.Sum(d => d.ElapsedTime.Ticks)):hh':'mm':'ss}");
        }

        public void End(ContentFinderCondition content)
        {
            Logger.Log($"Done Duty {content.Name} ({content.RowId})");           
            Service.Framework.RunOnFrameworkThread(() =>
            {
                _currentEndTime = DateTime.Now;
                var elapsedTime = _currentEndTime - _currentStartTime;
                StoreDoneDuty(content, elapsedTime);

                //Check if the current player is a mentor and add this duty to the mentor duties
                if (Service.Client.LocalPlayer.OnlineStatus.Value.Name.ToString().Contains("Mentor", StringComparison.OrdinalIgnoreCase))
                {
                    StoreDoneMentorDuty(content, elapsedTime);
                }

                PrintDutyInfo(content.Name.ToString());
            });
        }

        private void StoreDoneMentorDuty(ContentFinderCondition content, TimeSpan elapsedTime)
        {
            var duty = ContentRepository.GetBlankDutyEntyList().First(d => d.RowId == content.RowId);
            string jobName = Service.Client.LocalPlayer.ClassJob.Value.Name.ToString();
            Logger.Log($"Finished duty '{duty.Name}' in mentor roulette in '{elapsedTime}' as '{jobName}'");
            _flatDoneMentorDuties.Add(new DutyEntry(DateTime.Now, duty.Type, duty.Name, elapsedTime, jobName));
        }

        private void StoreDoneDuty(ContentFinderCondition content, TimeSpan elapsedTime)
        {
            var duty = ContentRepository.GetBlankDutyEntyList().First(d => d.RowId == content.RowId);
            string jobName = Service.Client.LocalPlayer.ClassJob.Value.Name.ToString();
            Logger.Log($"Finished duty '{duty.Name}' in '{elapsedTime}' as '{jobName}'");
            Service.Chat.Print($"Finished duty '{duty.Name}' in '{elapsedTime}' as '{jobName}'");
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
