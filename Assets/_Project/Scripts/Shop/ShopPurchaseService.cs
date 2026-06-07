using DomiNox.Run;

namespace DomiNox.Shop
{
    public static class ShopPurchaseService
    {
        public static bool CanAfford(int price, RunState run, int minimumCredits)
        {
            return run != null && run.Credits - price >= minimumCredits;
        }

        public static bool TrySpend(int price, RunState run, int minimumCredits)
        {
            if (!CanAfford(price, run, minimumCredits))
            {
                return false;
            }

            run.Credits -= price;
            return true;
        }
    }
}
