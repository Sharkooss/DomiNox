using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Shop
{
    public static class BoosterPackRegistry
    {
        public static IReadOnlyList<BoosterPackDefinition> All { get; } = new[]
        {
            new BoosterPackDefinition("gemstone_pack_normal", "Gemstone Pack", BoosterPackType.Normal, 4, 3, 1, "Choose 1 of 3 Gemstone Tiles."),
            new BoosterPackDefinition("gemstone_pack_jumbo", "Jumbo Gemstone Pack", BoosterPackType.Jumbo, 6, 5, 1, "Choose 1 of 5 Gemstone Tiles."),
            new BoosterPackDefinition("gemstone_pack_mega", "Mega Gemstone Pack", BoosterPackType.Mega, 8, 5, 2, "Choose 2 of 5 Gemstone Tiles.")
        };

        public static BoosterPackDefinition GetById(string id) => All.FirstOrDefault(pack => pack.Id == id);
    }
}
