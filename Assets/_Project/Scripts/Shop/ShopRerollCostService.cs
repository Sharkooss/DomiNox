using DomiNox.Run;

namespace DomiNox.Shop
{
    public static class ShopRerollCostService
    {
        public const int BaseRerollCost = 5;

        public static int GetCurrentRerollCost(RunState run, ShopState shop)
        {
            if (run == null || shop == null)
            {
                return BaseRerollCost;
            }

            if (run.NextShopInfiniteFreeRerolls || (run.NextShopFreeReroll && shop.RerollCountThisShop == 0))
            {
                return 0;
            }

            var cost = BaseRerollCost + shop.RerollCountThisShop;
            return System.Math.Max(0, cost);
        }

        public static bool ShouldCountSuccessfulReroll(RunState run, ShopState shop, int paidCost)
        {
            return run == null || (!run.NextShopInfiniteFreeRerolls && !(run.NextShopFreeReroll && paidCost == 0));
        }
    }
}
