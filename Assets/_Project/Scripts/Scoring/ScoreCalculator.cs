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
            var detectedPatternInfos = patternDetector.DetectPatternInfos(placedDominoes, maxPlacedDominoes);
            var patterns = detectedPatternInfos.Select(pattern => pattern.Name).ToList();

            foreach (var pattern in detectedPatternInfos)
            {
                count += pattern.ChipsBonus;
                mult += pattern.MultBonus;
                breakdown.Add($"{pattern.Name}: +{pattern.ChipsBonus} Count, +{pattern.MultBonus} Mult");
            }

            dominexEffectEngine.ApplyScoringEffects(placedDominoes, dominexContext, ref count, ref mult, breakdown);

            breakdown.Add($"Score final: {count} x {mult} = {count * mult}");
            return new ScoreResult(count, mult, patterns, breakdown);
        }
    }
}
