using DomiNox.Core;
using DomiNox.Consumables;
using DomiNox.Dominoes;
using DomiNox.Dominex;
using DomiNox.Shop;
using DomiNox.Bosses;
using System.Collections.Generic;

namespace DomiNox.Run
{
    public sealed class RunState
    {
        public int Credits { get; set; } = GameConstants.StartingCredits;
        public int MaxDomiNexSlots { get; set; } = GameConstants.StartingDomiNexSlots;
        public DominoBag Bag { get; } = new DominoBag();
        public DomiNexInventory DomiNexInventory { get; } = new DomiNexInventory();
        public ConsumableInventory Consumables { get; } = new ConsumableInventory();
        public PatternUsageState PatternUsage { get; } = new PatternUsageState();
        public PatternLevelState PatternLevels { get; } = new PatternLevelState();
        public HashSet<string> RevealedSecretPatterns { get; } = new HashSet<string>();
        public LevelState CurrentLevel { get; set; }
        public ShopState CurrentShop { get; set; }
        public LevelRewardState CurrentReward { get; set; }
        public RunPhase Phase { get; set; } = RunPhase.FloorProgress;
        public BossDefinition CurrentFloorBoss { get; set; }
        public string PreviousBossId { get; set; }

        public int ActiveDomiNexCount => DomiNexInventory.Count;
        public int ActiveConsumableCount => Consumables.Count;
        public bool HasFreeDomiNexSlot() => ActiveDomiNexCount < MaxDomiNexSlots;
        public bool CanAddDomiNex(DomiNexDefinition definition) => DomiNexInventory.CanAdd(definition, MaxDomiNexSlots);
        public bool HasFreeConsumableSlot() => Consumables.CanAdd(GameConstants.MaxConsumableSlots);
    }
}
