using Lumina.Excel.GeneratedSheets;

namespace MentorRouletteCounter
{
    internal static class ContentFinderConditionExtensions
    {
        public static bool IsMentorRoulette(this ContentFinderCondition cfc)
        {
            return !(cfc.ExpertRoulette
                || cfc.HighLevelRoulette
                || cfc.LevelCapRoulette
                || cfc.LevelingRoulette
                || cfc.TrialRoulette
                || cfc.MSQRoulette
                || cfc.GuildHestRoulette
                || cfc.AllianceRoulette
                || cfc.NormalRaidRoulette);
        }
    }
}
