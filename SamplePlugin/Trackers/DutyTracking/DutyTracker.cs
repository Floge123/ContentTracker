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
    internal sealed class DutyTracker : IDrawableTracker
    {
        private static readonly string ExportFlatPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\Export_all.txt";
        private static readonly string ExportFlatMentorRoulettePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\Export_Mentor_all.txt";
        private readonly DutyVisualization dutyVisualization = new();
        private IList<DutyEntry> flatDoneDuties;
        private IList<DutyEntry> flatDoneMentorDuties;
        private DateTime currentStartTime;
        private DateTime currentEndTime;

        public DutyTracker()
        {
            flatDoneDuties = new List<DutyEntry>();
            flatDoneMentorDuties = new List<DutyEntry>();
            
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

        public void Draw()
        {
            dutyVisualization?.Draw(flatDoneDuties, flatDoneMentorDuties);
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
            currentStartTime = DateTime.Now;
        }

        private List<string> GetMatchingDuties(ICollection<string> duties, string query) => [.. duties.Where(d => d.Contains(query, StringComparison.OrdinalIgnoreCase))];

        private void PrintDutyInfo(string duty)
        {
            PrintDutyInfo(flatDoneDuties, duty, "Total");
            PrintDutyInfo(flatDoneMentorDuties, duty, "Mentor");
        }

        private void PrintDutyInfo(IList<DutyEntry> duties, string duty, string prefix)
        {
            var filtered = duties.Where(d => d.Name.Equals(duty, StringComparison.OrdinalIgnoreCase)).OrderByDescending(d => d.TimeStamp).ToList();
            var latest = filtered.Select(d => d.TimeStamp).FirstOrDefault();
            var earliest = filtered.Select(d => d.TimeStamp).LastOrDefault();
            Service.Chat.Print($"\t{prefix}: {filtered.Count} | Time: {FormatTime(new TimeSpan(filtered.Sum(d => d.ElapsedTime.Ticks)))}");

            if (earliest != default)
                Service.Chat.Print($"\t\tEarliest: {earliest:G}");

            if (latest != default)
                Service.Chat.Print($"\t\tLatest: {latest:G}");
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
                currentEndTime = DateTime.Now;
                var elapsedTime = currentEndTime - currentStartTime;
                if (elapsedTime.TotalMinutes > 180)
                    elapsedTime = TimeSpan.Zero;

                bool asMentor = Service.Client.LocalPlayer.OnlineStatus.Value.Name.ToString().Contains("Mentor", StringComparison.OrdinalIgnoreCase);
                StoreDoneDuty(content, elapsedTime, asMentor);

                PrintDutyInfo(content.Name.ToString());
            });
        }

        private void StoreDoneDuty(ContentFinderCondition content, TimeSpan elapsedTime, bool asMentor)
        {
            var duty = ContentRepository.GetBlankDutyEntyList().First(d => d.RowId == content.RowId);
            string jobName = Service.PlayerState.ClassJob.Value.Name.ToString();
            var character = Service.PlayerState.CharacterName;
            Logger.Log($"Finished duty '{duty.Name}' in '{elapsedTime}' as '{jobName}'");
            Service.Chat.Print($"Finished duty '{duty.Name}' in '{elapsedTime}' as '{jobName}'");
            flatDoneDuties.Add(new DutyEntry(DateTime.Now, duty.Type, duty.Name, elapsedTime, jobName, asMentor, character));

            if (asMentor)
                flatDoneMentorDuties.Add(new DutyEntry(DateTime.Now, duty.Type, duty.Name, elapsedTime, jobName, true, character));
        }

        private void ExportAsCsv()
        {
            Export(ExportFlatPath, flatDoneDuties);
            Export(ExportFlatMentorRoulettePath, flatDoneMentorDuties);
        }

        private void ReadExportedStates()
        {
            ReadFlatDuties(ExportFlatMentorRoulettePath, flatDoneMentorDuties, true);
            ReadFlatDuties(ExportFlatPath, flatDoneDuties, false);
        }

        private void ReadFlatDuties(string path, IList<DutyEntry> duties, bool asMentor)
        {
            PathHelper.EnsurePathExists(path);
            using var parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                var readDuty = DutyEntry.FromCsv(fields);
                if (asMentor || flatDoneMentorDuties.Contains(readDuty))
                    readDuty.AsMentor = true;

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
