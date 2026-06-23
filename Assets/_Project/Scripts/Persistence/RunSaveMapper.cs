using System.Linq;
using DomiNox.Bosses;
using DomiNox.Dominex;
using DomiNox.Dominoes;
using DomiNox.Run;

namespace DomiNox.Persistence
{
    // Converts a RunState to/from its serializable snapshot.
    // Runtime objects are rebuilt from their ids through the registries: the save only stores
    // mutable state + ids, never the content definitions themselves.
    public static class RunSaveMapper
    {
        public static RunSaveData ToData(RunState run)
        {
            var data = new RunSaveData();
            if (run == null)
            {
                return data;
            }

            data.credits = run.Credits;
            data.maxDomiNexSlots = run.MaxDomiNexSlots;
            data.maxConsumableSlots = run.MaxConsumableSlots;
            data.phase = (int)run.Phase;
            data.floorIndex = run.CurrentLevel?.FloorIndex ?? 1;
            data.levelIndex = run.CurrentLevel?.LevelIndex ?? 1;
            data.currentFloorBossId = run.CurrentFloorBoss?.Id;
            data.previousBossId = run.PreviousBossId;

            var jackpot = run.Jackpot;
            data.jackpotMeter = jackpot.Meter;
            data.jackpotMaxMeter = jackpot.MaxMeter;
            data.jackpotSpinTickets = jackpot.SpinTickets;
            data.jackpotLuck = jackpot.JackpotLuck;
            data.jackpotMachineHeat = jackpot.MachineHeat;
            data.jackpotMultiplierLevelsRemaining = jackpot.JackpotMeterMultiplierLevelsRemaining;
            data.jackpotMeterMultiplier = jackpot.JackpotMeterMultiplier;

            data.nextShopHasFreeDomiNex = run.NextShopHasFreeDomiNex;
            data.nextShopDiscountPercent = run.NextShopDiscountPercent;
            data.nextShopInfiniteFreeRerolls = run.NextShopInfiniteFreeRerolls;
            data.nextShopFreeReroll = run.NextShopFreeReroll;
            data.nextLevelExtraDiscards = run.NextLevelExtraDiscards;
            data.nextLevelQuotaMultiplier = run.NextLevelQuotaMultiplier;
            data.nextBossQuotaMultiplierOverride = run.NextBossQuotaMultiplierOverride;
            data.doubleCreditsUntilEndOfFloor = run.DoubleCreditsUntilEndOfFloor;

            foreach (var slot in run.DomiNexInventory.SlotInstances)
            {
                data.domiNexSlots.Add(new DomiNexSlotData
                {
                    id = slot?.Definition.Id ?? string.Empty,
                    purchasePrice = slot?.PurchasePrice ?? 0
                });
            }

            foreach (var consumable in run.Consumables.ActiveInstances)
            {
                data.consumables.Add(new ConsumableData { id = consumable.DefinitionId, sellValue = consumable.SellValue });
            }

            foreach (var pair in run.PatternLevels.Levels)
            {
                data.patternLevels.Add(new PatternLevelData { patternId = pair.Key, level = pair.Value });
            }

            foreach (var pair in run.PatternUsage.Counts)
            {
                data.patternUsage.Add(new PatternUsageData { patternName = pair.Key, count = pair.Value });
            }

            data.revealedSecretPatterns.AddRange(run.RevealedSecretPatterns);
            data.destroyedDominoInstanceIds.AddRange(run.DestroyedDominoInstanceIds);

            foreach (var domino in run.PurchasedDominoes)
            {
                data.purchasedDominoes.Add(new PurchasedDominoData
                {
                    instanceId = domino.InstanceId,
                    left = domino.Definition.Left,
                    right = domino.Definition.Right,
                    modifierId = domino.ModifierId
                });
            }

            return data;
        }

        // Applies a snapshot onto a freshly created RunState (mutates run in place).
        // Does NOT rebuild CurrentLevel/Grid/Hand — those are reconstructed by the caller at FloorProgress.
        public static void Apply(RunSaveData data, RunState run)
        {
            if (data == null || run == null)
            {
                return;
            }

            run.Credits = data.credits;
            run.MaxDomiNexSlots = data.maxDomiNexSlots;
            run.MaxConsumableSlots = data.maxConsumableSlots;
            run.Phase = (RunPhase)data.phase;
            run.CurrentFloorBoss = BossRegistry.GetById(data.currentFloorBossId);
            run.PreviousBossId = data.previousBossId;

            run.Jackpot.Meter = data.jackpotMeter;
            run.Jackpot.MaxMeter = data.jackpotMaxMeter;
            run.Jackpot.SpinTickets = data.jackpotSpinTickets;
            run.Jackpot.JackpotLuck = data.jackpotLuck;
            run.Jackpot.MachineHeat = data.jackpotMachineHeat;
            run.Jackpot.JackpotMeterMultiplierLevelsRemaining = data.jackpotMultiplierLevelsRemaining;
            run.Jackpot.JackpotMeterMultiplier = data.jackpotMeterMultiplier;

            run.NextShopHasFreeDomiNex = data.nextShopHasFreeDomiNex;
            run.NextShopDiscountPercent = data.nextShopDiscountPercent;
            run.NextShopInfiniteFreeRerolls = data.nextShopInfiniteFreeRerolls;
            run.NextShopFreeReroll = data.nextShopFreeReroll;
            run.NextLevelExtraDiscards = data.nextLevelExtraDiscards;
            run.NextLevelQuotaMultiplier = data.nextLevelQuotaMultiplier;
            run.NextBossQuotaMultiplierOverride = data.nextBossQuotaMultiplierOverride;
            run.DoubleCreditsUntilEndOfFloor = data.doubleCreditsUntilEndOfFloor;

            var slots = data.domiNexSlots.Select(slot =>
            {
                if (string.IsNullOrEmpty(slot.id))
                {
                    return null;
                }

                var definition = DomiNexRegistry.GetById(slot.id);
                return definition == null ? null : new ActiveDomiNexInstance(definition, slot.purchasePrice);
            }).ToList();
            run.DomiNexInventory.RestoreSlots(slots);

            foreach (var consumable in data.consumables)
            {
                run.Consumables.Add(consumable.id, run.MaxConsumableSlots, consumable.sellValue);
            }

            foreach (var level in data.patternLevels)
            {
                // PatternLevelState starts every pattern at level 1; raise it to the saved level.
                run.PatternLevels.Increase(level.patternId, level.level - 1);
            }

            foreach (var usage in data.patternUsage)
            {
                run.PatternUsage.Restore(usage.patternName, usage.count);
            }

            run.RevealedSecretPatterns.Clear();
            run.RevealedSecretPatterns.UnionWith(data.revealedSecretPatterns);

            run.DestroyedDominoInstanceIds.Clear();
            run.DestroyedDominoInstanceIds.UnionWith(data.destroyedDominoInstanceIds);

            run.PurchasedDominoes.Clear();
            foreach (var domino in data.purchasedDominoes)
            {
                var definition = new DominoDefinition($"shop_domino_{domino.left}_{domino.right}", domino.left, domino.right);
                run.PurchasedDominoes.Add(new DominoInstance(domino.instanceId, definition, domino.modifierId));
            }
        }
    }
}
