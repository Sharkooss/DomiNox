using System;
using DomiNox.Core;
using DomiNox.Dominoes;

namespace DomiNox.Shop
{
    public static class DominoShopOfferService
    {
        public static DominoDefinition GenerateRandomShopDomino(Random random)
        {
            random ??= new Random();
            var left = random.Next(GameConstants.DominoMinValue, GameConstants.DominoMaxValue + 1);
            var right = random.Next(GameConstants.DominoMinValue, GameConstants.DominoMaxValue + 1);
            if (left > right)
            {
                (left, right) = (right, left);
            }

            return new DominoDefinition($"shop_domino_{left}_{right}", left, right);
        }

        public static int GetPrice(DominoDefinition domino)
        {
            if (domino == null)
            {
                return 3;
            }

            var price = 3;
            if (domino.IsDouble)
            {
                price += 1;
            }

            if (domino.Sum == 7)
            {
                price += 1;
            }

            if (domino.Sum >= 10)
            {
                price += 1;
            }

            return Math.Max(2, Math.Min(6, price));
        }
    }
}
