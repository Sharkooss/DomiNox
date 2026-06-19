using UnityEngine;

namespace DomiNox.Dominoes
{
    public sealed class DominoModifierDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public Color ColorTheme { get; }
        public string Badge { get; }

        public DominoModifierDefinition(string id, string name, string description, Color colorTheme, string badge)
        {
            Id = id;
            Name = name;
            Description = description;
            ColorTheme = colorTheme;
            Badge = badge;
        }
    }
}
