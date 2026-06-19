using System.Collections.Generic;
using System.Linq;
using DomiNox.Bosses;
using DomiNox.Dominoes;
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
            var steps = new List<ScoringStep>();
            var dominoModifierEffects = new List<DominoModifierEffectResult>();
            var order = 0;
            var finalScoreMultiplier = 1f;
            steps.Add(new ScoringStep(ScoringStepType.Reset, string.Empty, "Reset", 0, 0, 1f, "Reset scoring display", order++));
            var disabledPatternIds = boss?.Definition.RuleType == BossRuleType.DisablePatterns ? boss.Definition.DisabledPatternIds : null;
            var detectedPatternInfos = patternDetector.DetectPatternInfos(placedDominoes, maxPlacedDominoes, disabledPatternIds);
            dominexContext = dominexContext?.WithDetectedPatterns(detectedPatternInfos, boss);
            var patterns = detectedPatternInfos.Select(pattern => pattern.Name).ToList();
            var patternIds = detectedPatternInfos.Select(pattern => pattern.Id).ToList();
            var valuePattern = detectedPatternInfos.FirstOrDefault(pattern => pattern.Category == PatternCategory.Value);
            var designPattern = detectedPatternInfos.FirstOrDefault(pattern => pattern.Category == PatternCategory.Design);
            var valueLevel = valuePattern == null ? 1 : dominexContext?.GetPatternLevel(valuePattern.Id) ?? 1;
            var designLevel = designPattern == null ? 1 : dominexContext?.GetPatternLevel(designPattern.Id) ?? 1;
            var valueBonus = valuePattern == null ? default : PatternScalingService.GetScaledPatternBonus(valuePattern, valueLevel);
            var designBonus = designPattern == null ? default : PatternScalingService.GetScaledPatternBonus(designPattern, designLevel);
            PatternComboResult comboResult = null;

            foreach (var pattern in detectedPatternInfos)
            {
                var level = dominexContext?.GetPatternLevel(pattern.Id) ?? 1;
                var bonus = PatternScalingService.GetScaledPatternBonus(pattern, level);
                count += bonus.Count;
                mult += bonus.Mult;
                breakdown.Add($"{pattern.Name} Lv.{level}: +{bonus.Count} Count, +{bonus.Mult} Mult");
                if (bonus.Count != 0)
                {
                    steps.Add(new ScoringStep(pattern.Category == PatternCategory.Value ? ScoringStepType.PatternCountBonus : ScoringStepType.PatternCountBonus, pattern.Id, $"{pattern.Name} Lv.{level}", bonus.Count, 0, 1f, $"+{bonus.Count} Tile", order++, floatingTextType: FloatingTextType.Count));
                }

                if (bonus.Mult != 0)
                {
                    steps.Add(new ScoringStep(pattern.Category == PatternCategory.Value ? ScoringStepType.PatternMultBonus : ScoringStepType.PatternMultBonus, pattern.Id, $"{pattern.Name} Lv.{level}", 0, bonus.Mult, 1f, $"+{bonus.Mult} Mult", order++, floatingTextType: FloatingTextType.Mult));
                }
            }

            if (valuePattern != null && designPattern != null && PatternComboCatalog.TryGetCombo(valuePattern.Id, designPattern.Id, out var combo))
            {
                count += combo.CountBonus;
                mult += combo.MultBonus;
                comboResult = new PatternComboResult(combo.Id, combo.Name, valuePattern.Id, valuePattern.Name, valueLevel, designPattern.Id, designPattern.Name, designLevel, combo.CountBonus, combo.MultBonus, valueBonus.Count + designBonus.Count + combo.CountBonus, valueBonus.Mult + designBonus.Mult + combo.MultBonus);
                patterns.Add(combo.Name);
                breakdown.Add($"{combo.Name} bonus: +{combo.CountBonus} Count, +{combo.MultBonus} Mult");
                if (combo.CountBonus != 0)
                {
                    steps.Add(new ScoringStep(ScoringStepType.PatternComboCountBonus, combo.Id, combo.Name, combo.CountBonus, 0, 1f, $"+{combo.CountBonus} Tile", order++, floatingTextType: FloatingTextType.Count));
                }

                if (combo.MultBonus != 0)
                {
                    steps.Add(new ScoringStep(ScoringStepType.PatternComboMultBonus, combo.Id, combo.Name, 0, combo.MultBonus, 1f, $"+{combo.MultBonus} Mult", order++, floatingTextType: FloatingTextType.Mult));
                }
            }

            foreach (var placed in placedDominoes.OrderBy(placed => placed.Position.Y).ThenBy(placed => placed.Position.X))
            {
                var dominoCount = placed.Domino.Definition.Sum;
                if (dominoCount != 0)
                {
                    steps.Add(new ScoringStep(ScoringStepType.DominoCount, placed.Domino.InstanceId, placed.Domino.Definition.ToString(), dominoCount, 0, 1f, $"+{dominoCount} Tile", order++, placed, FloatingTextType.Count));
                }

                foreach (var modifierEffect in DominoModifierEffectResolver.Resolve(placed))
                {
                    dominoModifierEffects.Add(modifierEffect);
                    count += modifierEffect.CountDelta;
                    mult += modifierEffect.MultDelta;
                    finalScoreMultiplier *= modifierEffect.FinalScoreMultiplier;
                    if (modifierEffect.CountDelta != 0 || modifierEffect.MultDelta != 0 || modifierEffect.FinalScoreMultiplier != 1f)
                    {
                        breakdown.Add($"{modifierEffect.ModifierName}: {modifierEffect.Description}");
                        steps.Add(new ScoringStep(ScoringStepType.DominoModifier, modifierEffect.ModifierId, modifierEffect.ModifierName, modifierEffect.CountDelta, modifierEffect.MultDelta, modifierEffect.FinalScoreMultiplier, modifierEffect.Description, order++, placed, modifierEffect.FinalScoreMultiplier != 1f ? FloatingTextType.Multiplier : modifierEffect.MultDelta != 0 ? FloatingTextType.Mult : FloatingTextType.Count));
                    }
                }
            }

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
                    if (countBonus != 0)
                    {
                        steps.Add(new ScoringStep(ScoringStepType.BossCount, boss.Definition.Id, boss.Definition.Name, countBonus, 0, 1f, $"+{countBonus} Tile", order++, floatingTextType: FloatingTextType.Count));
                    }

                    if (multBonus != 0)
                    {
                        steps.Add(new ScoringStep(ScoringStepType.BossMult, boss.Definition.Id, boss.Definition.Name, 0, multBonus, 1f, $"+{multBonus} Mult", order++, floatingTextType: FloatingTextType.Mult));
                    }
                }

                if (patternIds.Contains(PatternNames.JackpotSevenId))
                {
                    finalScoreMultiplier *= boss.Definition.JackpotFinalScoreMultiplier;
                    breakdown.Add($"Boss - Jackpot 7: x{finalScoreMultiplier:0.##} score final");
                    steps.Add(new ScoringStep(ScoringStepType.DomiNexMultiplier, boss.Definition.Id, boss.Definition.Name, 0, 0, finalScoreMultiplier, $"x{finalScoreMultiplier:0.##}", order++, floatingTextType: FloatingTextType.Multiplier));
                }
            }

            dominexEffectEngine.ApplyScoringEffects(placedDominoes, dominexContext, ref count, ref mult, ref finalScoreMultiplier, breakdown, steps, ref order);

            mult = System.Math.Max(1, mult);
            var finalScore = (int)System.Math.Ceiling(count * mult * finalScoreMultiplier);
            breakdown.Add(finalScoreMultiplier == 1f
                ? $"Score final: {count} x {mult} = {finalScore}"
                : $"Score final: {count} x {mult} x {finalScoreMultiplier:0.##} = {finalScore}");
            steps.Add(new ScoringStep(ScoringStepType.FinalScore, string.Empty, "Final Score", 0, 0, finalScoreMultiplier, finalScore.ToString(), order++, floatingTextType: FloatingTextType.Multiplier));
            return new ScoreResult(count, mult, finalScore, patterns, breakdown, valuePattern?.Id, valuePattern?.Name, valueLevel, designPattern?.Id, designPattern?.Name, designLevel, steps, patternCombo: comboResult, dominoModifierEffects: dominoModifierEffects);
        }
    }
}
