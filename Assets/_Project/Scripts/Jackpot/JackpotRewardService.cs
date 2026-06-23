using System;
using System.Linq;
using DomiNox.Core;
using DomiNox.Dominex;
using DomiNox.Dominoes;
using DomiNox.Patterns;
using DomiNox.Run;
using DomiNox.Shop;

namespace DomiNox.Jackpot
{
    public static class JackpotRewardService
    {
        public static string ApplyReward(RunState run, JackpotRewardDefinition reward, Random random, Action<BoosterPackDefinition> openFreePack)
        {
            if (run == null || reward == null)
            {
                return "No reward.";
            }

            switch (reward.Type)
            {
                case JackpotRewardType.SmallCredits:
                    return Credits(run, 5, reward.Name);
                case JackpotRewardType.MeterRefund:
                    JackpotMeterService.AddJackpotMeter(run, 25, JackpotGainSource.MeterRefund);
                    return "Meter Refund: +25 Jackpot Meter.";
                case JackpotRewardType.FreeShopReroll:
                    run.NextShopFreeReroll = true;
                    return "Next shop reroll is free.";
                case JackpotRewardType.ExtraDiscard:
                    run.NextLevelExtraDiscards++;
                    return "+1 discard next level.";
                case JackpotRewardType.Credits10:
                    return Credits(run, 10, reward.Name);
                case JackpotRewardType.JumboGemstonePack:
                    openFreePack?.Invoke(BoosterPackRegistry.GetById("gemstone_pack_jumbo"));
                    return "Free Jumbo Gemstone Pack opened.";
                case JackpotRewardType.FutureImprovedDomino:
                    return AddModifiedDomino(run, random, new[] { DominoModifierRegistry.BlueId, DominoModifierRegistry.RedId, DominoModifierRegistry.GoldId, DominoModifierRegistry.JackpotId }, "Domino Pair");
                case JackpotRewardType.NextShopFreeDomiNex:
                    run.NextShopHasFreeDomiNex = true;
                    return "Next shop has one free DomiNex.";
                case JackpotRewardType.NextShopDiscount:
                    run.NextShopDiscountPercent = 50;
                    return "Next shop first purchase -50%.";
                case JackpotRewardType.SkullCreditsQuota:
                    run.NextLevelQuotaMultiplier += 0.1f;
                    return Credits(run, 20, "Skull Pair") + " Next quota +10%.";
                case JackpotRewardType.SpinAndCredits:
                    run.Jackpot.SpinTickets++;
                    return Credits(run, 10, "Seven Pair") + " +1 Spin Ticket.";
                case JackpotRewardType.Credits30:
                    return Credits(run, 30, reward.Name);
                case JackpotRewardType.MegaGemstonePackAndPatternLevels:
                    ApplyMostPlayedPatternLevels(run, 5);
                    openFreePack?.Invoke(BoosterPackRegistry.GetById("gemstone_pack_mega"));
                    return "Free Mega Pack opened. Most played patterns gained +5 total levels.";
                case JackpotRewardType.FutureSpecialDominoChoice:
                    return AddModifiedDomino(run, random, new[] { DominoModifierRegistry.LuckyId, DominoModifierRegistry.GlassId, DominoModifierRegistry.RedId }, "Triple Dominoes");
                case JackpotRewardType.LegendaryDomiNexChoice:
                case JackpotRewardType.MajorCrownDomiNex:
                    return AddFreeLegendaryOrCredits(run, reward.Type == JackpotRewardType.MajorCrownDomiNex ? 25 : 20);
                case JackpotRewardType.ExtraSlots:
                case JackpotRewardType.MajorRoyalSeat:
                    run.MaxDomiNexSlots++;
                    run.MaxConsumableSlots++;
                    return "+1 DomiNex slot and +1 consumable slot.";
                case JackpotRewardType.CursedDomiNexPlaceholder:
                    return AddCursedDomiNexOrCredits(run, random, 30);
                case JackpotRewardType.MajorGemFlood:
                    ApplyVisibleGemUpgrades(run, 3);
                    return "Gem Flood: applied 3 visible Gem Tile upgrades.";
                case JackpotRewardType.MajorCasinoCredit:
                    return Credits(run, 50, reward.Name);
                case JackpotRewardType.MajorPatternAscension:
                    ApplyMostPlayedPatternLevels(run, 5);
                    return "Pattern Ascension: +5 levels to most played patterns.";
                case JackpotRewardType.MajorBossBribe:
                    run.NextBossQuotaMultiplierOverride = 0.1f;
                    return "Next boss quota -90%.";
                case JackpotRewardType.MajorInfiniteReroll:
                    run.NextShopInfiniteFreeRerolls = true;
                    return "Next shop rerolls are free.";
                case JackpotRewardType.MajorDoublePrize:
                    run.DoubleCreditsUntilEndOfFloor = true;
                    return "Credits doubled until end of floor.";
                case JackpotRewardType.MajorJackpotEngine:
                    run.Jackpot.JackpotMeterMultiplier = 3f;
                    run.Jackpot.JackpotMeterMultiplierLevelsRemaining = 3;
                    return "Jackpot Meter x3 for 3 levels.";
                default:
                    return "Reward not implemented.";
            }
        }

