namespace DomiNox.Scoring
{
    public sealed class PatternComboResult
    {
        public string Id { get; }
        public string Name { get; }
        public string ValuePatternId { get; }
        public string ValuePatternName { get; }
        public int ValuePatternLevel { get; }
        public string DesignPatternId { get; }
        public string DesignPatternName { get; }
        public int DesignPatternLevel { get; }
        public int ComboCountBonus { get; }
        public int ComboMultBonus { get; }
        public int PreviewCount { get; }
        public int PreviewMult { get; }

        public PatternComboResult(string id, string name, string valuePatternId, string valuePatternName, int valuePatternLevel, string designPatternId, string designPatternName, int designPatternLevel, int comboCountBonus, int comboMultBonus, int previewCount, int previewMult)
        {
            Id = id;
            Name = name;
            ValuePatternId = valuePatternId;
            ValuePatternName = valuePatternName;
            ValuePatternLevel = valuePatternLevel;
            DesignPatternId = designPatternId;
            DesignPatternName = designPatternName;
            DesignPatternLevel = designPatternLevel;
            ComboCountBonus = comboCountBonus;
            ComboMultBonus = comboMultBonus;
            PreviewCount = previewCount;
            PreviewMult = previewMult;
        }
    }
}
