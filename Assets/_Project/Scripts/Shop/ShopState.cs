using System.Collections.Generic;

namespace DomiNox.Shop
{
    public sealed class ShopState
    {
        public List<ShopOffer> Offers { get; } = new List<ShopOffer>();
        public List<BoosterPackShopOffer> BoosterPackOffers { get; } = new List<BoosterPackShopOffer>();
        public int RerollCountThisShop { get; set; }
    }
}
