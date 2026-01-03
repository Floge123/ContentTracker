using Dalamud.Bindings.ImGui;
using MentorRouletteCounter.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MentorRouletteCounter.Trackers.DutyTracking
{
    internal class DutyVisualization : IDisposable
    {
        private IEnumerable<DutyEntry>? currentEntries;
        private string dutyFilter = string.Empty;
        private string jobFilter = string.Empty;
        private List<DutyDetailsWindows> detailsWindows = new();
        private bool topMentorFilter = false;
        private bool topByTime = false;
        private bool allCharacters = false;
        private List<string> characters = [];

        private Dictionary<DutyType, bool> typeFilters = [];

        public DutyVisualization()
        {
            foreach (var item in Enum.GetValues<DutyType>())
            {
                typeFilters.Add(item, true);
            }
        }


        public void Dispose()
        {
            foreach (var item in detailsWindows)
            {
                Service.WindowSystem.RemoveWindow(item);
                item.Dispose();
            }
        }

        public void Draw(IEnumerable<DutyEntry> entries, IList<DutyEntry> mentorEntries)
        {
            currentEntries = entries;
            characters = [.. entries.Select(x => x.Character).Distinct()];

            DrawTopPage(entries);
            DrawEntries(entries, "All");
            DrawEntries(mentorEntries, "Mentor");
        }

        private void DrawEntries(IEnumerable<DutyEntry> entries, string header)
        {
            if (ImGui.BeginTabItem(header))
            {
                ImGui.InputText("Duty", ref dutyFilter);
                ImGui.InputText("Job", ref jobFilter);
                ImGui.Checkbox("All characters##header", ref allCharacters);

                var filtered = entries.Where(e => e.Name.Contains(dutyFilter, StringComparison.OrdinalIgnoreCase) && (jobFilter == string.Empty || e.JobName.Contains(jobFilter, StringComparison.OrdinalIgnoreCase)));
                DrawTable(filtered);

                ImGui.EndTabItem();
            }
        }

        private void DrawTopPage(IEnumerable<DutyEntry> entries)
        {
            if (ImGui.BeginTabItem("Top Duties"))
            {
                if (ImGui.CollapsingHeader("Filter"))
                {
                    if (ImGui.BeginTable("Duty Type Filter#Table", 4))
                    {
                        foreach (var dutyType in Enum.GetValues<DutyType>())
                        {
                            var flag = typeFilters[dutyType];
                            ImGui.TableNextColumn();
                            ImGui.Checkbox($"{dutyType}##FilterCheckbox", ref flag);
                            typeFilters[dutyType] = flag;
                        }
                        ImGui.EndTable();
                    }
                    ImGui.Separator();
                    ImGui.Checkbox("Only Mentor Duties##FilterCheckbox", ref topMentorFilter);
                    ImGui.SameLine();
                    ImGui.Checkbox("By Time", ref topByTime);
                    ImGui.SameLine();
                    ImGui.Checkbox("All characters##Top", ref allCharacters);
                }

                DrawTopTable(entries);

                ImGui.EndTabItem();
            }
        }

        private void DrawTopTable(IEnumerable<DutyEntry> entries)
        {
            IEnumerable<IGrouping<(string Name, DutyType Type), DutyEntry>> grouped = entries.Where(e => (!topMentorFilter || e.AsMentor) && (allCharacters || e.Character == string.Empty || e.Character == Service.PlayerState.CharacterName)).GroupBy(e => (e.Name, e.Type));
            var filtered = grouped.Where(g => typeFilters[g.Key.Type]);

            if (ImGui.BeginTable("TopEntries", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Duty");
                ImGui.TableSetupColumn("Count");
                ImGui.TableSetupColumn("Earliest");
                ImGui.TableSetupColumn("Latest");
                ImGui.TableSetupColumn("Time");
                ImGui.TableSetupColumn("Actions");
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                int i = 0;
                var ordered = topByTime
                    ? filtered.OrderByDescending(e => e.Sum(e => e.ElapsedTime.TotalSeconds))
                    : filtered.OrderByDescending(e => e.Count());

                foreach (var item in ordered)
                {
                    ImGui.PushID(i++);
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(item.Key.Name);

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{item.Count()}");

                    ImGui.TableSetColumnIndex(2);
                    var sorted = item.OrderByDescending(e => e.TimeStamp);
                    ImGui.Text($"{sorted.LastOrDefault()?.TimeStamp:G}");

                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text($"{sorted.FirstOrDefault()?.TimeStamp:G}");

                    ImGui.TableSetColumnIndex(4);
                    ImGui.Text(FormatTime(TimeSpan.FromSeconds(item.Sum(e => e.ElapsedTime.TotalSeconds))));
                    ImGui.TableSetColumnIndex(5);
                    if (ImGui.Button($"Open Details...##TopTable"))
                    {
                        try
                        {
                            var details = new DutyDetailsWindows(item.Key.Name, item);
                            Service.WindowSystem.AddWindow(details);
                            detailsWindows.Add(details);
                            details.Toggle();
                        }
                        catch (Exception)
                        {
                            //ignore
                        }
                    }
                }

                ImGui.EndTable();
            }
        }

        private string FormatTime(TimeSpan span) => string.Format("{0}hr {1}mn {2}sec",
                     (int)span.TotalHours,
                     span.Minutes,
                     span.Seconds);

        private void DrawTable(IEnumerable<DutyEntry> entries)
        {
            if (ImGui.BeginTable("Entries", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Date");
                ImGui.TableSetupColumn("Duty");
                ImGui.TableSetupColumn("Job");
                ImGui.TableSetupColumn("Time");
                ImGui.TableSetupColumn("Character");
                ImGui.TableSetupColumn("Actions");
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                int i = 0;
                foreach (var item in entries.OrderByDescending(e => e.TimeStamp))
                {
                    ImGui.PushID(i++);
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{item.TimeStamp:G}");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(item.Name);
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text(item.JobName);
                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text(FormatTime(item.ElapsedTime));

                    ImGui.TableSetColumnIndex(4);
                    ImGui.Text(item.Character);
                    ImGui.TableSetColumnIndex(5);
                    if (ImGui.Button($"Open Details..."))
                    {
                        try
                        {
                            var details = new DutyDetailsWindows(item.Name, currentEntries ?? []);
                            Service.WindowSystem.AddWindow(details);
                            detailsWindows.Add(details);
                            details.Toggle();
                        }
                        catch (Exception)
                        {
                            //ignore
                        }
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
