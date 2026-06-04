using System.Collections.Generic;
using System.Linq;
using DomiNox.Grid;
using DomiNox.Run;

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

        public void ApplyScoringEffects(List<PlacedDomino> placedDominoes, DomiNexScoringContext context, ref int count, ref int mult, List<string> breakdown)
        {
            if (context == null || context.ActiveDomiNex == null)
            {
                return;
            }

            foreach (var dominex in context.ActiveDomiNex)
            {
                foreach (var effect in dominex.Effects.Where(effect => effect.Trigger == DomiNexTrigger.Scoring))
                {
                    ApplyScoringEffect(dominex, effect, placedDominoes, context, ref count, ref mult, breakdown);
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

        private static void ApplyScoringEffect(DomiNexDefinition dominex, DomiNexEffectDefinition effect, List<PlacedDomino> placedDominoes, DomiNexScoringContext context, ref int count, ref int mult, List<string> breakdown)
        {
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
                case DomiNexEffectType.AddMultIfFullNox:
                    if (placedDominoes.Count == context.MaxPlacedDominoes)
                    {
                        mult += effect.Value;
                        AddLine(breakdown, dominex, $"{effect.Value:+#;-#;0} Mult");
                    }
                    break;
            }
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
