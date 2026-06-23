using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Jackpot
{
    public static class JackpotRewardCatalog
    {
        public static IReadOnlyList<JackpotRewardDefinition> ConsolationRewards { get; } = new[]
        {
            Define("small_credits", "Small Credits", JackpotRewardType.SmallCredits, "+5 credits."),
            Define("meter_refund", "Meter Refund", JackpotRewardType.MeterRefund, "+25 Jackpot Meter."),
            Define("free_shop_reroll", "Free Shop Reroll", JackpotRewardType.FreeShopReroll, "Next shop reroll is free."),
            Define("extra_discard", "Extra Discard", JackpotRewardType.ExtraDiscard, "+1 discard next level.")
        };

        public static IReadOnlyList<JackpotRewardDefinition> MajorRewards { get; } = new[]
        {
            Define("royal_seat", "Royal Seat", JackpotRewardType.MajorRoyalSeat, "+1 DomiNex slot and +1 consumable slot."),
            Define("gem_flood", "Gem Flood", JackpotRewardType.MajorGemFlood, "Apply 3 Gem Tile upgrades from 7 choices."),
            Define("casino_credit", "Casino Credit", JackpotRewardType.MajorCasinoCredit, "+50 credits."),
            Define("pattern_ascension", "Pattern Ascension", JackpotRewardType.MajorPatternAscension, "+5 levels to a pattern. Auto-applies to most played pattern in this prototype."),
            Define("crown_dominex", "Crown DomiNex", JackpotRewardType.MajorCrownDomiNex, "Choose a Legendary DomiNex. Compensation if unavailable."),
            Define("boss_bribe", "Boss Bribe", JackpotRewardType.MajorBossBribe, "Next boss quota -90%."),
            Define("infinite_reroll", "Infinite Reroll", JackpotRewardType.MajorInfiniteReroll, "Next shop has free rerolls."),
            Define("double_prize", "Double Prize", JackpotRewardType.MajorDoublePrize, "Credits doubled until end of floor."),
            Define("jackpot_engine", "Jackpot Engine", JackpotRewardType.MajorJackpotEngine, "Jackpot Meter x3 for 3 levels.")
        };

        public static JackpotRewardDefinition GetPairReward(JackpotSymbol symbol)
        {
            return symbol switch
            {
                JackpotSymbol.Coin => Define("pair_coins", "Coin Pair", JackpotRewardType.Credits10, "+10 credits."),
                JackpotSymbol.Gem => Define("pair_gems", "Gem Pair", JackpotRewardType.JumboGemstonePack, "Open a free Jumbo Gemstone Pack."),
                JackpotSymbol.Domino => Define("pair_dominoes", "Domino Pair", JackpotRewardType.FutureImprovedDomino, "Add an improved domino to your bag."),
                JackpotSymbol.DomiNex => Define("pair_dominex", "DomiNex Pair", JackpotRewardType.NextShopFreeDomiNex, "Next shop has one free DomiNex."),
                JackpotSymbol.Crown => Define("pair_crowns", "Crown Pair", JackpotRewardType.NextShopDiscount, "Next shop first purchase -50%."),
                JackpotSymbol.Skull => Define("pair_skulls", "Skull Pair", JackpotRewardType.SkullCreditsQuota, "+20 credits, next quota +10%."),
                JackpotSymbol.Seven => Define("pair_sevens", "Seven Pair", JackpotRewardType.SpinAndCredits, "+1 Spin Ticket and +10 credits."),
                _ => ConsolationRewards[0]
            };
        }

        public static JackpotRewardDefinition GetTripleReward(JackpotSymbol symbol)
        {
            return symbol switch
            {
                JackpotSymbol.Coin => Define("triple_coins", "Triple Coins", JackpotRewardType.Credits30, "+30 credits."),
                JackpotSymbol.Gem => Define("triple_gems", "Triple Gems", JackpotRewardType.MegaGemstonePackAndPatternLevels, "Open a free Mega Pack and add +5 levels to most played patterns."),
                JackpotSymbol.Domino => Define("triple_dominoes", "Triple Dominoes", JackpotRewardType.FutureSpecialDominoChoice, "Add a special modified domino to your bag."),
                JackpotSymbol.DomiNex => Define("triple_dominex", "Triple DomiNex", JackpotRewardType.LegendaryDomiNexChoice, "Free Legendary DomiNex or +20 credits if unavailable/full."),
                JackpotSymbol.Crown => Define("triple_crowns", "Triple Crowns", JackpotRewardType.ExtraSlots, "+1 DomiNex slot and +1 consumable slot."),
                JackpotSymbol.Skull => Define("triple_skulls", "Triple Skulls", JackpotRewardType.CursedDomiNexPlaceholder, "Gain a Cursed DomiNex (or +30 credits if unavailable)."),
                JackpotSymbol.Seven => Define("major_777", "MAJOR JACKPOT", JackpotRewardType.MajorCasinoCredit, "Choose 2 major rewards."),
                _ => ConsolationRewards[0]
            };
        }

        public static List<JackpotRewardDefinition> PickMajorOptions(System.Random random, int count)
        {
            return MajorRewards.OrderBy(_ => random.Next()).Take(count).ToList();
        }

        private static JackpotRewardDefinition Define(string id, string name, JackpotRewardType type, string description, bool implemented = true)
        {
            return new JackpotRewardDefinition(id, name, type, description, implemented);
        }
    }
}
