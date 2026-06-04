using System.Collections.Generic;

namespace DomiNox.Dominex
{
    public sealed class DomiNexDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public DomiNexRarity Rarity { get; }
        public string Description { get; }
        public IReadOnlyList<string> Tags { get; }
        public IReadOnlyList<DomiNexEffectDefinition> Effects { get; }

        public DomiNexDefinition(string id, string name, DomiNexRarity rarity, string description, IReadOnlyList<string> tags, IReadOnlyList<DomiNexEffectDefinition> effects)
        {
            Id = id;
            Name = name;
            Rarity = rarity;
            Description = description;
            Tags = tags;
            Effects = effects;
        }
    }
}
