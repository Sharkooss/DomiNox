using System.Collections.Generic;
using DomiNox.Core;

namespace DomiNox.Run
{
    public sealed class PatternLevelState
    {
        private readonly Dictionary<string, int> levels = new Dictionary<string, int>();

        public IReadOnlyDictionary<string, int> Levels => levels;

        public int GetLevel(string patternId)
        {
            return !string.IsNullOrWhiteSpace(patternId) && levels.TryGetValue(patternId, out var level) ? level : 1;
        }

        public void Increase(string patternId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(patternId) || amount <= 0)
            {
                return;
            }

            levels[patternId] = System.Math.Min(GameConstants.MaxPatternLevel, GetLevel(patternId) + amount);
        }

        public bool IsMaxLevel(string patternId)
        {
            return GetLevel(patternId) >= GameConstants.MaxPatternLevel;
        }
    }
}
