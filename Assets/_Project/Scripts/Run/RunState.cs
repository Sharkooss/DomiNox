using DomiNox.Core;
using DomiNox.Dominoes;
using DomiNox.Dominex;

namespace DomiNox.Run
{
    public sealed class RunState
    {
        public int Credits { get; set; } = GameConstants.StartingCredits;
        public DominoBag Bag { get; } = new DominoBag();
        public DomiNexInventory DomiNexInventory { get; } = new DomiNexInventory();
        public LevelState CurrentLevel { get; set; }
    }
}
