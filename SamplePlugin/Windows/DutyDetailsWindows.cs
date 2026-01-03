using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using MentorRouletteCounter.Trackers;
using MentorRouletteCounter.Trackers.DutyTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MentorRouletteCounter.Windows
{
    internal class DutyDetailsWindows : Window
    {
        private string jobFilter = string.Empty;
        private bool mentorFilter = false;
        private bool allCharacters = false;

        public string Duty { get; init; }
        public IEnumerable<DutyEntry> Entries { get; init; }

        // We give this window a hidden ID using ##.
        // The user will see "My Amazing Window" as window title,
        // but for ImGui the ID is "My Amazing Window##With a hidden ID"
        public DutyDetailsWindows(string name, IEnumerable<DutyEntry> entries)
            : base($"Duty Details for '{name}'", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            Duty = name;
            Entries = entries.Where(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).OrderByDescending(e => e.TimeStamp);
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(700, 330),
                MaximumSize = new Vector2(float.MaxValue, 700)
            };
        }

        public override void OnClose()
        {
            base.OnClose();

            Service.WindowSystem.RemoveWindow(this);
        }

        public void Dispose() 
        {
        }

        public override void Draw()
        {
            if (ImGui.BeginTabBar($"{Duty}_DetailsBar"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    ImGui.Text($"Total runs: {Entries.Count()}");
                    ImGui.Text($"Latest run: {DrawDutyEntry(Entries.FirstOrDefault())}");
                    ImGui.Text($"Earliest run: {DrawDutyEntry(Entries.LastOrDefault())}");
                    var orderedByTime = Entries.OrderBy(e => e.ElapsedTime);
                    ImGui.Text($"Fastest run: {DrawDutyEntry(orderedByTime.FirstOrDefault())}");
                    ImGui.Text($"Slowest run: {DrawDutyEntry(orderedByTime.LastOrDefault())}");


                    ImGui.Separator();

                    var mentors = Entries.Where(e => e.AsMentor);
                    ImGui.Text($"Total mentor runs: {mentors.Count()}");
                    ImGui.Text($"Latest mentor run: {DrawDutyEntry(mentors.FirstOrDefault())}");
                    ImGui.Text($"Earliest mentor run: {DrawDutyEntry(mentors.LastOrDefault())}");
                    var mentorsOrdered = mentors.OrderBy(e => e.ElapsedTime);
                    ImGui.Text($"Fastest mentor run: {DrawDutyEntry(mentorsOrdered.FirstOrDefault())}");
                    ImGui.Text($"Slowest mentor run: {DrawDutyEntry(mentorsOrdered.LastOrDefault())}");

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Entries"))
                {
                    ImGui.InputText("Job", ref jobFilter);
                    ImGui.SameLine();
                    ImGui.Checkbox("Only Mentor", ref mentorFilter);
                    ImGui.SameLine();
                    ImGui.Checkbox("All Characters", ref allCharacters);

                    var filtered = Entries.Where(e => (jobFilter == string.Empty || e.JobName.Contains(jobFilter, StringComparison.OrdinalIgnoreCase)) && (!mentorFilter || e.AsMentor) && (allCharacters || e.Character == string.Empty || e.Character == Service.PlayerState.CharacterName));
                    DrawTable(filtered);
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }

        }

        private string DrawDutyEntry(DutyEntry? entry)
        {
            if (entry is null)
                return string.Empty;

            return $"{entry.TimeStamp:G} as {entry.JobName} in {entry.ElapsedTime}";
        }

        private void DrawTable(IEnumerable<DutyEntry> entries)
        {
            if (ImGui.BeginTable("Entries", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Date");
                ImGui.TableSetupColumn("Duty");
                ImGui.TableSetupColumn("Job");
                ImGui.TableSetupColumn("Time");
                ImGui.TableSetupColumn("Character");
                ImGui.TableSetupColumn("As Mentor?");
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                foreach (var item in entries.OrderByDescending(e => e.TimeStamp))
                {
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
                    ImGui.Text(item.Character);

                    ImGui.TableSetColumnIndex(5);
                    ImGui.Text(item.AsMentor ? "Yes" : "No");
                }

                ImGui.EndTable();
            }
        }
    }
}
