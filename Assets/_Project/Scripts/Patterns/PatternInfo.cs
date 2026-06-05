namespace DomiNox.Patterns
{
    public sealed class PatternInfo
    {
        public string Name { get; }
        public PatternCategory Category { get; }
        public string Requirement { get; }
        public string Effect { get; }
        public int ChipsBonus { get; }
        public int MultBonus { get; }
        public int Priority { get; }
        public string[] DiagramRows { get; }

        public PatternInfo(string name, PatternCategory category, string requirement, string effect, int chipsBonus, int multBonus, int priority, params string[] diagramRows)
        {
            Name = name;
            Category = category;
            Requirement = requirement;
            Effect = effect;
            ChipsBonus = chipsBonus;
            MultBonus = multBonus;
            Priority = priority;
            DiagramRows = diagramRows;
        }
    }
}
