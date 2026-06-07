namespace DomiNox.Shop
{
    public sealed class BoosterPackShopOffer
    {
        public BoosterPackDefinition Pack { get; }
        public bool IsPurchased { get; private set; }

        public BoosterPackShopOffer(BoosterPackDefinition pack)
        {
            Pack = pack;
        }

        public void MarkPurchased()
        {
            IsPurchased = true;
        }
    }
}
