using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SamplePlugin
{
    internal static class ContentRepository
    {
        public static List<string> Dungeons { get; private set; }
        public static List<string> NormalRaids { get; private set; }
        public static List<string> NormalTrials { get; private set; }
        public static List<string> ExtremeTrials { get; private set; }
        public static List<string> AllianceRaids { get; private set; }
        public static List<string> Guildhests { get; private set; }

        public static void Initialize()
        {
            var all = Service.GameData.GetExcelSheet<ContentFinderCondition>().Where(d => d.ContentType?.Value != null).ToList();
            Dungeons = all.Where(d => d.ContentType.Value.Name == "Dungeons").Select(d => d.Name.RawString).ToList();
            NormalRaids = all.Where(d => d.ContentType.Value.Name == "Raids" && d.ContentMemberType.Value.TanksPerParty > 1).Select(d => d.Name.RawString).ToList();
            NormalTrials = all.Where(d => d.ContentType.Value.Name == "Trials" && !d.Name.RawString.Contains("(Extreme)")).Select(d => d.Name.RawString).ToList();
            ExtremeTrials = all.Where(d => d.ContentType.Value.Name == "Trials" && d.Name.RawString.Contains("(Extreme)")).Select(d => d.Name.RawString).ToList();
            AllianceRaids = all.Where(d => d.ContentType.Value.Name == "Raids" && d.ContentMemberType.Value.TanksPerParty == 1).Select(d => d.Name.RawString).ToList();
            Guildhests = all.Where(d => d.ContentType.Value.Name == "Guildhests").Select(d => d.Name.RawString).ToList();
        }

        public static IList<DutyEntry> GetBlankDutyEntyList()
        {
            var list = new List<DutyEntry>();
            Dungeons.ForEach(d => list.Add(new DutyEntry(d, GetContentTypeForDuty(d))));
            NormalRaids.ForEach(d => list.Add(new DutyEntry(d, GetContentTypeForDuty(d))));
            NormalTrials.ForEach(d => list.Add(new DutyEntry(d, GetContentTypeForDuty(d))));
            ExtremeTrials.ForEach(d => list.Add(new DutyEntry(d, GetContentTypeForDuty(d))));
            AllianceRaids.ForEach(d => list.Add(new DutyEntry(d, GetContentTypeForDuty(d))));
            Guildhests.ForEach(d => list.Add(new DutyEntry(d, GetContentTypeForDuty(d))));

            return list;
        }

        public static DutyType GetContentTypeForDuty(string dutyName)
        {
            if (Dungeons.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.Dungeon;

            if (Guildhests.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.Guildhest;

            if (NormalRaids.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.NormalRaid;

            if (NormalTrials.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.NormalTrial;

            if (ExtremeTrials.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.ExtremeTrial;

            if (AllianceRaids.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.Alliance;

            throw new NotSupportedException();
        }
    }
}
