namespace DomiNox.Patterns
{
    public sealed class PatternInfo
    {
        public string Id { get; }
        public string Name { get; }
        public PatternCategory Category { get; }
        public string Requirement { get; }
        public string Effect { get; }
        public int ChipsBonus { get; }
        public int MultBonus { get; }
        public int Priority { get; }
        public int Level { get; }
        public int ExtensionCount { get; }
        public bool IsSecret { get; }
        public string[] DiagramRows { get; }

        public PatternInfo(string id, string name, PatternCategory category, string requirement, string effect, int chipsBonus, int multBonus, int priority, params string[] diagramRows)
            : this(id, name, category, requirement, effect, chipsBonus, multBonus, priority, 1, 0, false, diagramRows)
        {
        }

        public PatternInfo(string id, string name, PatternCategory category, string requirement, string effect, int chipsBonus, int multBonus, int priority, int level, int extensionCount, params string[] diagramRows)
            : this(id, name, category, requirement, effect, chipsBonus, multBonus, priority, level, extensionCount, false, diagramRows)
        {
        }

        public PatternInfo(string id, string name, PatternCategory category, string requirement, string effect, int chipsBonus, int multBonus, int priority, int level, int extensionCount, bool isSecret, params string[] diagramRows)
        {
            Id = id;
            Name = name;
            Category = category;
            Requirement = requirement;
            Effect = effect;
            ChipsBonus = chipsBonus;
            MultBonus = multBonus;
            Priority = priority;
            Level = level;
            ExtensionCount = extensionCount;
            IsSecret = isSecret;
            DiagramRows = diagramRows;
        }

        public PatternInfo WithRuntimeData(int level, int extensionCount = 0)
        {
            return new PatternInfo(Id, Name, Category, Requirement, Effect, ChipsBonus, MultBonus, Priority, level, extensionCount, IsSecret, DiagramRows);
        }
    }
}
