namespace DomiNox.Shop
{
    public sealed class BoosterPackDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public BoosterPackContentType ContentType { get; }
        public BoosterPackType Type { get; }
        public int Price { get; }
        public int OfferedCardCount { get; }
        public int PickMin { get; }
        public int PickMax { get; }
        public int PickCount => PickMax;
        public string Description { get; }

        public BoosterPackDefinition(string id, string name, BoosterPackType type, int price, int offeredCardCount, int pickCount, string description)
            : this(id, name, BoosterPackContentType.GemstoneTile, type, price, offeredCardCount, type == BoosterPackType.Mega ? 0 : pickCount, pickCount, description)
        {
        }

        public BoosterPackDefinition(string id, string name, BoosterPackContentType contentType, BoosterPackType type, int price, int offeredCardCount, int pickMin, int pickMax, string description)
        {
            Id = id;
            Name = name;
            ContentType = contentType;
            Type = type;
            Price = price;
            OfferedCardCount = offeredCardCount;
            PickMin = pickMin;
            PickMax = pickMax;
            Description = description;
        }
    }
}
