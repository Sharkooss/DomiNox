namespace DomiNox.Patterns
{
    public sealed class PatternComboDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public string ValuePatternId { get; }
        public string DesignPatternId { get; }
        public int CountBonus { get; }
        public int MultBonus { get; }
        public PatternComboDifficulty Difficulty { get; }
        public string Description { get; }

        public PatternComboDefinition(string id, string name, string valuePatternId, string designPatternId, int countBonus, int multBonus, PatternComboDifficulty difficulty, string description)
        {
            Id = id;
            Name = name;
            ValuePatternId = valuePatternId;
            DesignPatternId = designPatternId;
            CountBonus = countBonus;
            MultBonus = multBonus;
            Difficulty = difficulty;
            Description = description;
        }
    }
}
