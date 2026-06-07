using System.Collections.Generic;
using System.Linq;
using DomiNox.Patterns;

namespace DomiNox.Dominex
{
    public static class DomiNexRegistry
    {
        public static IReadOnlyList<string> PrototypeActiveIds { get; } = new string[0];
        public static IReadOnlyList<string> TemporarilyDisabledIds { get; } = new string[0];

        public static IReadOnlyList<DomiNexDefinition> All { get; } = new[]
        {
            Define("dominex", "Dominex", DomiNexRarity.Common, "+4 Mult.", Tags("flat", "mult"), Scoring(DomiNexEffectType.AddMult, 4)),
            Define("low_dominex", "Low Dominex", DomiNexRarity.Common, "Si tous les dominos joues ont une somme <= 5, +8 Mult.", Tags("low", "mult"), Scoring(DomiNexEffectType.AddMultIfAllSumsAtMost, 8, 5)),
            Define("silly_dominex", "Silly Dominex", DomiNexRarity.Common, "Si tous les dominos joues ont une somme <= 5, +50 Tile.", Tags("low", "count"), Scoring(DomiNexEffectType.AddCountIfAllSumsAtMost, 50, 5)),
            Define("high_dominex", "High Dominex", DomiNexRarity.Common, "Si tous les dominos joues ont une somme > 8, +6 Mult.", Tags("high", "mult"), Scoring(DomiNexEffectType.AddMultIfAllSumsAbove, 6, 8)),
            Define("willy_dominex", "Willy Dominex", DomiNexRarity.Common, "Si tous les dominos joues ont une somme > 8, +40 Tile.", Tags("high", "count"), Scoring(DomiNexEffectType.AddCountIfAllSumsAbove, 40, 8)),
            Define("twin_dominex", "Twin Dominex", DomiNexRarity.Common, "Si au moins 2 doubles sont joues, +10 Mult.", Tags("double", "mult"), Scoring(DomiNexEffectType.AddMultIfDoubleCountAtLeast, 10, 2)),
            Define("niwt_dominex", "Niwt Dominex", DomiNexRarity.Common, "Si au moins 2 doubles sont joues, +80 Tile.", Tags("double", "count"), Scoring(DomiNexEffectType.AddCountIfDoubleCountAtLeast, 80, 2)),
            Define("triplet_dominex", "Triplet Dominex", DomiNexRarity.Common, "Si au moins 3 doubles sont joues, +12 Mult.", Tags("double", "mult"), Scoring(DomiNexEffectType.AddMultIfDoubleCountAtLeast, 12, 3)),
            Define("telprit_dominex", "Telprit Dominex", DomiNexRarity.Common, "Si au moins 3 doubles sont joues, +100 Tile.", Tags("double", "count"), Scoring(DomiNexEffectType.AddCountIfDoubleCountAtLeast, 100, 3)),
            Define("straight_dominex", "Straight Dominex", DomiNexRarity.Common, "Si Small Tile Straight est joue, +70 Tile.", Tags("straight", "count"), Scoring(DomiNexEffectType.AddCountIfPattern, 70, 0, PatternNames.SmallTileStraightId)),
            Define("long_straight_dominex", "Long Straight Dominex", DomiNexRarity.Common, "Si Long Tile Straight est joue, +20 Mult.", Tags("straight", "mult"), Scoring(DomiNexEffectType.AddMultIfPatternId, 20, 0, PatternNames.LongTileStraightId)),
            Define("jacko_7even_dominex", "Jack'o 7even Dominex", DomiNexRarity.Common, "Chaque domino joue dont la somme vaut 7 donne +30 Tile et +3 Mult.", Tags("seven", "jackpot", "count", "mult"), Scoring(DomiNexEffectType.AddCountAndMultPerDominoSumExactly, 30, 7, "3")),
            Define("line_dominex", "Line Dominex", DomiNexRarity.Common, "Si Tile Line est joue, +60 Tile.", Tags("line", "design", "count"), Scoring(DomiNexEffectType.AddCountIfPattern, 60, 0, PatternNames.TileLineId)),
            Define("cross_dominex", "Cross Dominex", DomiNexRarity.Common, "Si Cross Tile est joue, +80 Tile. Si la croix est prolongee, +10 Mult par extension.", Tags("cross", "design", "count", "mult"), Scoring(DomiNexEffectType.AddCountIfPatternWithCrossExtensionMult, 80, 10, PatternNames.CrossTileId)),
            Define("looping_dominex", "Looping Dominex", DomiNexRarity.Common, "Si Tile Loop est joue, +100 Tile.", Tags("loop", "design", "count"), Scoring(DomiNexEffectType.AddCountIfPattern, 100, 0, PatternNames.TileLoopId)),
            Define("looper_dominex", "Looper Dominex", DomiNexRarity.Common, "Si Tile Loop est joue, +12 Mult.", Tags("loop", "design", "mult"), Scoring(DomiNexEffectType.AddMultIfPatternId, 12, 0, PatternNames.TileLoopId)),
            Define("half_dominex", "Half Dominex", DomiNexRarity.Common, "Si 3 dominos ou moins sont places, +20 Mult.", Tags("placed", "mult"), Scoring(DomiNexEffectType.AddMultIfPlacedAtMost, 20, 3)),
            Define("reroll_dominex", "Reroll", DomiNexRarity.Common, "Le premier reroll de chaque shop est gratuit.", Tags("shop", "reroll"), Future("Needs shop reroll action.")),
            Define("mystic", "Mystic", DomiNexRarity.Common, "Si tu valides avec 0 discard restant, +15 Mult.", Tags("discard", "mult"), Scoring(DomiNexEffectType.AddMultIfNoDiscardRemaining, 15)),
            Define("shallow", "Shallow", DomiNexRarity.Common, "+5 Mult par discard restant.", Tags("discard", "mult"), Scoring(DomiNexEffectType.AddMultPerDiscardRemaining, 5)),
            Define("credit_dominex", "Credit Dominex", DomiNexRarity.Common, "Tu peux acheter meme si tu descends jusqu'a -20 credits.", Tags("economy", "debt"), Future("Handled by shop credit limit.")),
            Define("gros_michel", "Gros Michel", DomiNexRarity.Common, "+15 Mult. Apres chaque main jouee, 1 chance sur 6 de detruire ce DomiNex.", Tags("mult", "chance", "destroy"), Scoring(DomiNexEffectType.AddMult, 15)),
            Define("even_dominex", "Even Dominex", DomiNexRarity.Common, "Chaque domino joue contenant uniquement des valeurs paires donne +4 Mult.", Tags("even", "mult"), Scoring(DomiNexEffectType.AddMultPerEvenDomino, 4)),
            Define("odd_dominex", "Odd Dominex", DomiNexRarity.Common, "Chaque domino joue contenant uniquement des valeurs impaires donne +30 Tile.", Tags("odd", "count"), Scoring(DomiNexEffectType.AddCountPerOddDomino, 30)),
            Define("scholar", "Scholar", DomiNexRarity.Common, "Chaque domino joue contenant un 0 donne +20 Tile et +4 Mult.", Tags("zero", "count", "mult"), Scoring(DomiNexEffectType.AddCountAndMultPerDominoContainingValue, 20, 0, "4")),

            Define("fibonacci", "Fibonacci", DomiNexRarity.Rare, "Chaque domino joue contenant 1, 3 ou 5 donne +5 Mult.", Tags("value", "mult"), Scoring(DomiNexEffectType.AddMultPerDominoContainingAnyValue, 5, 0, "1,3,5")),
            Define("empty_dominex", "Empty Dominex", DomiNexRarity.Rare, "Mult x nombre de slots DomiNex libres.", Tags("slots", "multiplier"), Scoring(DomiNexEffectType.MultiplyMultByFreeDomiNexSlots)),
            Define("doublish", "Doublish", DomiNexRarity.Rare, "Score final x1 + 0.2 par double dans ton sac.", Tags("double", "bag", "multiplier"), Scoring(DomiNexEffectType.AddFinalMultiplierByBagDoubleCount, 20)),
            Define("space_dominex", "Space Dominex", DomiNexRarity.Rare, "1 chance sur 4 d'ameliorer le niveau du Value Pattern joue de +1.", Tags("pattern_level", "chance"), Future("Handled by post-scoring."))
        };

