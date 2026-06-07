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

        public static bool IsComboVisible(PatternComboDefinition combo, RunState run)
        {
            if (DevMode.Enabled || combo == null)
            {
                return true;
            }

            var design = PatternCatalog.GetById(combo.DesignPatternId);
            return IsPatternVisible(design, run);
        }

        public static bool IsConsumableVisible(ConsumableDefinition consumable, RunState run)
        {
            if (DevMode.Enabled || consumable == null)
            {
                return true;
            }

            return IsPatternVisible(PatternCatalog.GetById(consumable.TargetPatternId), run);
        }
    }
}
