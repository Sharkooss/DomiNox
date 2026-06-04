using System;
using System.Collections.Generic;
using System.Linq;
using DomiNox.Dominex;

namespace DomiNox.Shop
{
    public sealed class DomiNexShopService
    {
        private const int OfferCount = 3;
        private readonly Random random = new Random();

        public ShopState GenerateShop(DomiNexInventory inventory, int floorIndex)
        {
            var shop = new ShopState();
            var candidates = DomiNexRegistry.All
                .Where(definition => definition.Rarity != DomiNexRarity.Cursed)
                .Where(definition => !inventory.Contains(definition.Id))
                .ToList();

            for (var i = 0; i < OfferCount && candidates.Count > 0; i++)
            {
                var selected = PickWeighted(candidates, floorIndex);
                candidates.Remove(selected);
                shop.Offers.Add(new ShopOffer(selected, GetPrice(selected.Rarity)));
            }

            return shop;
        }

        public int GetPrice(DomiNexRarity rarity)
        {
            return rarity switch
            {
                DomiNexRarity.Common => 4,
                DomiNexRarity.Rare => 7,
                DomiNexRarity.Epic => 12,
                DomiNexRarity.Legendary => 20,
                DomiNexRarity.Cursed => 0,
                _ => 6
            };
        }

        private DomiNexDefinition PickWeighted(IReadOnlyList<DomiNexDefinition> candidates, int floorIndex)
        {
            var totalWeight = candidates.Sum(candidate => GetWeight(candidate.Rarity, floorIndex));
            var roll = random.Next(0, totalWeight);
            var cursor = 0;

            foreach (var candidate in candidates)
            {
                cursor += GetWeight(candidate.Rarity, floorIndex);
                if (roll < cursor)
                {
                    return candidate;
                }
            }

            return candidates[0];
        }

        private static int GetWeight(DomiNexRarity rarity, int floorIndex)
        {
            if (floorIndex <= 1)
            {
                return rarity switch
                {
                    DomiNexRarity.Common => 75,
                    DomiNexRarity.Rare => 22,
                    DomiNexRarity.Epic => 3,
                    DomiNexRarity.Legendary => 0,
                    _ => 0
                };
            }

            if (floorIndex == 2)
            {
                return rarity switch
                {
                    DomiNexRarity.Common => 55,
                    DomiNexRarity.Rare => 35,
                    DomiNexRarity.Epic => 9,
                    DomiNexRarity.Legendary => 1,
                    _ => 0
                };
            }

            return rarity switch
            {
                DomiNexRarity.Common => 40,
                DomiNexRarity.Rare => 38,
                DomiNexRarity.Epic => 17,
                DomiNexRarity.Legendary => 4,
                _ => 0
            };
        }
    }
}
