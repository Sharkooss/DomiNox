using DomiNox.Core;
using DomiNox.Dominoes;
using DomiNox.Dominex;
using DomiNox.Shop;
using DomiNox.Bosses;

namespace DomiNox.Run
{
    public sealed class RunState
    {
        public int Credits { get; set; } = GameConstants.StartingCredits;
        public int MaxDomiNexSlots { get; set; } = GameConstants.StartingDomiNexSlots;
        public DominoBag Bag { get; } = new DominoBag();
        public DomiNexInventory DomiNexInventory { get; } = new DomiNexInventory();
        public LevelState CurrentLevel { get; set; }
        public ShopState CurrentShop { get; set; }
        public LevelRewardState CurrentReward { get; set; }
        public RunPhase Phase { get; set; } = RunPhase.FloorProgress;
        public BossDefinition CurrentFloorBoss { get; set; }
        public string PreviousBossId { get; set; }

        public int ActiveDomiNexCount => DomiNexInventory.Count;
        public bool HasFreeDomiNexSlot() => ActiveDomiNexCount < MaxDomiNexSlots;
        public bool CanAddDomiNex(DomiNexDefinition definition) => DomiNexInventory.CanAdd(definition, MaxDomiNexSlots);
    }
}
