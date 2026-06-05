namespace DomiNox.Patterns
{
    public sealed class PatternInfo
    {
        public string Name { get; }
        public string Requirement { get; }
        public string Effect { get; }

        public PatternInfo(string name, string requirement, string effect)
        {
            Name = name;
            Requirement = requirement;
            Effect = effect;
        }
    }
}
