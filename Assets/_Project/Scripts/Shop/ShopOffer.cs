using DomiNox.Dominex;

namespace DomiNox.Shop
{
    public sealed class ShopOffer
    {
        public DomiNexDefinition DomiNex { get; }
        public int Price { get; }
        public bool IsPurchased { get; private set; }

        public ShopOffer(DomiNexDefinition domiNex, int price)
        {
            DomiNex = domiNex;
            Price = price;
        }

        public void MarkPurchased()
        {
            IsPurchased = true;
        }
    }
}
