namespace DomiNox.Jackpot
{
    public sealed class JackpotRewardDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public JackpotRewardType Type { get; }
        public string Description { get; }
        public bool IsImplemented { get; }

        public JackpotRewardDefinition(string id, string name, JackpotRewardType type, string description, bool isImplemented = true)
        {
            Id = id;
            Name = name;
            Type = type;
            Description = description;
            IsImplemented = isImplemented;
        }
    }
}
