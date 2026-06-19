using System.Collections.Generic;
using System.Linq;
using DomiNox.Grid;
using DomiNox.Patterns;
using DomiNox.Run;
using DomiNox.Scoring;

namespace DomiNox.Dominex
{
    public sealed class DomiNexEffectEngine
    {
        public void ApplyRunStart(DomiNexInventory inventory, RunState run, List<string> breakdown)
        {
            foreach (var effect in GetEffects(inventory, DomiNexTrigger.RunStart))
            {
                if (effect.Type == DomiNexEffectType.AddCredits)
                {
                    run.Credits += effect.Value;
                    breakdown?.Add($"DomiNex: +{effect.Value} credits");
                }
            }
        }

        public void ApplyLevelStart(DomiNexInventory inventory, LevelState level, List<string> breakdown)
        {
            foreach (var effect in GetEffects(inventory, DomiNexTrigger.LevelStart))
            {
                if (effect.Type == DomiNexEffectType.AddMaxPlacedDominoes)
                {
                    level.MaxPlacedDominoes += effect.Value;
                    breakdown?.Add($"DomiNex: {effect.Value:+#;-#;0} domino jouable");
                }
            }
        }

        public void ApplyScoringEffects(List<PlacedDomino> placedDominoes, DomiNexScoringContext context, ref int count, ref int mult, ref float finalScoreMultiplier, List<string> breakdown)
        {
            var order = 0;
            ApplyScoringEffects(placedDominoes, context, ref count, ref mult, ref finalScoreMultiplier, breakdown, null, ref order);
        }

        public void ApplyScoringEffects(List<PlacedDomino> placedDominoes, DomiNexScoringContext context, ref int count, ref int mult, ref float finalScoreMultiplier, List<string> breakdown, List<ScoringStep> steps, ref int order)
        {
            if (context == null || context.ActiveDomiNex == null)
            {
                return;
            }

            foreach (var dominex in context.ActiveDomiNex)
            {
                foreach (var effect in dominex.Effects.Where(effect => effect.Trigger == DomiNexTrigger.Scoring).OrderBy(effect => IsMultiplicative(effect.Type) ? 1 : 0))
                {
                    ApplyScoringEffect(dominex, effect, placedDominoes, context, ref count, ref mult, ref finalScoreMultiplier, breakdown, steps, ref order);
                }
            }
        }

        public bool HasActive(DomiNexInventory inventory, string id)
        {
            return inventory.Active.Any(definition => definition.Id == id);
        }

        private static IEnumerable<DomiNexEffectDefinition> GetEffects(DomiNexInventory inventory, DomiNexTrigger trigger)
        {
            return inventory.Active.SelectMany(definition => definition.Effects).Where(effect => effect.Trigger == trigger);
        }

        private static bool IsMultiplicative(DomiNexEffectType type)
        {
            return type == DomiNexEffectType.MultiplyMultByFreeDomiNexSlots
                || type == DomiNexEffectType.AddFinalMultiplierByBagDoubleCount
                || type == DomiNexEffectType.AddFinalMultiplierIfValueAndDesignPattern
                || type == DomiNexEffectType.AddFinalMultiplierIfFullPlacedElseMultPenalty;
        }

