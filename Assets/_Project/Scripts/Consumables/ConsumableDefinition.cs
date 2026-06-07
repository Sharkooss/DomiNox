namespace DomiNox.Consumables
{
    public sealed class ConsumableDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public ConsumableType Type { get; }
        public string TargetPatternId { get; }
        public int Price { get; }
        public string Description { get; }
        public string IconId { get; }

        public ConsumableDefinition(string id, string name, ConsumableType type, string targetPatternId, int price, string description, string iconId)
        {
            Id = id;
            Name = name;
            Type = type;
            TargetPatternId = targetPatternId;
            Price = price;
            Description = description;
            IconId = iconId;
        }
    }
}
