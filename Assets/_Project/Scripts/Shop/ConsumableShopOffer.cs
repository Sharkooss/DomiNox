using DomiNox.Consumables;

namespace DomiNox.Shop
{
    public sealed class ConsumableShopOffer
    {
        public ConsumableDefinition Consumable { get; }
        public int Price { get; }
        public bool IsPurchased { get; private set; }

        public ConsumableShopOffer(ConsumableDefinition consumable, int price)
        {
            Consumable = consumable;
            Price = price;
        }

        public void MarkPurchased()
        {
            IsPurchased = true;
        }
    }
}
