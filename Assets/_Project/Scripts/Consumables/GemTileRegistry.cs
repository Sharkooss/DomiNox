using System.Collections.Generic;
using System.Linq;
using DomiNox.Core;
using DomiNox.Patterns;
using DomiNox.Run;

namespace DomiNox.Consumables
{
    public static class GemTileRegistry
    {
        public static IReadOnlyList<ConsumableDefinition> All { get; } = new[]
        {
            Define("quartz_tile", "Quartz Tile", PatternNames.TileHighId, 3, "QT"),
            Define("sapphire_tile", "Sapphire Tile", PatternNames.LowTileId, 4, "SA"),
            Define("ruby_tile", "Ruby Tile", PatternNames.HighTileId, 4, "RU"),
            Define("opal_tile", "Opal Tile", PatternNames.DoubleTileId, 5, "OP"),
            Define("amethyst_tile", "Amethyst Tile", PatternNames.TripleDoubleId, 6, "AM"),
            Define("topaz_tile", "Topaz Tile", PatternNames.SmallTileStraightId, 5, "TO"),
            Define("emerald_tile", "Emerald Tile", PatternNames.LongTileStraightId, 8, "EM"),
            Define("diamond_tile", "Diamond Tile", PatternNames.JackpotSevenId, 7, "DI"),
            Define("onyx_tile", "Onyx Tile", PatternNames.TileLineId, 4, "ON"),
            Define("jade_tile", "Jade Tile", PatternNames.CrossTileId, 6, "JA"),
            Define("obsidian_tile", "Obsidian Tile", PatternNames.TileLoopId, 8, "OB"),
            Define("garnet_tile", "Garnet Tile", PatternNames.ChristCrossId, 9, "GA"),
            Define("lapis_tile", "Lapis Tile", PatternNames.BigLoopId, 10, "LA")
        };

        public static ConsumableDefinition GetById(string id)
        {
            return All.FirstOrDefault(definition => definition.Id == id);
        }

        public static IReadOnlyList<ConsumableDefinition> GetAvailableGemTiles(RunState run)
        {
            return All
                .Where(definition => CollectableVisibilityService.CanGemTileAppearInPack(definition, run))
                .Where(definition => !run.PatternLevels.IsMaxLevel(definition.TargetPatternId))
                .ToList();
        }

        public static bool CanUseConsumable(ConsumableDefinition definition, RunState run)
        {
            return definition != null && definition.Type == ConsumableType.GemTile && !run.PatternLevels.IsMaxLevel(definition.TargetPatternId);
        }

        public static bool UseConsumable(int index, RunState run, out string message)
        {
            message = "Invalid consumable.";
            if (index < 0 || index >= run.Consumables.Count)
            {
                return false;
            }

            var definition = GetById(run.Consumables.ActiveIds[index]);
            if (!CanUseConsumable(definition, run))
            {
                message = "Max level reached.";
                return false;
            }

            run.PatternLevels.Increase(definition.TargetPatternId);
            run.Consumables.RemoveAt(index);
            var pattern = PatternCatalog.GetById(definition.TargetPatternId);
            message = $"{pattern?.Name ?? definition.TargetPatternId} upgraded to Lv.{run.PatternLevels.GetLevel(definition.TargetPatternId)}";
            return true;
        }

        private static ConsumableDefinition Define(string id, string name, string targetPatternId, int price, string iconId)
        {
            var patternName = PatternCatalog.GetById(targetPatternId)?.Name ?? targetPatternId;
            return new ConsumableDefinition(id, name, ConsumableType.GemTile, targetPatternId, price, $"Upgrade {patternName} by 1 level.", iconId);
        }
    }
}
