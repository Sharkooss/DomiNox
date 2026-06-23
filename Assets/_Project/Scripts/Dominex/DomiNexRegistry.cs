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
            // ====================================================================================
            // COMMON — petites briques, faibles seules, faites pour se combiner.
            // ====================================================================================
            Define("dominex", "Dominex", DomiNexRarity.Common, "+4 Mult.", Tags("flat", "mult"), Scoring(DomiNexEffectType.AddMult, 4)),
            Define("tile_dominex", "Tile Dominex", DomiNexRarity.Common, "+22 Tile.", Tags("flat", "count"), Scoring(DomiNexEffectType.AddCount, 22)),
            Define("low_dominex", "Low Dominex", DomiNexRarity.Common, "Si tous les dominos joues ont une somme <= 5, +6 Mult.", Tags("low", "mult"), Scoring(DomiNexEffectType.AddMultIfAllSumsAtMost, 6, 5)),
            Define("silly_dominex", "Silly Dominex", DomiNexRarity.Common, "Si tous les dominos joues ont une somme <= 5, +35 Tile.", Tags("low", "count"), Scoring(DomiNexEffectType.AddCountIfAllSumsAtMost, 35, 5)),
            Define("high_dominex", "High Dominex", DomiNexRarity.Common, "Si tous les dominos joues ont une somme > 8, +5 Mult.", Tags("high", "mult"), Scoring(DomiNexEffectType.AddMultIfAllSumsAbove, 5, 8)),
            Define("willy_dominex", "Willy Dominex", DomiNexRarity.Common, "Si tous les dominos joues ont une somme > 8, +30 Tile.", Tags("high", "count"), Scoring(DomiNexEffectType.AddCountIfAllSumsAbove, 30, 8)),
            Define("twin_dominex", "Twin Dominex", DomiNexRarity.Common, "Si au moins 2 doubles sont joues, +7 Mult.", Tags("double", "mult"), Scoring(DomiNexEffectType.AddMultIfDoubleCountAtLeast, 7, 2)),
            Define("niwt_dominex", "Niwt Dominex", DomiNexRarity.Common, "Si au moins 2 doubles sont joues, +50 Tile.", Tags("double", "count"), Scoring(DomiNexEffectType.AddCountIfDoubleCountAtLeast, 50, 2)),
            Define("heavyweight", "Heavyweight", DomiNexRarity.Common, "+2 Mult par double joue.", Tags("double", "mult"), Scoring(DomiNexEffectType.AddMultPerDouble, 2)),
            Define("jacko_7even_dominex", "Jack'o 7even Dominex", DomiNexRarity.Common, "Chaque domino joue dont la somme vaut 7 donne +20 Tile et +2 Mult.", Tags("seven", "jackpot", "count", "mult"), Scoring(DomiNexEffectType.AddCountAndMultPerDominoSumExactly, 20, 7, "2")),
            Define("straight_dominex", "Straight Dominex", DomiNexRarity.Common, "Si Small Tile Straight est joue, +45 Tile.", Tags("straight", "count"), Scoring(DomiNexEffectType.AddCountIfPattern, 45, 0, PatternNames.SmallTileStraightId)),
            Define("long_straight_dominex", "Long Straight Dominex", DomiNexRarity.Common, "Si Long Tile Straight est joue, +12 Mult.", Tags("straight", "mult"), Scoring(DomiNexEffectType.AddMultIfPatternId, 12, 0, PatternNames.LongTileStraightId)),
            Define("line_dominex", "Line Dominex", DomiNexRarity.Common, "Si Tile Line est joue, +40 Tile.", Tags("line", "design", "count"), Scoring(DomiNexEffectType.AddCountIfPattern, 40, 0, PatternNames.TileLineId)),
            Define("looper_dominex", "Looper Dominex", DomiNexRarity.Common, "Si Tile Loop est joue, +8 Mult.", Tags("loop", "design", "mult"), Scoring(DomiNexEffectType.AddMultIfPatternId, 8, 0, PatternNames.TileLoopId)),
            Define("looping_dominex", "Looping Dominex", DomiNexRarity.Common, "Si Tile Loop est joue, +60 Tile.", Tags("loop", "design", "count"), Scoring(DomiNexEffectType.AddCountIfPattern, 60, 0, PatternNames.TileLoopId)),
            Define("cross_dominex", "Cross Dominex", DomiNexRarity.Common, "Si Cross Tile est joue, +50 Tile, +8 Mult par extension de croix.", Tags("cross", "design", "count", "mult"), Scoring(DomiNexEffectType.AddCountIfPatternWithCrossExtensionMult, 50, 8, PatternNames.CrossTileId)),
            Define("designer", "Designer", DomiNexRarity.Common, "Si un Design Pattern est joue, +6 Mult.", Tags("design", "mult"), Scoring(DomiNexEffectType.AddMultIfAnyDesignPattern, 6)),
            Define("half_dominex", "Half Dominex", DomiNexRarity.Common, "Si 3 dominos ou moins sont places, +14 Mult.", Tags("placed", "mult"), Scoring(DomiNexEffectType.AddMultIfPlacedAtMost, 14, 3)),
            Define("opener", "Opener", DomiNexRarity.Common, "Le premier domino pose donne +4 Mult.", Tags("placed", "mult"), Scoring(DomiNexEffectType.AddMultToFirstDomino, 4)),
            Define("finisher", "Finisher", DomiNexRarity.Common, "Le dernier domino pose donne +18 Tile.", Tags("placed", "count"), Scoring(DomiNexEffectType.AddCountToLastDomino, 18)),
            Define("mystic", "Mystic", DomiNexRarity.Common, "Si tu valides avec 0 discard restant, +12 Mult.", Tags("discard", "mult"), Scoring(DomiNexEffectType.AddMultIfNoDiscardRemaining, 12)),
            Define("shallow", "Shallow", DomiNexRarity.Common, "+4 Mult par discard restant.", Tags("discard", "mult"), Scoring(DomiNexEffectType.AddMultPerDiscardRemaining, 4)),
            Define("closer", "Closer", DomiNexRarity.Common, "Si tu n'as utilise aucun discard, +10 Mult.", Tags("discard", "mult"), Scoring(DomiNexEffectType.AddMultIfNoDiscard, 10)),
            Define("even_dominex", "Even Dominex", DomiNexRarity.Common, "Chaque domino joue contenant uniquement des valeurs paires donne +3 Mult.", Tags("even", "mult"), Scoring(DomiNexEffectType.AddMultPerEvenDomino, 3)),
            Define("odd_dominex", "Odd Dominex", DomiNexRarity.Common, "Chaque domino joue contenant uniquement des valeurs impaires donne +20 Tile.", Tags("odd", "count"), Scoring(DomiNexEffectType.AddCountPerOddDomino, 20)),
            Define("scholar", "Scholar", DomiNexRarity.Common, "Chaque domino joue contenant un 0 donne +14 Tile et +3 Mult.", Tags("zero", "count", "mult"), Scoring(DomiNexEffectType.AddCountAndMultPerDominoContainingValue, 14, 0, "3")),
            Define("reroll_dominex", "Reroll", DomiNexRarity.Common, "Le premier reroll de chaque shop est gratuit.", Tags("shop", "reroll"), Passive(DomiNexEffectType.FreeFirstShopReroll)),
            Define("credit_dominex", "Credit Dominex", DomiNexRarity.Common, "Tu peux acheter meme si tu descends jusqu'a -20 credits.", Tags("economy", "debt"), Passive(DomiNexEffectType.SetMinimumCreditFloor, 20)),
            Define("gros_michel", "Gros Michel", DomiNexRarity.Common, "+15 Mult. Apres chaque main jouee, 1 chance sur 6 de detruire ce DomiNex.", Tags("mult", "chance", "destroy"), Scoring(DomiNexEffectType.AddMult, 15), PostScoring(DomiNexEffectType.ChanceDestroySelf, 1, 6)),
            Define("spark_dominex", "Spark Dominex", DomiNexRarity.Common, "+6 Jackpot Meter au debut de chaque niveau.", Tags("jackpot"), LevelStart(DomiNexEffectType.AddJackpotMeterFlat, 6)),

            // ====================================================================================
            // RARE — effets qui montent en puissance ou amorcent des synergies.
            // ====================================================================================
            Define("triplet_dominex", "Triplet Dominex", DomiNexRarity.Rare, "Si au moins 3 doubles sont joues, +10 Mult.", Tags("double", "mult"), Scoring(DomiNexEffectType.AddMultIfDoubleCountAtLeast, 10, 3)),
            Define("telprit_dominex", "Telprit Dominex", DomiNexRarity.Rare, "Si au moins 3 doubles sont joues, +70 Tile.", Tags("double", "count"), Scoring(DomiNexEffectType.AddCountIfDoubleCountAtLeast, 70, 3)),
            Define("fibonacci", "Fibonacci", DomiNexRarity.Rare, "Chaque domino joue contenant 1, 3 ou 5 donne +4 Mult.", Tags("value", "mult"), Scoring(DomiNexEffectType.AddMultPerDominoContainingAnyValue, 4, 0, "1,3,5")),
            Define("tycoon", "Tycoon", DomiNexRarity.Rare, "+1 Mult par tranche de 5 credits.", Tags("economy", "mult"), Scoring(DomiNexEffectType.AddMultPerCreditStep, 1, 5)),
            Define("boss_slayer", "Boss Slayer", DomiNexRarity.Rare, "Pendant un niveau de boss, +35 Tile et +4 Mult.", Tags("boss", "count", "mult"), Scoring(DomiNexEffectType.AddCountAndMultIfBossLevel, 35, 4)),
            Define("perfectionist", "Perfectionist", DomiNexRarity.Rare, "Si exactement 5 dominos sont places, +20 Mult.", Tags("placed", "mult"), Scoring(DomiNexEffectType.AddMultIfExactPlacedCount, 20, 5)),
            Define("space_dominex", "Space Dominex", DomiNexRarity.Rare, "1 chance sur 4 d'ameliorer le niveau du Value Pattern joue de +1.", Tags("pattern_level", "chance"), PostScoring(DomiNexEffectType.ChancePatternLevelUp, 1, 4)),
            Define("doublish", "Doublish", DomiNexRarity.Rare, "Score final x1 + 0.15 par double dans ton sac.", Tags("double", "bag", "multiplier"), Scoring(DomiNexEffectType.AddFinalMultiplierByBagDoubleCount, 15)),
            Define("empty_dominex", "Empty Dominex", DomiNexRarity.Rare, "Mult x nombre de slots DomiNex libres.", Tags("slots", "multiplier"), Scoring(DomiNexEffectType.MultiplyMultByFreeDomiNexSlots)),
            Define("twin_link", "Twin Link", DomiNexRarity.Rare, "+4 Mult par autre DomiNex 'double' actif.", Tags("double", "synergy", "mult"), Scoring(DomiNexEffectType.AddMultPerActiveDomiNexWithTag, 4, 0, "double")),
            Define("ascendant", "Ascendant", DomiNexRarity.Rare, "+5 Mult par niveau du Value Pattern joue au-dela de 1.", Tags("value", "pattern_level", "synergy", "mult"), Scoring(DomiNexEffectType.AddMultPerValuePatternLevel, 5)),
            Define("hot_hand", "Hot Hand", DomiNexRarity.Rare, "+3 Mult par point de Heat de la machine.", Tags("jackpot", "heat", "mult"), Scoring(DomiNexEffectType.AddMultPerHeat, 3)),
            Define("meter_reader", "Meter Reader", DomiNexRarity.Rare, "+2 Mult par tranche de 25 du Jackpot Meter.", Tags("jackpot", "mult"), Scoring(DomiNexEffectType.AddMultPerJackpotMeterStep, 2, 25)),
            Define("lucky_charm", "Lucky Charm", DomiNexRarity.Rare, "+1 Jackpot Luck au debut de chaque niveau.", Tags("jackpot"), LevelStart(DomiNexEffectType.AddJackpotLuck, 1)),

            // ====================================================================================
            // EPIC — mecaniques structurantes et synergies fortes.
            // ====================================================================================
            Define("synergist", "Synergist", DomiNexRarity.Epic, "+5 Mult par autre DomiNex actif.", Tags("synergy", "mult"), Scoring(DomiNexEffectType.AddMultPerActiveDomiNex, 5)),
            Define("architect", "Architect", DomiNexRarity.Epic, "+1 domino jouable. +35 Tile.", Tags("placed", "count"), LevelStart(DomiNexEffectType.AddMaxPlacedDominoes, 1), Scoring(DomiNexEffectType.AddCount, 35)),
            Define("prospector", "Prospector", DomiNexRarity.Epic, "+6 credits au debut de chaque niveau.", Tags("economy"), LevelStart(DomiNexEffectType.AddCredits, 6)),
            Define("tactician", "Tactician", DomiNexRarity.Epic, "+2 discards au debut de chaque niveau.", Tags("discard"), LevelStart(DomiNexEffectType.AddDiscardsThisLevel, 2)),
            Define("gambler", "Gambler", DomiNexRarity.Epic, "Si le Nox est plein, x1.8 score final, sinon -2 Mult.", Tags("placed", "multiplier", "risk"), Scoring(DomiNexEffectType.AddFinalMultiplierIfFullPlacedElseMultPenalty, 180, 2)),
            Define("double_dealer", "Double Dealer", DomiNexRarity.Epic, "Mult x nombre de doubles joues.", Tags("double", "multiplier"), Scoring(DomiNexEffectType.MultiplyMultByDoubleCount)),
            Define("high_roller", "High Roller", DomiNexRarity.Epic, "+5 Mult par Spin Ticket en reserve.", Tags("jackpot", "spin", "mult"), Scoring(DomiNexEffectType.AddMultPerSpinTicket, 5)),
            Define("jackpot_engine", "Jackpot Engine", DomiNexRarity.Epic, "Mult x nombre de Spin Tickets en reserve.", Tags("jackpot", "spin", "multiplier"), Scoring(DomiNexEffectType.MultiplyMultBySpinTickets)),
            Define("fortune_seeker", "Fortune Seeker", DomiNexRarity.Epic, "+1 Spin Ticket et +1 Jackpot Luck au debut de chaque niveau.", Tags("jackpot", "spin"), LevelStart(DomiNexEffectType.AddSpinTickets, 1), LevelStart(DomiNexEffectType.AddJackpotLuck, 1)),
            Define("overclock", "Overclock", DomiNexRarity.Epic, "+3 Heat au debut de niveau, et +6 Mult par point de Heat.", Tags("jackpot", "heat", "mult"), LevelStart(DomiNexEffectType.AddMachineHeat, 3), Scoring(DomiNexEffectType.AddMultPerHeat, 6)),
            Define("rigged_machine", "Rigged Machine", DomiNexRarity.Epic, "+2 Jackpot Luck par niveau (plus de Crown et Seven a la machine).", Tags("jackpot"), LevelStart(DomiNexEffectType.AddJackpotLuck, 2)),
            Define("design_weaver", "Design Weaver", DomiNexRarity.Epic, "+9 Mult par autre DomiNex 'design' actif.", Tags("design", "synergy", "mult"), Scoring(DomiNexEffectType.AddMultPerActiveDomiNexWithTag, 9, 0, "design")),
            Define("banker", "Banker", DomiNexRarity.Epic, "+15 Jackpot Meter a chaque niveau reussi.", Tags("jackpot", "economy"), LevelWon(DomiNexEffectType.AddJackpotMeterOnWin, 15)),

            // ====================================================================================
            // LEGENDARY — rares et tres puissants, souvent multiplicatifs.
            // ====================================================================================
            Define("overlord", "Overlord", DomiNexRarity.Legendary, "Mult x nombre de dominos places.", Tags("placed", "multiplier"), Scoring(DomiNexEffectType.MultiplyMultByPlacedDominoes)),
            Define("the_house", "The House", DomiNexRarity.Legendary, "+55 Tile et +14 Mult.", Tags("flat", "count", "mult"), Scoring(DomiNexEffectType.AddCount, 55), Scoring(DomiNexEffectType.AddMult, 14)),
            Define("midas", "Midas", DomiNexRarity.Legendary, "+6 credits a chaque niveau reussi.", Tags("economy"), LevelWon(DomiNexEffectType.AddCredits, 6)),
            Define("seventh_heaven", "Seventh Heaven", DomiNexRarity.Legendary, "Chaque domino joue dont la somme vaut 7 donne +50 Tile et +5 Mult.", Tags("seven", "jackpot", "count", "mult"), Scoring(DomiNexEffectType.AddCountAndMultPerDominoSumExactly, 50, 7, "5")),
            Define("double_down", "Double Down", DomiNexRarity.Legendary, "Si au moins 2 doubles sont joues, +200 Tile et +20 Mult.", Tags("double", "count", "mult"), Scoring(DomiNexEffectType.AddCountIfDoubleCountAtLeast, 200, 2), Scoring(DomiNexEffectType.AddMultIfDoubleCountAtLeast, 20, 2)),
            Define("royal_flush", "Royal Flush", DomiNexRarity.Legendary, "Si Long Tile Straight est joue, +30 Mult.", Tags("straight", "mult"), Scoring(DomiNexEffectType.AddMultIfPatternId, 30, 0, PatternNames.LongTileStraightId)),
            Define("jackpot_king", "Jackpot King", DomiNexRarity.Legendary, "Score final x1 + 0.5 par Spin Ticket en reserve.", Tags("jackpot", "spin", "multiplier"), Scoring(DomiNexEffectType.AddFinalMultiplierPerSpinTicket, 50)),
            Define("the_collective", "The Collective", DomiNexRarity.Legendary, "+8 Mult par autre DomiNex actif.", Tags("synergy", "mult"), Scoring(DomiNexEffectType.AddMultPerActiveDomiNex, 8)),
            Define("dynasty", "Dynasty", DomiNexRarity.Legendary, "Mult x nombre total de DomiNex actifs.", Tags("synergy", "multiplier"), Scoring(DomiNexEffectType.MultiplyMultByActiveDomiNex)),

            // ====================================================================================
            // UNLOCKABLE — verrouilles derriere des objectifs (voir ObjectiveRegistry).
            // ====================================================================================
            Define("le_schisme", "Le Schisme", DomiNexRarity.Legendary, "Tu peux poser une 2e region de dominos non connectee a la premiere. +4 dominos jouables.", Tags("unlock", "schism", "placed", "gamechanger"), LevelStart(DomiNexEffectType.AddMaxClusters, 1), LevelStart(DomiNexEffectType.AddMaxPlacedDominoes, 4)),
            Define("the_whale", "The Whale", DomiNexRarity.Legendary, "+2 Mult par tranche de 10 credits.", Tags("unlock", "economy", "mult"), Scoring(DomiNexEffectType.AddMultPerCreditStep, 2, 10)),
            Define("giant_slayer", "Giant Slayer", DomiNexRarity.Legendary, "Pendant un niveau de boss, +100 Tile et +12 Mult.", Tags("unlock", "boss", "count", "mult"), Scoring(DomiNexEffectType.AddCountAndMultIfBossLevel, 100, 12)),
            Define("hoarder", "Hoarder", DomiNexRarity.Epic, "+4 Mult par autre DomiNex actif.", Tags("unlock", "synergy", "mult"), Scoring(DomiNexEffectType.AddMultPerActiveDomiNex, 4)),
            Define("twin_souls", "Twin Souls", DomiNexRarity.Legendary, "Si un Design Pattern est joue, +30 Mult.", Tags("unlock", "design", "mult"), Scoring(DomiNexEffectType.AddMultIfAnyDesignPattern, 30)),

            // ====================================================================================
            // CURSED — gros risque / grosse recompense.
            // ====================================================================================
            Define("blood_pact", "Blood Pact", DomiNexRarity.Cursed, "-1 domino jouable, mais +40 Mult.", Tags("cursed", "placed", "mult"), LevelStart(DomiNexEffectType.AddMaxPlacedDominoes, -1), Scoring(DomiNexEffectType.AddMult, 40)),
            Define("tax_collector", "Tax Collector", DomiNexRarity.Cursed, "-3 credits au debut de niveau, mais +60 Tile et +8 Mult.", Tags("cursed", "economy", "count", "mult"), LevelStart(DomiNexEffectType.AddCredits, -3), Scoring(DomiNexEffectType.AddCount, 60), Scoring(DomiNexEffectType.AddMult, 8)),
            Define("glass_cannon", "Glass Cannon", DomiNexRarity.Cursed, "+25 Mult. Apres chaque main jouee, 1 chance sur 3 de detruire ce DomiNex.", Tags("cursed", "mult", "chance", "destroy"), Scoring(DomiNexEffectType.AddMult, 25), PostScoring(DomiNexEffectType.ChanceDestroySelf, 1, 3)),
            Define("all_in", "All In", DomiNexRarity.Cursed, "Si le Nox est plein, x3 score final, sinon -50 Mult.", Tags("cursed", "placed", "multiplier", "risk"), Scoring(DomiNexEffectType.AddFinalMultiplierIfFullPlacedElseMultPenalty, 300, 50))
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
                && definition.Effects.Any(effect => effect.Trigger != DomiNexTrigger.FutureHook);
        }

        private static DomiNexDefinition Define(string id, string name, DomiNexRarity rarity, string description, IReadOnlyList<string> tags, params DomiNexEffectDefinition[] effects)
        {
            return new DomiNexDefinition(id, name, rarity, description, tags, effects);
        }

        private static IReadOnlyList<string> Tags(params string[] tags) => tags;
        private static DomiNexEffectDefinition Scoring(DomiNexEffectType type, int value = 0, int threshold = 0, string note = "") => new DomiNexEffectDefinition(DomiNexTrigger.Scoring, type, value, threshold, note);
        private static DomiNexEffectDefinition PostScoring(DomiNexEffectType type, int value = 0, int threshold = 0) => new DomiNexEffectDefinition(DomiNexTrigger.PostScoring, type, value, threshold);
        private static DomiNexEffectDefinition Passive(DomiNexEffectType type, int value = 0) => new DomiNexEffectDefinition(DomiNexTrigger.Passive, type, value);
        private static DomiNexEffectDefinition LevelStart(DomiNexEffectType type, int value = 0, int threshold = 0) => new DomiNexEffectDefinition(DomiNexTrigger.LevelStart, type, value, threshold);
        private static DomiNexEffectDefinition LevelWon(DomiNexEffectType type, int value = 0, int threshold = 0) => new DomiNexEffectDefinition(DomiNexTrigger.LevelWon, type, value, threshold);
        private static DomiNexEffectDefinition RunStart(DomiNexEffectType type, int value = 0, int threshold = 0) => new DomiNexEffectDefinition(DomiNexTrigger.RunStart, type, value, threshold);
        private static DomiNexEffectDefinition Future(string note) => new DomiNexEffectDefinition(DomiNexTrigger.FutureHook, DomiNexEffectType.FutureHook, note: note);
    }
}
