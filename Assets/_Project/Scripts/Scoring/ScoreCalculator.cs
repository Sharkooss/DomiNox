using System.Collections.Generic;
using System.Linq;
using DomiNox.Bosses;
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
            return Calculate(placedDominoes, maxPlacedDominoes, dominexContext, null);
        }

        public ScoreResult Calculate(List<PlacedDomino> placedDominoes, int maxPlacedDominoes, DomiNexScoringContext dominexContext, BossLevelState boss)
        {
            placedDominoes = placedDominoes ?? new List<PlacedDomino>();

            var count = placedDominoes.Sum(placed => placed.Domino.Definition.Sum);
            var mult = 1;
            var breakdown = new List<string> { $"Count de base: {count}", "Mult de base: 1" };
            var disabledPatternIds = boss?.Definition.RuleType == BossRuleType.DisablePatterns ? boss.Definition.DisabledPatternIds : null;
            var detectedPatternInfos = patternDetector.DetectPatternInfos(placedDominoes, maxPlacedDominoes, disabledPatternIds);
            var patterns = detectedPatternInfos.Select(pattern => pattern.Name).ToList();

            foreach (var pattern in detectedPatternInfos)
            {
                count += pattern.ChipsBonus;
                mult += pattern.MultBonus;
                breakdown.Add($"{pattern.Name}: +{pattern.ChipsBonus} Count, +{pattern.MultBonus} Mult");
            }

            var finalScoreMultiplier = 1f;
            if (boss?.Definition.RuleType == BossRuleType.JackpotBoost)
            {
                var sevenCount = placedDominoes.Count(placed => placed.Domino.Definition.Sum == 7);
                var countBonus = sevenCount * boss.Definition.SevenDominoCountBonus;
                var multBonus = sevenCount * boss.Definition.SevenDominoMultBonus;
                if (countBonus != 0 || multBonus != 0)
                {
                    count += countBonus;
                    mult += multBonus;
                    breakdown.Add($"Boss - Somme 7: +{countBonus} Count, +{multBonus} Mult");
                }

                if (patterns.Contains(PatternNames.JackpotSeven))
                {
                    finalScoreMultiplier = boss.Definition.JackpotFinalScoreMultiplier;
                    breakdown.Add($"Boss - Jackpot 7: x{finalScoreMultiplier:0.##} score final");
                }
            }

            dominexEffectEngine.ApplyScoringEffects(placedDominoes, dominexContext, ref count, ref mult, breakdown);

            var finalScore = (int)System.Math.Ceiling(count * mult * finalScoreMultiplier);
            breakdown.Add($"Score final: {count} x {mult} = {finalScore}");
            return new ScoreResult(count, mult, finalScore, patterns, breakdown);
        }
    }
}
