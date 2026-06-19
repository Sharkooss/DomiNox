using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Shop
{
    public static class BoosterPackRegistry
    {
        public static IReadOnlyList<BoosterPackDefinition> All { get; } = new[]
        {
            new BoosterPackDefinition("gemstone_pack_normal", "Gemstone Pack", BoosterPackContentType.GemstoneTile, BoosterPackType.Normal, 4, 3, 1, 1, "Choose 1 of 3 Gemstone Tiles."),
            new BoosterPackDefinition("gemstone_pack_jumbo", "Jumbo Gemstone Pack", BoosterPackContentType.GemstoneTile, BoosterPackType.Jumbo, 6, 5, 1, 1, "Choose 1 of 5 Gemstone Tiles."),
            new BoosterPackDefinition("gemstone_pack_mega", "Mega Gemstone Pack", BoosterPackContentType.GemstoneTile, BoosterPackType.Mega, 8, 5, 0, 2, "Choose up to 2 of 5 Gemstone Tiles."),
            new BoosterPackDefinition("dominex_pack_normal", "DomiNex Pack", BoosterPackContentType.DomiNex, BoosterPackType.Normal, 6, 3, 1, 1, "Choose 1 of 3 DomiNex."),
            new BoosterPackDefinition("dominex_pack_jumbo", "Jumbo DomiNex Pack", BoosterPackContentType.DomiNex, BoosterPackType.Jumbo, 9, 5, 1, 1, "Choose 1 of 5 DomiNex."),
            new BoosterPackDefinition("dominex_pack_mega", "Mega DomiNex Pack", BoosterPackContentType.DomiNex, BoosterPackType.Mega, 12, 5, 0, 2, "Choose up to 2 of 5 DomiNex."),
            new BoosterPackDefinition("domino_pack_normal", "Domino Pack", BoosterPackContentType.Domino, BoosterPackType.Normal, 4, 3, 1, 1, "Choose 1 of 3 Dominoes."),
            new BoosterPackDefinition("domino_pack_jumbo", "Jumbo Domino Pack", BoosterPackContentType.Domino, BoosterPackType.Jumbo, 6, 5, 1, 1, "Choose 1 of 5 Dominoes."),
            new BoosterPackDefinition("domino_pack_mega", "Mega Domino Pack", BoosterPackContentType.Domino, BoosterPackType.Mega, 8, 5, 0, 2, "Choose up to 2 of 5 Dominoes.")
        };

        public static BoosterPackDefinition GetById(string id) => All.FirstOrDefault(pack => pack.Id == id);
    }
}
