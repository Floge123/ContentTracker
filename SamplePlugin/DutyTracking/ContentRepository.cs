using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorRouletteCounter.DutyTracking
{
    internal static class ContentRepository
    {
        public static IDictionary<uint, string> Dungeons { get; private set; }
        public static IDictionary<uint, string> NormalRaids { get; private set; }
        public static IDictionary<uint, string> SavageRaids { get; private set; }
        public static IDictionary<uint, string> NormalTrials { get; private set; }
        public static IDictionary<uint, string> ExtremeTrials { get; private set; }
        public static IDictionary<uint, string> AllianceRaids { get; private set; }
        public static IDictionary<uint, string> Guildhests { get; private set; }
        public static IDictionary<uint, string> PVP { get; private set; }
        public static IDictionary<uint, string> DoL { get; private set; }
        public static IDictionary<uint, string> DeepDungeons { get; private set; }

        public static void Initialize()
        {
            var all = Service.GameData.GetExcelSheet<ContentFinderCondition>().Where(d => d.ContentType?.Value != null).ToList();

            Dungeons = all.Where(d => d.ContentType.Value.RowId == 2).ToDictionary(d => d.RowId, d => d.Name.RawString);
            var raids = all.Where(d => d.ContentType.Value.RowId == 5 && d.ContentMemberType.Value.TanksPerParty > 1).ToList();
            NormalRaids = raids.Where(d => !d.Name.RawString.Contains("(Savage)") && !d.Name.RawString.Contains("(episch)")).ToDictionary(d => d.RowId, d => d.Name.RawString);
            SavageRaids = raids.Where(d => d.Name.RawString.Contains("(Savage)") || d.Name.RawString.Contains("(episch)")).ToDictionary(d => d.RowId, d => d.Name.RawString);
            NormalTrials = all.Where(d => d.ContentType.Value.RowId == 4
                && !d.Name.RawString.Contains("(Extreme)")
                && !d.Name.RawString.Contains("The Minstrel's Ballad:", StringComparison.OrdinalIgnoreCase)).ToDictionary(d => d.RowId, d => d.Name.RawString);
            ExtremeTrials = all.Where(d => d.ContentType.Value.RowId == 4
                && (d.Name.RawString.Contains("(Extreme)") || d.Name.RawString.Contains("The Minstrel's Ballad:", StringComparison.OrdinalIgnoreCase))).ToDictionary(d => d.RowId, d => d.Name.RawString);
            AllianceRaids = all.Where(d => d.ContentType.Value.RowId == 5 && d.ContentMemberType.Value.TanksPerParty == 1).ToDictionary(d => d.RowId, d => d.Name.RawString);
            Guildhests = all.Where(d => d.ContentType.Value.RowId == 3).ToDictionary(d => d.RowId, d => d.Name.RawString);
            PVP = all.Where(d => d.ContentType.Value.RowId == 6).ToDictionary(d => d.RowId, d => d.Name.RawString);
            DoL = all.Where(d => d.ContentType.Value.RowId == 16).ToDictionary(d => d.RowId, d => d.Name.RawString);
            DeepDungeons = all.Where(d => d.ContentType.Value.RowId == 21).ToDictionary(d => d.RowId, d => d.Name.RawString);
        }

        public static IList<DutyEntry> GetBlankDutyEntyList()
        {
            var list = new List<DutyEntry>();
            Dungeons.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.Dungeon, d.Value, d.Key)));
            NormalRaids.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.NormalRaid, d.Value, d.Key)));
            SavageRaids.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.SavageRaid, d.Value, d.Key)));
            NormalTrials.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.NormalTrial, d.Value, d.Key)));
            ExtremeTrials.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.ExtremeTrial, d.Value, d.Key)));
            AllianceRaids.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.Alliance, d.Value, d.Key)));
            Guildhests.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.Guildhest, d.Value, d.Key)));
            PVP.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.PVP, d.Value, d.Key)));
            DoL.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.DoL, d.Value, d.Key)));
            DeepDungeons.ToList().ForEach(d => list.Add(new DutyEntry(DutyType.DeepDungeon, d.Value, d.Key)));
            return list.DistinctBy(d => new { d.RowId }).Where(d => !string.IsNullOrEmpty(d.Name)).ToList();
        }

        public static DutyType GetContentTypeForDuty(string dutyName)
        {
            if (Dungeons.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.Dungeon;

            if (Guildhests.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.Guildhest;

            if (NormalRaids.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.NormalRaid;

            if (SavageRaids.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.SavageRaid;

            if (NormalTrials.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.NormalTrial;

            if (ExtremeTrials.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.ExtremeTrial;

            if (AllianceRaids.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.Alliance;

            if (PVP.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.PVP;

            if (DoL.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.DoL;

            if (DeepDungeons.Any(d => d.Value.Contains(dutyName, StringComparison.OrdinalIgnoreCase)))
                return DutyType.DeepDungeon;

            throw new NotSupportedException();
        }
    }
}