        private static void ApplyScoringEffect(DomiNexDefinition dominex, DomiNexEffectDefinition effect, List<PlacedDomino> placedDominoes, DomiNexScoringContext context, ref int count, ref int mult, ref float finalScoreMultiplier, List<string> breakdown, List<ScoringStep> steps, ref int order)
        {
            var beforeCount = count;
            var beforeMult = mult;
            var beforeMultiplier = finalScoreMultiplier;
            switch (effect.Type)
            {
                case DomiNexEffectType.AddCount:
                    count += effect.Value;
                    AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Count");
                    break;
                case DomiNexEffectType.AddMult:
                    mult += effect.Value;
                    AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    break;
                case DomiNexEffectType.AddCountPerDominoSumAtLeast:
                    ApplyPerDominoCount(dominex, placedDominoes.Count(placed => placed.Domino.Definition.Sum >= effect.Threshold), effect.Value, ref count, breakdown);
                    break;
                case DomiNexEffectType.AddCountPerDominoSumAtMost:
                    ApplyPerDominoCount(dominex, placedDominoes.Count(placed => placed.Domino.Definition.Sum <= effect.Threshold), effect.Value, ref count, breakdown);
                    break;
                case DomiNexEffectType.AddMultPerDominoSumAtLeast:
                    ApplyPerDominoMult(dominex, placedDominoes.Count(placed => placed.Domino.Definition.Sum >= effect.Threshold), effect.Value, ref mult, breakdown);
                    break;
                case DomiNexEffectType.AddMultPerDominoSumAtMost:
                    ApplyPerDominoMult(dominex, placedDominoes.Count(placed => placed.Domino.Definition.Sum <= effect.Threshold), effect.Value, ref mult, breakdown);
                    break;
                case DomiNexEffectType.AddMultPerDominoSumExactly:
                    ApplyPerDominoMult(dominex, placedDominoes.Count(placed => placed.Domino.Definition.Sum == effect.Threshold), effect.Value, ref mult, breakdown);
                    break;
                case DomiNexEffectType.AddMultPerDouble:
                    ApplyPerDominoMult(dominex, placedDominoes.Count(placed => placed.Domino.Definition.IsDouble), effect.Value, ref mult, breakdown);
                    break;
                case DomiNexEffectType.AddMultPerCreditStep:
                    var creditSteps = effect.Threshold <= 0 ? 0 : context.Credits / effect.Threshold;
                    ApplyPerDominoMult(dominex, creditSteps, effect.Value, ref mult, breakdown);
                    break;
                case DomiNexEffectType.AddMultIfNoDiscard:
                    if (context.DiscardsUsed == 0)
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddCountIfPlacedAtLeast:
                    if (placedDominoes.Count >= effect.Threshold)
                    {
                        count += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Count");
                    }
                    break;
                case DomiNexEffectType.AddCountToLastDomino:
                    if (placedDominoes.Count > 0)
                    {
                        count += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Count");
                    }
                    break;
                case DomiNexEffectType.AddMultToFirstDomino:
                    if (placedDominoes.Count > 0)
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultIfExactPlacedCount:
                    if (placedDominoes.Count == effect.Threshold)
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultIfPlacedAtMost:
                    if (placedDominoes.Count <= effect.Threshold)
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultIfFullNox:
                    if (placedDominoes.Count == context.MaxPlacedDominoes)
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultIfAnyDesignPattern:
                    if (context.DetectedPatterns.Any(pattern => pattern.Category == PatternCategory.Design))
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddCountAndMultIfPattern:
                    if (HasPattern(context, effect.Note))
                    {
                        count += effect.Value;
                        mult += effect.Threshold;
                        AddLine(breakdown, dominex, $"+{effect.Value} Count, +{effect.Threshold} Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultIfNoDiscardRemaining:
                    if (context.DiscardsRemaining == 0)
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultPerCreditStepNoInterest:
                    var creditStepCount = effect.Threshold <= 0 ? 0 : context.Credits / effect.Threshold;
                    ApplyPerDominoMult(dominex, creditStepCount, effect.Value, ref mult, breakdown);
                    break;
                case DomiNexEffectType.AddCountAndMultToFirstSumExactly:
                    if (placedDominoes.Count > 0 && placedDominoes[0].Domino.Definition.Sum == effect.Threshold)
                    {
                        count += effect.Value;
                        mult += 2;
                        AddLine(breakdown, dominex, $"+{effect.Value} Count, +2 Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultIfPattern:
                    if (HasPattern(context, effect.Note))
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddCountIfDominoSumAtMostCountAtLeast:
                    if (placedDominoes.Count(placed => placed.Domino.Definition.Sum <= effect.Threshold) >= 4)
                    {
                        count += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Count");
                    }
                    break;
                case DomiNexEffectType.AddCountPerDominoSumAtLeastWithPenalty:
                    var highCount = placedDominoes.Count(placed => placed.Domino.Definition.Sum >= effect.Threshold);
                    if (highCount > 0)
                    {
                        ApplyPerDominoCount(dominex, highCount, effect.Value, ref count, breakdown);
                    }
                    else
                    {
                        mult -= 1;
                        AddLine(breakdown, dominex, "-1 Mult");
                    }
                    break;
                case DomiNexEffectType.AddCountAndMultIfBossLevel:
                    if (context.Boss != null)
                    {
                        count += effect.Value;
                        mult += effect.Threshold;
                        AddLine(breakdown, dominex, $"+{effect.Value} Count, +{effect.Threshold} Mult");
                    }
                    break;
                case DomiNexEffectType.AddFirstDominoBaseCountAgain:
                    if (placedDominoes.Count > 0)
                    {
                        var firstSum = placedDominoes[0].Domino.Definition.Sum;
                        count += firstSum;
                        AddLine(breakdown, dominex, $"+{firstSum} Count");
                    }
                    break;
                case DomiNexEffectType.AddFinalMultiplierIfValueAndDesignPattern:
                    if (context.DetectedPatterns.Any(pattern => pattern.Category == PatternCategory.Value) && context.DetectedPatterns.Any(pattern => pattern.Category == PatternCategory.Design))
                    {
                        finalScoreMultiplier *= effect.Value / 100f;
                        AddLine(breakdown, dominex, $"x{effect.Value / 100f:0.##} score final");
                    }
                    break;
                case DomiNexEffectType.AddFinalMultiplierIfFullPlacedElseMultPenalty:
                    if (placedDominoes.Count == context.MaxPlacedDominoes)
                    {
                        finalScoreMultiplier *= effect.Value / 100f;
                        AddLine(breakdown, dominex, $"x{effect.Value / 100f:0.##} score final");
                    }
                    else
                    {
                        mult -= effect.Threshold;
                        AddLine(breakdown, dominex, $"-{effect.Threshold} Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultIfAllSumsAtMost:
                    if (placedDominoes.Count > 0 && placedDominoes.All(placed => placed.Domino.Definition.Sum <= effect.Threshold))
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddCountIfAllSumsAtMost:
                    if (placedDominoes.Count > 0 && placedDominoes.All(placed => placed.Domino.Definition.Sum <= effect.Threshold))
                    {
                        count += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Count");
                    }
                    break;
                case DomiNexEffectType.AddMultIfAllSumsAbove:
                    if (placedDominoes.Count > 0 && placedDominoes.All(placed => placed.Domino.Definition.Sum > effect.Threshold))
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddCountIfAllSumsAbove:
                    if (placedDominoes.Count > 0 && placedDominoes.All(placed => placed.Domino.Definition.Sum > effect.Threshold))
                    {
                        count += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Count");
                    }
                    break;
                case DomiNexEffectType.AddMultIfDoubleCountAtLeast:
                    if (placedDominoes.Count(placed => placed.Domino.Definition.IsDouble) >= effect.Threshold)
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddCountIfDoubleCountAtLeast:
                    if (placedDominoes.Count(placed => placed.Domino.Definition.IsDouble) >= effect.Threshold)
                    {
                        count += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Count");
                    }
                    break;
                case DomiNexEffectType.AddCountIfPattern:
                    if (HasPattern(context, effect.Note))
                    {
                        count += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Count");
                    }
                    break;
                case DomiNexEffectType.AddMultIfPatternId:
                    if (HasPattern(context, effect.Note))
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
                case DomiNexEffectType.AddCountAndMultPerDominoSumExactly:
                    var exactSumMatches = placedDominoes.Count(placed => placed.Domino.Definition.Sum == effect.Threshold);
                    if (exactSumMatches > 0)
                    {
                        var exactCountBonus = exactSumMatches * effect.Value;
                        var exactMultBonus = exactSumMatches * ParseInt(effect.Note);
                        count += exactCountBonus;
                        mult += exactMultBonus;
                        AddLine(breakdown, dominex, $"+{exactCountBonus} Count, +{exactMultBonus} Mult");
                    }
                    break;
                case DomiNexEffectType.AddCountIfPatternWithCrossExtensionMult:
                    var cross = context.DetectedPatterns.FirstOrDefault(pattern => pattern.Id == effect.Note);
                    if (cross != null)
                    {
                        count += effect.Value;
                        var extensionMult = cross.ExtensionCount * effect.Threshold;
                        mult += extensionMult;
                        AddLine(breakdown, dominex, $"+{effect.Value} Count, +{extensionMult} Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultPerDiscardRemaining:
                    ApplyPerDominoMult(dominex, context.DiscardsRemaining, effect.Value, ref mult, breakdown);
                    break;
                case DomiNexEffectType.AddMultPerEvenDomino:
                    ApplyPerDominoMult(dominex, placedDominoes.Count(placed => IsEven(placed.Domino.Definition.Left) && IsEven(placed.Domino.Definition.Right)), effect.Value, ref mult, breakdown);
                    break;
                case DomiNexEffectType.AddCountPerOddDomino:
                    ApplyPerDominoCount(dominex, placedDominoes.Count(placed => IsOdd(placed.Domino.Definition.Left) && IsOdd(placed.Domino.Definition.Right)), effect.Value, ref count, breakdown);
                    break;
                case DomiNexEffectType.AddCountAndMultPerDominoContainingValue:
                    var containingValue = placedDominoes.Count(placed => placed.Domino.Definition.Left == effect.Threshold || placed.Domino.Definition.Right == effect.Threshold);
                    if (containingValue > 0)
                    {
                        var containingCountBonus = containingValue * effect.Value;
                        var containingMultBonus = containingValue * ParseInt(effect.Note);
                        count += containingCountBonus;
                        mult += containingMultBonus;
                        AddLine(breakdown, dominex, $"+{containingCountBonus} Count, +{containingMultBonus} Mult");
                    }
                    break;
                case DomiNexEffectType.AddMultPerDominoContainingAnyValue:
                    var values = ParseValues(effect.Note);
                    ApplyPerDominoMult(dominex, placedDominoes.Count(placed => values.Contains(placed.Domino.Definition.Left) || values.Contains(placed.Domino.Definition.Right)), effect.Value, ref mult, breakdown);
                    break;
                case DomiNexEffectType.MultiplyMultByFreeDomiNexSlots:
                    var freeSlots = System.Math.Max(1, context.MaxDomiNexSlots - context.ActiveDomiNexCount);
                    mult *= freeSlots;
                    AddLine(breakdown, dominex, $"x{freeSlots} Mult");
                    break;
                case DomiNexEffectType.AddFinalMultiplierByBagDoubleCount:
                    var multiplier = 1f + ((effect.Value / 100f) * context.BagDoubleCount);
                    finalScoreMultiplier *= multiplier;
                    AddLine(breakdown, dominex, $"x{multiplier:0.##} score final");
                    break;
            }

            AddDomiNexSteps(dominex, count - beforeCount, mult - beforeMult, finalScoreMultiplier / beforeMultiplier, steps, ref order);
        }

        private static void AddDomiNexSteps(DomiNexDefinition dominex, int countDelta, int multDelta, float multiplierRatio, List<ScoringStep> steps, ref int order)
        {
            if (steps == null)
            {
                return;
            }

            if (countDelta != 0)
            {
                steps.Add(new ScoringStep(ScoringStepType.DomiNexCount, dominex.Id, dominex.Name, countDelta, 0, 1f, $"{countDelta:+#;-#;0} Tile", order++, floatingTextType: countDelta > 0 ? FloatingTextType.Count : FloatingTextType.Warning));
            }

            if (multDelta != 0)
            {
                steps.Add(new ScoringStep(ScoringStepType.DomiNexMult, dominex.Id, dominex.Name, 0, multDelta, 1f, $"{multDelta:+#;-#;0} Mult", order++, floatingTextType: multDelta > 0 ? FloatingTextType.Mult : FloatingTextType.Warning));
            }

            if (System.Math.Abs(multiplierRatio - 1f) > 0.001f)
            {
                steps.Add(new ScoringStep(ScoringStepType.DomiNexMultiplier, dominex.Id, dominex.Name, 0, 0, multiplierRatio, $"x{multiplierRatio:0.##}", order++, floatingTextType: FloatingTextType.Multiplier));
            }
        }

        private static bool HasPattern(DomiNexScoringContext context, string patternName)
        {
            return context.DetectedPatterns.Any(pattern => pattern.Id == patternName || pattern.Name == patternName);
        }

        private static bool IsEven(int value) => value % 2 == 0;
        private static bool IsOdd(int value) => value % 2 != 0;

        private static int ParseInt(string value)
        {
            return int.TryParse(value, out var parsed) ? parsed : 0;
        }

        private static HashSet<int> ParseValues(string csv)
        {
            return new HashSet<int>((csv ?? string.Empty).Split(',').Select(part => int.TryParse(part.Trim(), out var value) ? value : -999));
        }

        private static void ApplyPerDominoCount(DomiNexDefinition dominex, int matches, int value, ref int count, List<string> breakdown)
        {
            var bonus = matches * value;
            if (bonus == 0)
            {
                return;
            }

            count += bonus;
            AddLine(breakdown, dominex, $"{bonus:+#;-#;0} Count");
        }

        private static void ApplyPerDominoMult(DomiNexDefinition dominex, int matches, int value, ref int mult, List<string> breakdown)
        {
            var bonus = matches * value;
            if (bonus == 0)
            {
                return;
            }

            mult += bonus;
            AddLine(breakdown, dominex, $"{bonus:+#;-#;0} Mult");
        }

        private static void AddLine(List<string> breakdown, DomiNexDefinition dominex, string text)
        {
            breakdown.Add($"DomiNex - {dominex.Name}: {text}");
        }
    }
}
