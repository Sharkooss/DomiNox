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
        public BossLevelState Boss { get; }

        public DomiNexScoringContext(IReadOnlyList<DomiNexDefinition> activeDomiNex, int credits, int discardsUsed, int discardsRemaining, int maxPlacedDominoes, IReadOnlyList<PatternInfo> detectedPatterns = null, BossLevelState boss = null)
        {
            ActiveDomiNex = activeDomiNex;
            Credits = credits;
            DiscardsUsed = discardsUsed;
            DiscardsRemaining = discardsRemaining;
            MaxPlacedDominoes = maxPlacedDominoes;
            DetectedPatterns = detectedPatterns ?? new List<PatternInfo>();
            Boss = boss;
        }

        public DomiNexScoringContext WithDetectedPatterns(IReadOnlyList<PatternInfo> detectedPatterns, BossLevelState boss)
        {
            return new DomiNexScoringContext(ActiveDomiNex, Credits, DiscardsUsed, DiscardsRemaining, MaxPlacedDominoes, detectedPatterns, boss);
        }
    }
}
