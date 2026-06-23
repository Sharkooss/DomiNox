using System;
using System.Collections.Generic;

namespace DomiNox.Persistence
{
    // Serializable snapshot of a run at a stable checkpoint (FloorProgress phase).
    // Uses public fields + Lists so Unity's JsonUtility can serialize it (it does not support Dictionary).
    [Serializable]
    public sealed class RunSaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        public int credits;
        public int maxDomiNexSlots;
        public int maxConsumableSlots;
        public int phase;
        public int floorIndex;
        public int levelIndex;
        public string currentFloorBossId;
        public string previousBossId;

        public int jackpotMeter;
        public int jackpotMaxMeter;
        public int jackpotSpinTickets;
        public int jackpotLuck;
        public int jackpotMachineHeat;
        public int jackpotMultiplierLevelsRemaining;
        public float jackpotMeterMultiplier;

        public bool nextShopHasFreeDomiNex;
        public int nextShopDiscountPercent;
        public bool nextShopInfiniteFreeRerolls;
        public bool nextShopFreeReroll;
        public int nextLevelExtraDiscards;
        public float nextLevelQuotaMultiplier;
        public float nextBossQuotaMultiplierOverride;
        public bool doubleCreditsUntilEndOfFloor;

        public List<DomiNexSlotData> domiNexSlots = new List<DomiNexSlotData>();
        public List<ConsumableData> consumables = new List<ConsumableData>();
        public List<PatternLevelData> patternLevels = new List<PatternLevelData>();
        public List<PatternUsageData> patternUsage = new List<PatternUsageData>();
        public List<string> revealedSecretPatterns = new List<string>();
        public List<PurchasedDominoData> purchasedDominoes = new List<PurchasedDominoData>();
        public List<string> destroyedDominoInstanceIds = new List<string>();
    }

    // An empty id marks an empty DomiNex slot (preserves the slot layout).
    [Serializable]
    public sealed class DomiNexSlotData
    {
        public string id;
        public int purchasePrice;
    }

    [Serializable]
    public sealed class ConsumableData
    {
        public string id;
        public int sellValue;
    }

    [Serializable]
    public sealed class PatternLevelData
    {
        public string patternId;
        public int level;
    }

    [Serializable]
    public sealed class PatternUsageData
    {
        public string patternName;
        public int count;
    }

    [Serializable]
    public sealed class PurchasedDominoData
    {
        public string instanceId;
        public int left;
        public int right;
        public string modifierId;
    }
}
