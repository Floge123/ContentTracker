using FFXIVClientStructs.FFXIV.Client.Game.Group;
using Lumina.Excel.Sheets;
using MentorRouletteCounter.Trackers;
using MentorRouletteCounter.Trackers.PeopleTracking;
using System;
using System.IO;

[assembly: Tracker(typeof(PeopleTracker))]

namespace MentorRouletteCounter.Trackers.PeopleTracking
{
    internal class PeopleTracker : ITracker
    {
        private static readonly string ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\PersonTrack.txt";

        public void Dispose()
        {
            Service.Duty.DutyCompleted -= Duty_DutyCompleted;
        }

        public void Initialize()
        {
            Service.Duty.DutyCompleted += Duty_DutyCompleted;
        }

        private void Duty_DutyCompleted(object? sender, ushort e)
        {
            try
            {
                var territory = Service.GameData.Excel.GetSheet<TerritoryType>()?.GetRow(e);
                var content = territory?.ContentFinderCondition.Value;
                if (content is null)
                    return;

                Track(content.Value);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private unsafe void Track(ContentFinderCondition content) => Service.Framework.RunOnFrameworkThread(() =>
                                                                             {
                                                                                 Span<PartyMember> members = GroupManager.Instance()->MainGroup.PartyMembers;
                                                                                 using var writer = new StreamWriter(ExportPath, true);
                                                                                 PathHelper.EnsurePathExists(ExportPath);
                                                                                 foreach (var member in members)
                                                                                 {
                                                                                     var name = member.NameString;
                                                                                     if (name == Service.PlayerState.CharacterName || string.IsNullOrEmpty(name))
                                                                                         continue;
                                                                                     writer.WriteLine(new PeopleEntry(name, content.Name.ToString(), DateTime.Now).AsCsv());
                                                                                 }
                                                                             });
    }
}
