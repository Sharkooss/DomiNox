using System.Collections.Generic;

namespace DomiNox.Run
{
    public sealed class PatternUsageState
    {
        private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

        public IReadOnlyDictionary<string, int> Counts => counts;

        public void Record(IEnumerable<string> patternNames)
        {
            if (patternNames == null)
            {
                return;
            }

            foreach (var patternName in patternNames)
            {
                if (string.IsNullOrWhiteSpace(patternName))
                {
                    continue;
                }

                counts.TryGetValue(patternName, out var current);
                counts[patternName] = current + 1;
            }
        }

        public int GetCount(string patternName)
        {
            return !string.IsNullOrWhiteSpace(patternName) && counts.TryGetValue(patternName, out var count) ? count : 0;
        }

        // Restores an absolute usage count from a save.
        public void Restore(string patternName, int count)
        {
            if (string.IsNullOrWhiteSpace(patternName) || count <= 0)
            {
                return;
            }

            counts[patternName] = count;
        }
    }
}
