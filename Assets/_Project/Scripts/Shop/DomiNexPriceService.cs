using System;
using System.Collections.Generic;
using DomiNox.Dominex;

namespace DomiNox.Shop
{
    public static class DomiNexPriceService
    {
        public static int GeneratePrice(DomiNexRarity rarity, Random random)
        {
            switch (rarity)
            {
                case DomiNexRarity.Common:
                    return PickWeightedPrice(new Dictionary<int, int> { { 1, 1 }, { 2, 2 }, { 3, 4 }, { 4, 8 }, { 5, 4 }, { 6, 2 } }, random);
                case DomiNexRarity.Rare:
                    return PickWeightedPrice(new Dictionary<int, int> { { 4, 1 }, { 5, 3 }, { 6, 6 }, { 7, 3 }, { 8, 1 } }, random);
                case DomiNexRarity.Epic:
                    return PickWeightedPrice(new Dictionary<int, int> { { 7, 2 }, { 8, 5 }, { 9, 5 }, { 10, 2 } }, random);
                case DomiNexRarity.Legendary:
                    return 20;
                default:
                    return 0;
            }
        }

        private static int PickWeightedPrice(IReadOnlyDictionary<int, int> weights, Random random)
        {
            var total = 0;
            foreach (var weight in weights.Values)
            {
                total += weight;
            }

            var roll = random.Next(total);
            var cursor = 0;
            foreach (var pair in weights)
            {
                cursor += pair.Value;
                if (roll < cursor)
                {
                    return pair.Key;
                }
            }

            return 0;
        }
    }
}
