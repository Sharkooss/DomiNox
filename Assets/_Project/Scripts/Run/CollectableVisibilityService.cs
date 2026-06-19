using DomiNox.Consumables;
using DomiNox.Core;
using DomiNox.Patterns;

namespace DomiNox.Run
{
    public static class CollectableVisibilityService
    {
        public static bool IsPatternVisible(PatternInfo pattern, RunState run)
        {
            return DevMode.Enabled || pattern == null || !pattern.IsSecret || run?.RevealedSecretPatterns.Contains(pattern.Id) == true;
        }

        public static bool IsPatternVisibleInRunInfo(PatternInfo pattern, RunState run)
        {
            return pattern == null || !pattern.IsSecret || run?.RevealedSecretPatterns.Contains(pattern.Id) == true;
        }

        public static bool IsPatternVisibleInRunInfo(string patternId, RunState run)
        {
            return IsPatternVisibleInRunInfo(PatternCatalog.GetById(patternId), run);
        }

        public static bool IsPatternVisible(string patternId, RunState run)
        {
            return IsPatternVisible(PatternCatalog.GetById(patternId), run);
        }

        public static bool IsComboVisible(PatternComboDefinition combo, RunState run)
        {
            if (DevMode.Enabled || combo == null)
            {
                return true;
            }

            var design = PatternCatalog.GetById(combo.DesignPatternId);
            return IsPatternVisible(design, run);
        }

        public static bool IsComboVisibleInRunInfo(PatternComboDefinition combo, RunState run)
        {
            if (combo == null)
            {
                return true;
            }

            var design = PatternCatalog.GetById(combo.DesignPatternId);
            return IsPatternVisibleInRunInfo(design, run);
        }

        public static bool IsComboVisible(string comboId, RunState run)
        {
            foreach (var combo in PatternComboCatalog.All)
            {
                if (combo.Id == comboId)
                {
                    return IsComboVisible(combo, run);
                }
            }

            return true;
        }

        public static bool IsConsumableVisible(ConsumableDefinition consumable, RunState run)
        {
            if (DevMode.Enabled || consumable == null)
            {
                return true;
            }

            return IsPatternVisible(PatternCatalog.GetById(consumable.TargetPatternId), run);
        }

        public static bool IsConsumableVisibleInRun(ConsumableDefinition consumable, RunState run)
        {
            if (consumable == null)
            {
                return true;
            }

            return IsPatternVisibleInRunInfo(PatternCatalog.GetById(consumable.TargetPatternId), run);
        }

        public static bool IsConsumableVisible(string consumableId, RunState run)
        {
            return IsConsumableVisible(GemTileRegistry.GetById(consumableId), run);
        }

        public static bool CanGemTileAppearInPack(ConsumableDefinition consumable, RunState run)
        {
            return IsConsumableVisibleInRun(consumable, run);
        }

        public static bool CanGemTileAppearInShop(ConsumableDefinition consumable, RunState run)
        {
            return IsConsumableVisibleInRun(consumable, run);
        }
    }
}
