using System.Collections.Generic;

namespace DomiNox.Patterns
{
    public static class PatternCatalog
    {
        public static IReadOnlyList<PatternInfo> ValuePatterns { get; } = new[]
        {
            new PatternInfo(PatternNames.JackpotSevenId, PatternNames.JackpotSeven, PatternCategory.Value, "Au moins 3 dominos joues ont une somme egale a 7.", "+50 Tile, +4 Mult.", 50, 4, 70, "7.7", ".7.", "..."),
            new PatternInfo(PatternNames.LongTileStraightId, PatternNames.LongTileStraight, PatternCategory.Value, "5 dominos connectes forment une suite exacte.", "+60 Tile, +7 Mult.", 60, 7, 60, "12", "23", "34", "45", "56"),
            new PatternInfo(PatternNames.SmallTileStraightId, PatternNames.SmallTileStraight, PatternCategory.Value, "3 dominos connectes forment une suite exacte.", "+30 Tile, +4 Mult.", 30, 4, 50, "12", "23", "34"),
            new PatternInfo(PatternNames.TripleDoubleId, PatternNames.TripleDouble, PatternCategory.Value, "Au moins 3 doubles sont joues.", "+20 Tile, +3 Mult.", 20, 3, 40, "D..", ".D.", "..D"),
            new PatternInfo(PatternNames.DoubleTileId, PatternNames.DoubleTile, PatternCategory.Value, "Au moins 2 doubles sont joues.", "+10 Tile, +3 Mult.", 10, 3, 30, "D.D", "...", "..."),
            new PatternInfo(PatternNames.HighTileId, PatternNames.HighTile, PatternCategory.Value, "Tous les dominos joues ont une somme strictement superieure a 8.", "+6 Tile, +2 Mult.", 6, 2, 20, "hi.", ".hi", "..."),
            new PatternInfo(PatternNames.LowTileId, PatternNames.LowTile, PatternCategory.Value, "Tous les dominos joues ont une somme inferieure ou egale a 5.", "+10 Tile, +2 Mult.", 10, 2, 10, "lo.", ".lo", "..."),
            new PatternInfo(PatternNames.TileHighId, PatternNames.TileHigh, PatternCategory.Value, "Pattern par defaut si aucun autre pattern de valeur n'est reconnu.", "+5 Tile, +1 Mult.", 5, 1, 0, ".H.", "...", "...")
        };

        public static IReadOnlyList<PatternInfo> DesignPatterns { get; } = new[]
        {
            new PatternInfo(PatternNames.BigLoopId, PatternNames.BigLoop, PatternCategory.Design, "Grande boucle fermee et connectee formant un carre 4x4.", "+100 Tile, +6 Mult.", 100, 6, 100, 1, 0, true, "XXXX", "X..X", "X..X", "XXXX"),
            new PatternInfo(PatternNames.TileLoopId, PatternNames.TileLoop, PatternCategory.Design, "Les dominos forment une boucle fermee avec connexions valides.", "+30 Tile, +3 Mult.", 30, 3, 80, "XXX", "X.X", "XXX"),
            new PatternInfo(PatternNames.ChristCrossId, PatternNames.ChristCross, PatternCategory.Design, "Forme de croix prolongee avec un centre clair et une branche verticale etendue.", "+50 Tile, +4 Mult.", 50, 4, 60, 1, 0, true, "..X..", "..X..", "XXXXX", "..X..", "..X..", "..X.."),
            new PatternInfo(PatternNames.CrossTileId, PatternNames.CrossTile, PatternCategory.Design, "Au moins 4 dominos forment une vraie croix autour d'un centre clair.", "+10 Tile, +2 Mult.", 10, 2, 40, ".X.", "XXX", ".X."),
            new PatternInfo(PatternNames.TileLineId, PatternNames.TileLine, PatternCategory.Design, "Au moins 3 dominos connectes sur une ligne horizontale ou verticale.", "+5 Tile, +2 Mult.", 5, 2, 20, "...", "XXXXX", "...")
        };

        public static IReadOnlyList<PatternInfo> BonusPatterns { get; } = new PatternInfo[0];

        public static IReadOnlyList<PatternInfo> All { get; } = BuildAll();

        public static PatternInfo GetByName(string name)
        {
            foreach (var pattern in All)
            {
                if (pattern.Name == name)
                {
                    return pattern;
                }
            }

            return null;
        }

        public static PatternInfo GetById(string id)
        {
            foreach (var pattern in All)
            {
                if (pattern.Id == id)
                {
                    return pattern;
                }
            }

            return null;
        }

        private static IReadOnlyList<PatternInfo> BuildAll()
        {
            var all = new List<PatternInfo>();
            all.AddRange(ValuePatterns);
            all.AddRange(DesignPatterns);
            all.AddRange(BonusPatterns);
            return all;
        }
    }
}