        public static IReadOnlyList<DomiNexDefinition> GetPrototypeActiveSet()
        {
            return All.Where(IsAvailableInPrototypeShop).ToList();
        }

        public static DomiNexDefinition GetById(string id)
        {
            return All.FirstOrDefault(definition => definition.Id == id);
        }

        public static bool IsAvailableInPrototypeShop(DomiNexDefinition definition)
        {
            return definition != null
                && !TemporarilyDisabledIds.Contains(definition.Id)
                && (definition.Id == "credit_dominex" || definition.Id == "space_dominex" || definition.Effects.Any(effect => effect.Trigger != DomiNexTrigger.FutureHook));
        }

        private static DomiNexDefinition Define(string id, string name, DomiNexRarity rarity, string description, IReadOnlyList<string> tags, params DomiNexEffectDefinition[] effects)
        {
            return new DomiNexDefinition(id, name, rarity, description, tags, effects);
        }

        private static IReadOnlyList<string> Tags(params string[] tags) => tags;
        private static DomiNexEffectDefinition Scoring(DomiNexEffectType type, int value = 0, int threshold = 0, string note = "") => new DomiNexEffectDefinition(DomiNexTrigger.Scoring, type, value, threshold, note);
        private static DomiNexEffectDefinition LevelStart(DomiNexEffectType type, int value = 0, int threshold = 0) => new DomiNexEffectDefinition(DomiNexTrigger.LevelStart, type, value, threshold);
        private static DomiNexEffectDefinition RunStart(DomiNexEffectType type, int value = 0, int threshold = 0) => new DomiNexEffectDefinition(DomiNexTrigger.RunStart, type, value, threshold);
        private static DomiNexEffectDefinition Future(string note) => new DomiNexEffectDefinition(DomiNexTrigger.FutureHook, DomiNexEffectType.FutureHook, note: note);
    }
}
