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
        public IReadOnlyDictionary<string, int> PatternLevels { get; }
        public int ActiveDomiNexCount { get; }
        public int MaxDomiNexSlots { get; }
        public int BagDoubleCount { get; }
        public int JackpotMeter { get; }
        public int MaxJackpotMeter { get; }
        public int JackpotSpinTickets { get; }
        public int MachineHeat { get; }
        public BossLevelState Boss { get; }

        public DomiNexScoringContext(IReadOnlyList<DomiNexDefinition> activeDomiNex, int credits, int discardsUsed, int discardsRemaining, int maxPlacedDominoes, IReadOnlyList<PatternInfo> detectedPatterns = null, BossLevelState boss = null, IReadOnlyDictionary<string, int> patternUsageCounts = null, IReadOnlyDictionary<string, int> patternLevels = null, int activeDomiNexCount = 0, int maxDomiNexSlots = 0, int bagDoubleCount = 0, int jackpotMeter = 0, int maxJackpotMeter = 0, int jackpotSpinTickets = 0, int machineHeat = 0)
        {
            ActiveDomiNex = activeDomiNex;
            Credits = credits;
            DiscardsUsed = discardsUsed;
            DiscardsRemaining = discardsRemaining;
            MaxPlacedDominoes = maxPlacedDominoes;
            DetectedPatterns = detectedPatterns ?? new List<PatternInfo>();
            PatternUsageCounts = patternUsageCounts ?? new Dictionary<string, int>();
            PatternLevels = patternLevels ?? new Dictionary<string, int>();
            ActiveDomiNexCount = activeDomiNexCount;
            MaxDomiNexSlots = maxDomiNexSlots;
            BagDoubleCount = bagDoubleCount;
            JackpotMeter = jackpotMeter;
            MaxJackpotMeter = maxJackpotMeter;
            JackpotSpinTickets = jackpotSpinTickets;
            MachineHeat = machineHeat;
            Boss = boss;
        }

        public DomiNexScoringContext WithDetectedPatterns(IReadOnlyList<PatternInfo> detectedPatterns, BossLevelState boss)
        {
            return new DomiNexScoringContext(ActiveDomiNex, Credits, DiscardsUsed, DiscardsRemaining, MaxPlacedDominoes, detectedPatterns, boss, PatternUsageCounts, PatternLevels, ActiveDomiNexCount, MaxDomiNexSlots, BagDoubleCount, JackpotMeter, MaxJackpotMeter, JackpotSpinTickets, MachineHeat);
        }

        public int GetPatternLevel(string patternId)
        {
            return !string.IsNullOrWhiteSpace(patternId) && PatternLevels.TryGetValue(patternId, out var level) ? level : 1;
        }
    }
}
