namespace DomiNox.Patterns
{
    public static class PatternScalingService
    {
        public static PatternBonus GetScaledPatternBonus(PatternInfo pattern, int level)
        {
            level = level < 1 ? 1 : level;
            var count = (int)System.Math.Round(pattern.ChipsBonus * (1f + (0.55f * (level - 1))));
            var mult = pattern.MultBonus + (int)System.Math.Floor((level - 1) * 1.25f);
            return new PatternBonus(count, mult);
        }
    }

    public readonly struct PatternBonus
    {
        public int Count { get; }
        public int Mult { get; }

        public PatternBonus(int count, int mult)
        {
            Count = count;
            Mult = mult;
        }
    }
}
