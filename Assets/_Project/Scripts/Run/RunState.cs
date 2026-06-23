using DomiNox.Core;
using DomiNox.Consumables;
using DomiNox.Dominoes;
using DomiNox.Dominex;
using DomiNox.Shop;
using DomiNox.Bosses;
using DomiNox.Jackpot;
using System.Collections.Generic;

namespace DomiNox.Run
{
    public sealed class RunState
    {
        public int Credits { get; set; } = GameConstants.StartingCredits;
        public int MaxDomiNexSlots { get; set; } = GameConstants.StartingDomiNexSlots;
        public int MaxConsumableSlots { get; set; } = GameConstants.MaxConsumableSlots;
        public JackpotState Jackpot { get; } = new JackpotState();
        public DominoBag Bag { get; } = new DominoBag();
        public List<DominoInstance> PurchasedDominoes { get; } = new List<DominoInstance>();
        public HashSet<string> DestroyedDominoInstanceIds { get; } = new HashSet<string>();
        public DomiNexInventory DomiNexInventory { get; } = new DomiNexInventory();
        public ConsumableInventory Consumables { get; } = new ConsumableInventory();
        public PatternUsageState PatternUsage { get; } = new PatternUsageState();
        public PatternLevelState PatternLevels { get; } = new PatternLevelState();
        public HashSet<string> RevealedSecretPatterns { get; } = new HashSet<string>();
        public HashSet<string> UnlockedDomiNexIds { get; } = new HashSet<string>();
        public bool EndlessUnlocked { get; set; }
        public LevelState CurrentLevel { get; set; }
        public ShopState CurrentShop { get; set; }
        public LevelRewardState CurrentReward { get; set; }
        public RunPhase Phase { get; set; } = RunPhase.FloorProgress;
        public BossDefinition CurrentFloorBoss { get; set; }
        public string PreviousBossId { get; set; }
        public bool NextShopHasFreeDomiNex { get; set; }
        public int NextShopDiscountPercent { get; set; }
        public bool NextShopInfiniteFreeRerolls { get; set; }
        public bool NextShopFreeReroll { get; set; }
        public int NextLevelExtraDiscards { get; set; }
        public float NextLevelQuotaMultiplier { get; set; } = 1f;
        public float NextBossQuotaMultiplierOverride { get; set; } = 1f;
        public bool DoubleCreditsUntilEndOfFloor { get; set; }

        public int ActiveDomiNexCount => DomiNexInventory.Count;
        public int ActiveConsumableCount => Consumables.Count;
        public int TotalDominoCount => 28 + PurchasedDominoes.Count;
        public bool HasFreeDomiNexSlot() => ActiveDomiNexCount < MaxDomiNexSlots;
        public bool CanAddDomiNex(DomiNexDefinition definition) => DomiNexInventory.CanAdd(definition, MaxDomiNexSlots);
        public bool HasFreeConsumableSlot() => Consumables.CanAdd(MaxConsumableSlots);
    }
}
