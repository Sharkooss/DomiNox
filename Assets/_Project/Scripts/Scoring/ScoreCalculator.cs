using System.Collections.Generic;
using System.Linq;
using DomiNox.Dominex;
using DomiNox.Grid;
using DomiNox.Patterns;

namespace DomiNox.Scoring
{
    public sealed class ScoreCalculator
    {
        private readonly PatternDetector patternDetector;
        private readonly DomiNexEffectEngine dominexEffectEngine;

        public ScoreCalculator(PatternDetector patternDetector, DomiNexEffectEngine dominexEffectEngine = null)
        {
            this.patternDetector = patternDetector;
            this.dominexEffectEngine = dominexEffectEngine ?? new DomiNexEffectEngine();
        }

        public ScoreResult Calculate(List<PlacedDomino> placedDominoes, int maxPlacedDominoes)
        {
            return Calculate(placedDominoes, maxPlacedDominoes, null);
        }

        public ScoreResult Calculate(List<PlacedDomino> placedDominoes, int maxPlacedDominoes, DomiNexScoringContext dominexContext)
        {
            placedDominoes = placedDominoes ?? new List<PlacedDomino>();

            var count = placedDominoes.Sum(placed => placed.Domino.Definition.Sum);
            var mult = 1;
            var breakdown = new List<string> { $"Count de base: {count}", "Mult de base: 1" };
            var patterns = patternDetector.DetectPatterns(placedDominoes, maxPlacedDominoes);
            var doubleCount = placedDominoes.Count(placed => placed.Domino.Definition.IsDouble);

            if (patterns.Contains(PatternNames.DoublePlayed))
            {
                var bonus = doubleCount * 2;
                mult += bonus;
                breakdown.Add($"Double joue: +{bonus} Mult");
            }

            if (patterns.Contains(PatternNames.TripleDouble))
            {
                mult += 8;
                breakdown.Add("Triple double: +8 Mult");
            }

            if (patterns.Contains(PatternNames.JackpotSeven))
            {
                mult += 10;
                breakdown.Add("Jackpot 7: +10 Mult");
            }

            if (patterns.Contains(PatternNames.FullNox))
            {
                mult += 5;
                breakdown.Add("Full Nox: +5 Mult");
            }

            if (patterns.Contains(PatternNames.HighStake))
            {
                count += 30;
                breakdown.Add("Haute mise: +30 Count");
            }

            dominexEffectEngine.ApplyScoringEffects(placedDominoes, dominexContext, ref count, ref mult, breakdown);

            breakdown.Add($"Score final: {count} x {mult} = {count * mult}");
            return new ScoreResult(count, mult, patterns, breakdown);
        }
    }
}
