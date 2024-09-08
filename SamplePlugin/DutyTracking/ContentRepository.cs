using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorRouletteCounter.DutyTracking
{
    internal static class ContentRepository
    {
        public static List<string> Dungeons { get; private set; }
        public static List<string> NormalRaids { get; private set; }
        public static List<string> SavageRaids { get; private set; }
        public static List<string> NormalTrials { get; private set; }
        public static List<string> ExtremeTrials { get; private set; }
        public static List<string> AllianceRaids { get; private set; }
        public static List<string> Guildhests { get; private set; }
        public static List<string> PVP { get; private set; }
        public static List<string> DoL { get; private set; }
        public static List<string> DeepDungeons { get; private set; }

        public static void Initialize()
        {
            var all = Service.GameData.GetExcelSheet<ContentFinderCondition>().Where(d => d.ContentType?.Value != null).ToList();

            Dungeons = all.Where(d => d.ContentType.Value.Name == "Dungeons").Select(d => d.Name.RawString).ToList();
            var raids = all.Where(d => d.ContentType.Value.Name == "Raids" && d.ContentMemberType.Value.TanksPerParty > 1).ToList();
            NormalRaids = raids.Where(d => !d.Name.RawString.Contains("(Savage)")).Select(d => d.Name.RawString).ToList();
            SavageRaids = raids.Where(d => d.Name.RawString.Contains("(Savage)")).Select(d => d.Name.RawString).ToList();
            NormalTrials = all.Where(d => d.ContentType.Value.Name == "Trials"
                && !d.Name.RawString.Contains("(Extreme)")
                && !d.Name.RawString.Contains("The Minstrel's Ballad:", StringComparison.OrdinalIgnoreCase)).Select(d => d.Name.RawString).ToList();
            ExtremeTrials = all.Where(d => d.ContentType.Value.Name == "Trials"
                && (d.Name.RawString.Contains("(Extreme)") || d.Name.RawString.Contains("The Minstrel's Ballad:", StringComparison.OrdinalIgnoreCase))).Select(d => d.Name.RawString).ToList();
            AllianceRaids = all.Where(d => d.ContentType.Value.Name == "Raids" && d.ContentMemberType.Value.TanksPerParty == 1).Select(d => d.Name.RawString).ToList();
            Guildhests = all.Where(d => d.ContentType.Value.Name == "Guildhests").Select(d => d.Name.RawString).ToList();
            PVP = all.Where(d => d.ContentType.Value.Name == "PvP").Select(d => d.Name.RawString).ToList();
            DoL = all.Where(d => d.ContentType.Value.Name == "Disciples of the Land").Select(d => d.Name.RawString).ToList();
            DeepDungeons = all.Where(d => d.ContentType.Value.Name == "Deep Dungeons").Select(d => d.Name.RawString).ToList();
        }

        public static IList<DutyEntry> GetBlankDutyEntyList()
        {
            var list = new List<DutyEntry>();
            Dungeons.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            NormalRaids.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            SavageRaids.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            NormalTrials.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            ExtremeTrials.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            AllianceRaids.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            Guildhests.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            PVP.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            DoL.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            DeepDungeons.ForEach(d => list.Add(new DutyEntry(GetContentTypeForDuty(d), d)));
            return list.DistinctBy(d => new { d.Name, d.Type }).Where(d => !string.IsNullOrEmpty(d.Name)).ToList();
        }

        public static DutyType GetContentTypeForDuty(string dutyName)
        {
            if (Dungeons.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.Dungeon;

            if (Guildhests.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.Guildhest;

            if (NormalRaids.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.NormalRaid;

            if (SavageRaids.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.SavageRaid;

            if (NormalTrials.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.NormalTrial;

            if (ExtremeTrials.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.ExtremeTrial;

            if (AllianceRaids.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.Alliance;

            if (PVP.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.PVP;

            if (DoL.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.DoL;

            if (DeepDungeons.Any(d => d.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.DeepDungeon;

            throw new NotSupportedException();
        }
    }
}
