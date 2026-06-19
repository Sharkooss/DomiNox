using System;
using System.Collections.Generic;
using System.Linq;
using DomiNox.Dominoes;

namespace DomiNox.Shop
{
    public sealed class DominoModifierRollService
    {
        private readonly Random random;

        private static readonly IReadOnlyList<(string Id, int Weight)> Weights = new[]
        {
            (DominoModifierRegistry.BlueId, 25),
            (DominoModifierRegistry.RedId, 25),
            (DominoModifierRegistry.GoldId, 20),
            (DominoModifierRegistry.JackpotId, 12),
            (DominoModifierRegistry.LuckyId, 8),
            (DominoModifierRegistry.GlassId, 6),
            (DominoModifierRegistry.LightId, 4)
        };

        public DominoModifierRollService(Random random = null)
        {
            this.random = random ?? new Random();
        }

        public string RollModifier()
        {
            var total = Weights.Sum(item => item.Weight);
            var roll = random.Next(total);
            var cursor = 0;
            foreach (var item in Weights)
            {
                cursor += item.Weight;
                if (roll < cursor)
                {
                    return item.Id;
                }
            }

            return DominoModifierRegistry.BlueId;
        }
    }
}
