using System;
using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Shop
{
    public sealed class ShopPackOfferGenerator
    {
        private readonly Random random;

        public ShopPackOfferGenerator(Random random = null)
        {
            this.random = random ?? new Random();
        }

        public List<BoosterPackShopOffer> GeneratePackOffers(int count)
        {
            var offers = new List<BoosterPackShopOffer>();
            for (var i = 0; i < count; i++)
            {
                var pack = PickPack(offers.Select(offer => offer.Pack.Id).ToHashSet());
                offers.Add(new BoosterPackShopOffer(pack));
            }

            return offers;
        }

        public BoosterPackDefinition PickPack(IReadOnlyCollection<string> avoidExactPackIds = null)
        {
            var contentType = PickContentType();
            var size = PickPackSize();
            var pack = FindPack(contentType, size);
            if (pack != null && avoidExactPackIds?.Contains(pack.Id) != true)
            {
                return pack;
            }

            var alternatives = BoosterPackRegistry.All
                .Where(candidate => avoidExactPackIds == null || !avoidExactPackIds.Contains(candidate.Id))
                .ToList();
            return alternatives.Count > 0 ? alternatives[random.Next(alternatives.Count)] : pack ?? BoosterPackRegistry.All[0];
        }

        public BoosterPackContentType PickContentType()
        {
            var roll = random.Next(100);
            if (roll < 35)
            {
                return BoosterPackContentType.GemstoneTile;
            }

            return roll < 70 ? BoosterPackContentType.DomiNex : BoosterPackContentType.Domino;
        }

        public BoosterPackType PickPackSize()
        {
            var roll = random.Next(100);
            if (roll < 75)
            {
                return BoosterPackType.Normal;
            }

            return roll < 95 ? BoosterPackType.Jumbo : BoosterPackType.Mega;
        }

        private static BoosterPackDefinition FindPack(BoosterPackContentType contentType, BoosterPackType size)
        {
            return BoosterPackRegistry.All.FirstOrDefault(pack => pack.ContentType == contentType && pack.Type == size);
        }
    }
}
