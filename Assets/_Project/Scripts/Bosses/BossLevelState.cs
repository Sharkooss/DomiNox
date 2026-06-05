using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Bosses
{
    public sealed class BossLevelState
    {
        public BossDefinition Definition { get; }
        public int? BannedValue { get; set; }
        public HashSet<string> LockedDominoIds { get; } = new HashSet<string>();

        public BossLevelState(BossDefinition definition)
        {
            Definition = definition;
        }

        public string GetEffectSummary()
        {
            switch (Definition.RuleType)
            {
                case BossRuleType.BannedValue:
                    return BannedValue.HasValue
                        ? $"La valeur {BannedValue.Value} est bannie: tout domino qui contient cette valeur est injouable."
                        : "Une valeur sera bannie: les dominos qui la contiennent seront injouables.";
                case BossRuleType.ModifyDiscards:
                    return $"Discards modifies de {Definition.DiscardsDelta}: tu joues ce niveau avec moins de marge de defausse.";
                case BossRuleType.DisablePatterns:
                    return $"Patterns desactives: {string.Join(", ", Definition.DisabledPatternIds.Select(FormatPatternId))}.";
                case BossRuleType.JackpotBoost:
                    return $"Chaque domino de somme 7 donne +{Definition.SevenDominoCountBonus} Count et +{Definition.SevenDominoMultBonus} Mult. Jackpot 7 multiplie le score final par x{Definition.JackpotFinalScoreMultiplier:0.##}.";
                case BossRuleType.LockHandDominoes:
                    return $"{Definition.LockedDominoCount} domino(s) de ta main sont verrouilles: ils ne peuvent pas etre defausses.";
                default:
                    return Definition.Description;
            }
        }

        private static string FormatPatternId(string patternId)
        {
            return string.IsNullOrWhiteSpace(patternId) ? "?" : patternId.Replace('_', ' ');
        }
    }
}
