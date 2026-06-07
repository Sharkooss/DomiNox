namespace DomiNox.Shop
{
    public sealed class BoosterPackDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public BoosterPackType Type { get; }
        public int Price { get; }
        public int OfferedCardCount { get; }
        public int PickCount { get; }
        public string Description { get; }

        public BoosterPackDefinition(string id, string name, BoosterPackType type, int price, int offeredCardCount, int pickCount, string description)
        {
            Id = id;
            Name = name;
            Type = type;
            Price = price;
            OfferedCardCount = offeredCardCount;
            PickCount = pickCount;
            Description = description;
        }
    }
}
