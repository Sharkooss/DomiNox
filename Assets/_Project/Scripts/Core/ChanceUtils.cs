using System;

namespace DomiNox.Core
{
    public static class ChanceUtils
    {
        private static readonly Random Random = new Random();

        public static bool RollChance(int numerator, int denominator)
        {
            if (denominator <= 0 || numerator <= 0)
            {
                return false;
            }

            if (numerator >= denominator)
            {
                return true;
            }

            return Random.Next(0, denominator) < numerator;
        }
    }
}
