using Dalamud.Game.Command;
using Lumina.Excel.Sheets;
using MentorRouletteCounter.Trackers;
using MentorRouletteCounter.Trackers.DutyTracking;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[assembly: Tracker(typeof(DutyTracker))]

namespace MentorRouletteCounter.Trackers.DutyTracking
{
    internal sealed class DutyTracker : ITracker
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
            
            AddCommand();
        }

        public void Initialize()
        {
            ContentRepository.Initialize();
            ReadExportedStates();

            Service.Duty.DutyStarted += Duty_DutyStarted;
            Service.Duty.DutyCompleted += Duty_DutyCompleted;
        }

        public void Dispose()
        {
            Service.Duty.DutyStarted -= Duty_DutyStarted;
            Service.Duty.DutyCompleted -= Duty_DutyCompleted;
        }

        private void AddCommand() => Service.Commands.AddHandler("/duty", new CommandInfo((command, arguments) =>
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

        private void Duty_DutyStarted(object? sender, ushort e)
        {
            var territory = Service.GameData.Excel.GetSheet<TerritoryType>()?.GetRow(e);
            var content = territory?.ContentFinderCondition.Value;
            Start(content);
        }

        private void Duty_DutyCompleted(object? sender, ushort e)
        {
            try
            {
                var territory = Service.GameData.Excel.GetSheet<TerritoryType>()?.GetRow(e);
                var content = territory?.ContentFinderCondition.Value;
                if (content is null)
                    return;

                End(content.Value);
                ExportAsCsv();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private void Start(ContentFinderCondition? content)
        {
            if (content != null)
            {
                Logger.Log($"Duty started {content?.Name} ({content?.RowId})");
                PrintDutyInfo(content!.Value.Name.ToString());
            }
            _currentStartTime = DateTime.Now;
        }

        private List<string> GetMatchingDuties(ICollection<string> duties, string query) => [.. duties.Where(d => d.Contains(query, StringComparison.OrdinalIgnoreCase))];

        private void PrintDutyInfo(string duty)
        {
            PrintDutyInfo(_flatDoneDuties, duty, "Total");
            PrintDutyInfo(_flatDoneMentorDuties, duty, "Mentor");
        }

        private void PrintDutyInfo(IList<DutyEntry> duties, string duty, string prefix)
        {
            var filtered = duties.Where(d => d.Name.Equals(duty, StringComparison.OrdinalIgnoreCase)).OrderByDescending(d => d.TimeStamp).ToList();
            var latest = filtered.Select(d => d.TimeStamp).FirstOrDefault();
            var earliest = filtered.Select(d => d.TimeStamp).LastOrDefault();
            Service.Chat.Print($"\t{prefix}: {filtered.Count} | Time: {FormatTime(new TimeSpan(filtered.Sum(d => d.ElapsedTime.Ticks)))}");

            if (earliest != default)
                Service.Chat.Print($"\t\tEarliest: {earliest}");

            if (latest != default)
                Service.Chat.Print($"\t\tLatest: {latest}");
        }

        private string FormatTime(TimeSpan span) => string.Format("{0}hr {1}mn {2}sec",
                     (int)span.TotalHours,
                     span.Minutes,
                     span.Seconds);

        private void End(ContentFinderCondition content)
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
            string jobName = Service.PlayerState.ClassJob.Value.Name.ToString();
            Logger.Log($"Finished duty '{duty.Name}' in mentor roulette in '{elapsedTime}' as '{jobName}'");
            _flatDoneMentorDuties.Add(new DutyEntry(DateTime.Now, duty.Type, duty.Name, elapsedTime, jobName));
        }

        private void StoreDoneDuty(ContentFinderCondition content, TimeSpan elapsedTime)
        {
            var duty = ContentRepository.GetBlankDutyEntyList().First(d => d.RowId == content.RowId);
            string jobName = Service.PlayerState.ClassJob.Value.Name.ToString();
            Logger.Log($"Finished duty '{duty.Name}' in '{elapsedTime}' as '{jobName}'");
            Service.Chat.Print($"Finished duty '{duty.Name}' in '{elapsedTime}' as '{jobName}'");
            _flatDoneDuties.Add(new DutyEntry(DateTime.Now, duty.Type, duty.Name, elapsedTime, jobName));
        }

        private void ExportAsCsv()
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
