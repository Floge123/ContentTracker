using FFXIVClientStructs.FFXIV.Client.Game.Group;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentorRouletteCounter.PeopleTracking
{
    internal class PeopleTracker
    {
        private static readonly string ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\PersonTrack.txt";

        public unsafe void Track(ContentFinderCondition content)
        {
            Span<PartyMember> members = GroupManager.Instance()->MainGroup.PartyMembers;
            using var writer = new StreamWriter(ExportPath, true);
            PathHelper.EnsurePathExists(ExportPath);
            foreach (var member in members)
            {
                var name = member.NameString;
                if (name == Service.Client.LocalPlayer.Name.TextValue || string.IsNullOrEmpty(name))
                    continue;
                writer.WriteLine(new PeopleEntry(name, content.Name.ToString(), DateTime.Now).AsCsv());
            }
        }
    }
}
