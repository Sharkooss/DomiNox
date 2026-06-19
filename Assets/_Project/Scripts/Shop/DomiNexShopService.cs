using System;
using System.Collections.Generic;
using System.Linq;
using DomiNox.Dominex;
using DomiNox.Consumables;
using DomiNox.Run;

namespace DomiNox.Shop
{
    public sealed class DomiNexShopService
    {
        private const int OfferCount = 3;
        private const int DirectDominoOfferChancePercent = 10;
        private const int MaxDirectDominoOffersPerShop = 1;
        private readonly Random random = new Random();
        private readonly ShopPackOfferGenerator packOfferGenerator;

        public DomiNexShopService()
        {
            packOfferGenerator = new ShopPackOfferGenerator(random);
        }

        public ShopState GenerateShop(DomiNexInventory inventory, int floorIndex)
        {
            return GenerateShop(inventory, floorIndex, null);
        }

        public ShopState GenerateShop(DomiNexInventory inventory, int floorIndex, RunState run)
        {
            var shop = new ShopState();
            RerollDirectDomiNexOffers(shop, inventory, floorIndex);
            AddBoosterPackOffers(shop);

            return shop;
        }

        public void RerollDirectDomiNexOffers(ShopState shop, DomiNexInventory inventory, int floorIndex)
        {
            if (shop == null)
            {
                return;
            }

            shop.Offers.Clear();
            var candidates = GetAvailableDomiNexPool(inventory).ToList();
            var dominoOffers = 0;

            for (var i = 0; i < OfferCount && candidates.Count > 0; i++)
            {
                if (dominoOffers < MaxDirectDominoOffersPerShop && random.Next(100) < DirectDominoOfferChancePercent)
                {
                    var domino = DominoShopOfferService.GenerateRandomShopDomino(random);
                    shop.Offers.Add(new ShopOffer(domino, DominoShopOfferService.GetPrice(domino)));
                    dominoOffers++;
                    continue;
                }

                var selected = PickWeighted(candidates, floorIndex);
                candidates.Remove(selected);
                shop.Offers.Add(new ShopOffer(selected, DomiNexPriceService.GeneratePrice(selected.Rarity, random)));
            }
        }

        public List<DomiNexDefinition> GenerateDomiNexPackChoices(DomiNexInventory inventory, int floorIndex, int count)
        {
            var candidates = GetAvailableDomiNexPool(inventory).ToList();
            var choices = new List<DomiNexDefinition>();
            while (choices.Count < count && candidates.Count > 0)
            {
                var selected = PickWeighted(candidates, floorIndex);
                candidates.Remove(selected);
                choices.Add(selected);
            }

            return choices;
        }

        public int CountAvailableDomiNexPackChoices(DomiNexInventory inventory)
        {
            return GetAvailableDomiNexPool(inventory).Count();
        }

        public static int GetPackVirtualPurchasePrice(DomiNexRarity rarity)
        {
            return rarity switch
            {
                DomiNexRarity.Common => 4,
                DomiNexRarity.Rare => 6,
                DomiNexRarity.Epic => 9,
                DomiNexRarity.Legendary => 20,
                _ => 4
            };
        }

        private void AddBoosterPackOffers(ShopState shop)
        {
            shop.BoosterPackOffers.AddRange(packOfferGenerator.GeneratePackOffers(2));
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

        private static IEnumerable<DomiNexDefinition> GetAvailableDomiNexPool(DomiNexInventory inventory)
        {
            return DomiNexRegistry.All
                .Where(definition => definition.Rarity != DomiNexRarity.Cursed)
                .Where(DomiNexRegistry.IsAvailableInPrototypeShop)
                .Where(definition => inventory == null || !inventory.Contains(definition.Id));
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
