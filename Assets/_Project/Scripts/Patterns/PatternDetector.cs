using System.Collections.Generic;
using System.Linq;
using DomiNox.Core;
using DomiNox.Grid;

namespace DomiNox.Patterns
{
    public sealed class PatternDetector
    {
        public List<string> DetectPatterns(List<PlacedDomino> placedDominoes)
        {
            return DetectPatterns(placedDominoes, GameConstants.PhaseOneMaxPlacedDominoes);
        }

        public List<string> DetectPatterns(List<PlacedDomino> placedDominoes, int maxPlacedDominoes)
        {
            var patterns = new List<string>();
            if (placedDominoes == null || placedDominoes.Count == 0)
            {
                return patterns;
            }

            var doubleCount = placedDominoes.Count(placed => placed.Domino.Definition.IsDouble);
            var jackpotSevenCount = placedDominoes.Count(placed => placed.Domino.Definition.Sum == 7);
            var allHighStake = placedDominoes.All(placed => placed.Domino.Definition.Sum >= 8);

            if (doubleCount >= 1)
            {
                patterns.Add(PatternNames.DoublePlayed);
            }

            if (doubleCount >= 3)
            {
                patterns.Add(PatternNames.TripleDouble);
            }

            if (jackpotSevenCount >= 3)
            {
                patterns.Add(PatternNames.JackpotSeven);
            }

            if (placedDominoes.Count == maxPlacedDominoes)
            {
                patterns.Add(PatternNames.FullNox);
            }

            if (allHighStake)
            {
                patterns.Add(PatternNames.HighStake);
            }

            return patterns;
        }
    }
}
