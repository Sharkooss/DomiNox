using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Patterns
{
    public static class PatternComboCatalog
    {
        public static IReadOnlyList<PatternComboDefinition> All { get; } = new[]
        {
            Define("high_line", "High Line", PatternNames.TileHighId, PatternNames.TileLineId, 10, 1, PatternComboDifficulty.Easy),
            Define("low_line", "Low Line", PatternNames.LowTileId, PatternNames.TileLineId, 20, 2, PatternComboDifficulty.Easy),
            Define("high_roll_line", "High Roll Line", PatternNames.HighTileId, PatternNames.TileLineId, 25, 2, PatternComboDifficulty.Easy),
            Define("double_line", "Double Line", PatternNames.DoubleTileId, PatternNames.TileLineId, 35, 3, PatternComboDifficulty.Medium),
            Define("triple_line", "Triple Line", PatternNames.TripleDoubleId, PatternNames.TileLineId, 60, 4, PatternComboDifficulty.Medium),
            Define("small_straight_line", "Small Straight Line", PatternNames.SmallTileStraightId, PatternNames.TileLineId, 50, 4, PatternComboDifficulty.Medium),
            Define("long_straight_line", "Long Straight Line", PatternNames.LongTileStraightId, PatternNames.TileLineId, 100, 6, PatternComboDifficulty.Hard),
            Define("jackpot_line", "Jackpot Line", PatternNames.JackpotSevenId, PatternNames.TileLineId, 80, 5, PatternComboDifficulty.Medium),

            Define("high_cross", "High Cross", PatternNames.TileHighId, PatternNames.CrossTileId, 25, 2, PatternComboDifficulty.Medium),
            Define("low_cross", "Low Cross", PatternNames.LowTileId, PatternNames.CrossTileId, 40, 3, PatternComboDifficulty.Medium),
            Define("high_roll_cross", "High Roll Cross", PatternNames.HighTileId, PatternNames.CrossTileId, 45, 3, PatternComboDifficulty.Medium),
            Define("double_cross", "Double Cross", PatternNames.DoubleTileId, PatternNames.CrossTileId, 70, 5, PatternComboDifficulty.Hard),
            Define("triple_cross", "Triple Cross", PatternNames.TripleDoubleId, PatternNames.CrossTileId, 110, 7, PatternComboDifficulty.Hard),
            Define("small_straight_cross", "Small Straight Cross", PatternNames.SmallTileStraightId, PatternNames.CrossTileId, 90, 6, PatternComboDifficulty.Hard),
            Define("long_straight_cross", "Long Straight Cross", PatternNames.LongTileStraightId, PatternNames.CrossTileId, 160, 9, PatternComboDifficulty.VeryHard),
            Define("jackpot_cross", "Jackpot Cross", PatternNames.JackpotSevenId, PatternNames.CrossTileId, 130, 8, PatternComboDifficulty.Hard),

            Define("high_loop", "High Loop", PatternNames.TileHighId, PatternNames.TileLoopId, 50, 4, PatternComboDifficulty.Hard),
            Define("low_loop", "Low Loop", PatternNames.LowTileId, PatternNames.TileLoopId, 70, 5, PatternComboDifficulty.Hard),
            Define("high_roll_loop", "High Roll Loop", PatternNames.HighTileId, PatternNames.TileLoopId, 80, 5, PatternComboDifficulty.Hard),
            Define("double_loop", "Double Loop", PatternNames.DoubleTileId, PatternNames.TileLoopId, 120, 8, PatternComboDifficulty.VeryHard),
            Define("triple_loop", "Triple Loop", PatternNames.TripleDoubleId, PatternNames.TileLoopId, 180, 11, PatternComboDifficulty.VeryHard),
            Define("small_straight_loop", "Small Straight Loop", PatternNames.SmallTileStraightId, PatternNames.TileLoopId, 150, 9, PatternComboDifficulty.VeryHard),
            Define("long_straight_loop", "Long Straight Loop", PatternNames.LongTileStraightId, PatternNames.TileLoopId, 250, 14, PatternComboDifficulty.Legendary),
            Define("jackpot_loop", "Jackpot Loop", PatternNames.JackpotSevenId, PatternNames.TileLoopId, 220, 12, PatternComboDifficulty.Legendary)
            ,

            Define("high_christ_cross", "High Christ Cross", PatternNames.TileHighId, PatternNames.ChristCrossId, 60, 5, PatternComboDifficulty.Hard),
            Define("low_christ_cross", "Low Christ Cross", PatternNames.LowTileId, PatternNames.ChristCrossId, 85, 6, PatternComboDifficulty.Hard),
            Define("high_roll_christ_cross", "High Roll Christ Cross", PatternNames.HighTileId, PatternNames.ChristCrossId, 95, 6, PatternComboDifficulty.Hard),
            Define("double_christ_cross", "Double Christ Cross", PatternNames.DoubleTileId, PatternNames.ChristCrossId, 130, 9, PatternComboDifficulty.VeryHard),
            Define("triple_christ_cross", "Triple Christ Cross", PatternNames.TripleDoubleId, PatternNames.ChristCrossId, 190, 12, PatternComboDifficulty.VeryHard),
            Define("small_straight_christ_cross", "Small Straight Christ Cross", PatternNames.SmallTileStraightId, PatternNames.ChristCrossId, 160, 10, PatternComboDifficulty.VeryHard),
            Define("long_straight_christ_cross", "Long Straight Christ Cross", PatternNames.LongTileStraightId, PatternNames.ChristCrossId, 280, 16, PatternComboDifficulty.Legendary),
            Define("jackpot_christ_cross", "Jackpot Christ Cross", PatternNames.JackpotSevenId, PatternNames.ChristCrossId, 250, 15, PatternComboDifficulty.Legendary)
            ,

            Define("high_big_loop", "High Big Loop", PatternNames.TileHighId, PatternNames.BigLoopId, 120, 8, PatternComboDifficulty.VeryHard),
            Define("low_big_loop", "Low Big Loop", PatternNames.LowTileId, PatternNames.BigLoopId, 150, 9, PatternComboDifficulty.VeryHard),
            Define("high_roll_big_loop", "High Roll Big Loop", PatternNames.HighTileId, PatternNames.BigLoopId, 170, 9, PatternComboDifficulty.VeryHard),
            Define("double_big_loop", "Double Big Loop", PatternNames.DoubleTileId, PatternNames.BigLoopId, 220, 12, PatternComboDifficulty.Legendary),
            Define("triple_big_loop", "Triple Big Loop", PatternNames.TripleDoubleId, PatternNames.BigLoopId, 300, 16, PatternComboDifficulty.Legendary),
            Define("small_straight_big_loop", "Small Straight Big Loop", PatternNames.SmallTileStraightId, PatternNames.BigLoopId, 260, 14, PatternComboDifficulty.Legendary),
            Define("long_straight_big_loop", "Long Straight Big Loop", PatternNames.LongTileStraightId, PatternNames.BigLoopId, 420, 22, PatternComboDifficulty.Mythic),
            Define("jackpot_big_loop", "Jackpot Big Loop", PatternNames.JackpotSevenId, PatternNames.BigLoopId, 380, 20, PatternComboDifficulty.Mythic)
        };

        public static bool TryGetCombo(string valuePatternId, string designPatternId, out PatternComboDefinition combo)
        {
            combo = All.FirstOrDefault(definition => definition.ValuePatternId == valuePatternId && definition.DesignPatternId == designPatternId);
            return combo != null;
        }

        private static PatternComboDefinition Define(string id, string name, string valuePatternId, string designPatternId, int countBonus, int multBonus, PatternComboDifficulty difficulty)
        {
            return new PatternComboDefinition(id, name, valuePatternId, designPatternId, countBonus, multBonus, difficulty, $"{PatternCatalog.GetById(valuePatternId)?.Name} combine avec {PatternCatalog.GetById(designPatternId)?.Name}.");
        }
    }
}
