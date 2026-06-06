using System.Collections.Generic;
using DomiNox.Bosses;
using DomiNox.Patterns;

namespace DomiNox.Dominex
{
    public sealed class DomiNexScoringContext
    {
        public IReadOnlyList<DomiNexDefinition> ActiveDomiNex { get; }
        public int Credits { get; }
        public int DiscardsUsed { get; }
        public int DiscardsRemaining { get; }
        public int MaxPlacedDominoes { get; }
        public IReadOnlyList<PatternInfo> DetectedPatterns { get; }
        public IReadOnlyDictionary<string, int> PatternUsageCounts { get; }
        public BossLevelState Boss { get; }

        public DomiNexScoringContext(IReadOnlyList<DomiNexDefinition> activeDomiNex, int credits, int discardsUsed, int discardsRemaining, int maxPlacedDominoes, IReadOnlyList<PatternInfo> detectedPatterns = null, BossLevelState boss = null, IReadOnlyDictionary<string, int> patternUsageCounts = null)
        {
            ActiveDomiNex = activeDomiNex;
            Credits = credits;
            DiscardsUsed = discardsUsed;
            DiscardsRemaining = discardsRemaining;
            MaxPlacedDominoes = maxPlacedDominoes;
            DetectedPatterns = detectedPatterns ?? new List<PatternInfo>();
            PatternUsageCounts = patternUsageCounts ?? new Dictionary<string, int>();
            Boss = boss;
        }

        public DomiNexScoringContext WithDetectedPatterns(IReadOnlyList<PatternInfo> detectedPatterns, BossLevelState boss)
        {
            return new DomiNexScoringContext(ActiveDomiNex, Credits, DiscardsUsed, DiscardsRemaining, MaxPlacedDominoes, detectedPatterns, boss, PatternUsageCounts);
        }
    }
}
