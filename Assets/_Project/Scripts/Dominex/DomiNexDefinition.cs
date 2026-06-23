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
            }

            var postScoringEffect = Effects.FirstOrDefault(e => e.Trigger == DomiNexTrigger.PostScoring);
            if (postScoringEffect?.Type == DomiNexEffectType.ChanceDestroySelf)
            {
                return $"1 in {postScoringEffect.Threshold} chance to break";
            }

            if (postScoringEffect?.Type == DomiNexEffectType.ChancePatternLevelUp)
            {
                return $"1 in {postScoringEffect.Threshold} chance";
            }

            var passiveFloor = Effects.FirstOrDefault(e => e.Trigger == DomiNexTrigger.Passive && e.Type == DomiNexEffectType.SetMinimumCreditFloor);
            if (passiveFloor != null)
            {
                return $"Debt limit: -{passiveFloor.Value} credits";
            }

            return string.Empty;
        }

        private static int CountDeckDoubles(RunState run)
        {
            var placed = run.CurrentLevel?.Grid.GetPlacedDominoes().Select(item => item.Domino) ?? Enumerable.Empty<DominoInstance>();
            var hand = run.CurrentLevel?.Hand.Dominoes ?? Enumerable.Empty<DominoInstance>();
            var playedThisLevel = run.CurrentLevel?.PlayedThisLevel ?? Enumerable.Empty<DominoInstance>();
            var discardedThisLevel = run.CurrentLevel?.DiscardedThisLevel ?? Enumerable.Empty<DominoInstance>();
            return run.Bag.RemainingDominoes.Concat(run.Bag.DiscardedDominoes).Concat(hand).Concat(placed).Concat(playedThisLevel).Concat(discardedThisLevel).GroupBy(domino => domino.InstanceId).Select(group => group.First()).Count(domino => domino.Definition.IsDouble);
        }
    }
}
