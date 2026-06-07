using System.Collections.Generic;
using System.Linq;
using DomiNox.Dominoes;
using DomiNox.Run;

namespace DomiNox.Dominex
{
    public sealed class DomiNexDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public DomiNexRarity Rarity { get; }
        public string Description { get; }
        public IReadOnlyList<string> Tags { get; }
        public IReadOnlyList<DomiNexEffectDefinition> Effects { get; }

        public DomiNexDefinition(string id, string name, DomiNexRarity rarity, string description, IReadOnlyList<string> tags, IReadOnlyList<DomiNexEffectDefinition> effects)
        {
            Id = id;
            Name = name;
            Rarity = rarity;
            Description = description;
            Tags = tags;
            Effects = effects;
        }

        public string GetCurrentEffectText(RunState run)
        {
            if (run == null)
            {
                return string.Empty;
            }

            switch (Id)
            {
                case "empty_dominex":
                    return $"Currently: x{System.Math.Max(1, run.MaxDomiNexSlots - run.ActiveDomiNexCount)} Mult";
                case "doublish":
                    return $"Currently: x{1f + (0.2f * CountDeckDoubles(run)):0.##}";
                case "gros_michel":
                    return "1 in 6 chance to break";
                case "space_dominex":
                    return "1 in 4 chance";
                case "credit_dominex":
                    return "Debt limit: -20 credits";
                default:
                    return string.Empty;
            }
        }

        private static int CountDeckDoubles(RunState run)
        {
            var placed = run.CurrentLevel?.Grid.GetPlacedDominoes().Select(item => item.Domino) ?? Enumerable.Empty<DominoInstance>();
            var hand = run.CurrentLevel?.Hand.Dominoes ?? Enumerable.Empty<DominoInstance>();
            return run.Bag.RemainingDominoes.Concat(run.Bag.DiscardedDominoes).Concat(hand).Concat(placed).GroupBy(domino => domino.InstanceId).Select(group => group.First()).Count(domino => domino.Definition.IsDouble);
        }
    }
}
