using DomiNox.Consumables;
using DomiNox.Dominex;

namespace DomiNox.Shop
{
    public static class SellValueService
    {
        public static int GetDomiNexSellValue(ActiveDomiNexInstance instance)
        {
            if (instance == null)
            {
                return 0;
            }

            var purchasePrice = instance.PurchasePrice > 0 ? instance.PurchasePrice : GetFallbackPurchasePrice(instance.Definition);
            return System.Math.Max(1, purchasePrice / 2);
        }

        public static int GetConsumableSellValue(ConsumableInstance instance)
        {
            return instance == null ? 0 : System.Math.Max(1, instance.SellValue);
        }

        private static int GetFallbackPurchasePrice(DomiNexDefinition definition)
        {
            if (definition == null)
            {
                return 1;
            }

            return definition.Rarity switch
            {
                DomiNexRarity.Common => 2,
                DomiNexRarity.Rare => 4,
                DomiNexRarity.Epic => 8,
                DomiNexRarity.Legendary => 20,
                _ => 1
            };
        }
    }
}
