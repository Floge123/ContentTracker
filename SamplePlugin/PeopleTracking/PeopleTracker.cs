using FFXIVClientStructs.FFXIV.Client.Game.Group;
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

        public unsafe void Track()
        {
            Span<PartyMember> members = new(GroupManager.Instance()->PartyMembers, GroupManager.Instance()->MemberCount);
            using var writer = new StreamWriter(ExportPath);
            PathHelper.EnsurePathExists(ExportPath);
            foreach (var member in members)
            {
                var name = ReadString(member.Name);
                if (name == Service.Client.LocalPlayer.Name.TextValue)
                    continue;
                writer.WriteLine(name);
            }
        }

        public static unsafe string ReadString(byte* ptr)
        {
            int length = 0;
            byte* currentPtr = ptr;

            // Iterate until the string terminating character is encountered
            while (*currentPtr != 0)
            {
                length++;
                currentPtr++;
            }

            // Convert the byte array to a string using UTF-8 encoding
            return Encoding.UTF8.GetString(ptr, length);
        }
    }
}