        private static string Credits(RunState run, int amount, string label)
        {
            var gained = RunEconomyService.AddCredits(run, amount, CreditSource.JackpotReward);
            return $"{label}: +{gained} credits.";
        }

        private static string AddFreeLegendaryOrCredits(RunState run, int compensation)
        {
            var candidates = DomiNexRegistry.All.Where(definition => definition.Rarity == DomiNexRarity.Legendary && !run.DomiNexInventory.Contains(definition.Id)).ToList();
            if (!run.HasFreeDomiNexSlot() || candidates.Count == 0)
            {
                return Credits(run, compensation, "Legendary DomiNex compensation");
            }

            run.DomiNexInventory.Add(candidates[0], 0);
            return $"Free Legendary DomiNex: {candidates[0].Name}.";
        }

        private static string AddCursedDomiNexOrCredits(RunState run, Random random, int compensation)
        {
            var candidates = DomiNexRegistry.All.Where(definition => definition.Rarity == DomiNexRarity.Cursed && !run.DomiNexInventory.Contains(definition.Id)).ToList();
            if (!run.HasFreeDomiNexSlot() || candidates.Count == 0)
            {
                return Credits(run, compensation, "Cursed DomiNex compensation");
            }

            var pick = candidates[(random ?? new Random()).Next(candidates.Count)];
            run.DomiNexInventory.Add(pick, 0);
            return $"Cursed DomiNex: {pick.Name}.";
        }

        private static string AddModifiedDomino(RunState run, Random random, string[] modifierPool, string label)
        {
            random ??= new Random();
            var definition = DominoShopOfferService.GenerateRandomShopDomino(random);
            var modifierId = modifierPool[random.Next(modifierPool.Length)];
            var modifier = DominoModifierRegistry.GetById(modifierId);
            run.PurchasedDominoes.Add(new DominoInstance($"jackpot_domino_{run.PurchasedDominoes.Count}", definition, modifierId));
            return $"{label}: {modifier?.Name ?? "domino"} ajoute a ton sac.";
        }

        private static void ApplyMostPlayedPatternLevels(RunState run, int totalLevels)
        {
            var targets = run.PatternUsage.Counts
                .Where(pair => pair.Value > 0)
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key)
                .Select(pair => pair.Key)
                .ToList();
            if (targets.Count == 0)
            {
                targets.Add(PatternNames.TileHighId);
            }

            var index = 0;
            for (var i = 0; i < totalLevels; i++)
            {
                var applied = false;
                for (var attempts = 0; attempts < targets.Count; attempts++)
                {
                    var target = targets[index % targets.Count];
                    index++;
                    if (!run.PatternLevels.IsMaxLevel(target))
                    {
                        run.PatternLevels.Increase(target);
                        applied = true;
                        break;
                    }
                }

                if (!applied)
                {
                    return;
                }
            }
        }

        private static void ApplyVisibleGemUpgrades(RunState run, int count)
        {
            foreach (var gem in DomiNox.Consumables.GemTileRegistry.GetAvailableGemTiles(run).Take(count))
            {
                run.PatternLevels.Increase(gem.TargetPatternId);
            }
        }
    }
}
