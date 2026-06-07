using DomiNox.Core;

namespace DomiNox.Run
{
    public static class LevelQuotaService
    {
        private static readonly int[][] DefinedQuotas =
        {
            new[] { 300, 450, 600, 750, 1200 },
            new[] { 2000, 3000, 4000, 5000, 7500 },
            new[] { 12000, 18000, 26000, 36000, 55000 }
        };

        public static int GetQuota(int floorIndex, int levelInFloor)
        {
            floorIndex = System.Math.Max(1, floorIndex);
            levelInFloor = System.Math.Max(1, System.Math.Min(GameConstants.LevelsPerFloor, levelInFloor));

            if (floorIndex <= DefinedQuotas.Length)
            {
                return DefinedQuotas[floorIndex - 1][levelInFloor - 1];
            }

            var previous = GetQuota(floorIndex - 1, levelInFloor);
            var multiplier = levelInFloor == GameConstants.LevelsPerFloor ? 2.3f : 2.2f;
            return RoundReadable(previous * multiplier);
        }

        public static int GetQuotaForGlobalLevel(int levelIndex)
        {
            var floorIndex = ((System.Math.Max(1, levelIndex) - 1) / GameConstants.LevelsPerFloor) + 1;
            var levelInFloor = ((System.Math.Max(1, levelIndex) - 1) % GameConstants.LevelsPerFloor) + 1;
            return GetQuota(floorIndex, levelInFloor);
        }

        public static int RoundReadable(float value)
        {
            var step = value < 10000f ? 100 : value < 100000f ? 1000 : 5000;
            return (int)(System.Math.Round(value / step) * step);
        }
    }
}
