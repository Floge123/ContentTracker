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

            DrawEntries(entries, "All");
            DrawEntries(mentorEntries, "Mentor");
        }

        private void DrawEntries(IEnumerable<DutyEntry> entries, string header)
        {
            if (ImGui.BeginTabItem(header))
            {
                ImGui.InputText("Duty", ref dutyFilter);
                ImGui.InputText("Job", ref jobFilter);

                var filtered = entries.Where(e => e.Name.Contains(dutyFilter, StringComparison.OrdinalIgnoreCase) && (jobFilter == string.Empty || e.JobName.Contains(jobFilter, StringComparison.OrdinalIgnoreCase)));
                DrawTable(filtered);

                ImGui.EndTabItem();
            }
        }

        private void DrawTable(IEnumerable<DutyEntry> entries)
        {
            if (ImGui.BeginTable("Entries", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Date");
                ImGui.TableSetupColumn("Duty");
                ImGui.TableSetupColumn("Job");
                ImGui.TableSetupColumn("Time");
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
                    ImGui.Text(item.ElapsedTime.ToString());

                    ImGui.TableSetColumnIndex(4);
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
