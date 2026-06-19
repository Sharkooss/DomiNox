using DomiNox.Dominex;
using DomiNox.Dominoes;

namespace DomiNox.Shop
{
    public sealed class ShopOffer
    {
        public ShopOfferType Type { get; }
        public DomiNexDefinition DomiNex { get; }
        public DominoDefinition Domino { get; }
        public int Price { get; }
        public bool IsPurchased { get; private set; }

        public ShopOffer(DomiNexDefinition domiNex, int price)
        {
            Type = ShopOfferType.DomiNex;
            DomiNex = domiNex;
            Price = price;
        }

        public ShopOffer(DominoDefinition domino, int price)
        {
            Type = ShopOfferType.Domino;
            Domino = domino;
            Price = price;
        }

        public void MarkPurchased()
        {
            IsPurchased = true;
        }
    }
}
