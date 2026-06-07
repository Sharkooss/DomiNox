namespace DomiNox.Scoring
{
    public sealed class ScoringPreview
    {
        public string PatternId { get; }
        public string PatternName { get; }
        public int Level { get; }
        public int CountBonus { get; }
        public int MultBonus { get; }
        public string Detail { get; }
        public bool IsCombo { get; }

        public ScoringPreview(string patternId, string patternName, int level, int countBonus, int multBonus, string detail = null, bool isCombo = false)
        {
            PatternId = patternId;
            PatternName = patternName;
            Level = level;
            CountBonus = countBonus;
            MultBonus = multBonus;
            Detail = detail;
            IsCombo = isCombo;
        }
    }
}
