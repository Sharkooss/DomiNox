using DomiNox.Core;
using DomiNox.Dominoes;

namespace DomiNox.Run
{
    public sealed class RunState
    {
        public int Credits { get; set; } = GameConstants.StartingCredits;
        public DominoBag Bag { get; } = new DominoBag();
        public LevelState CurrentLevel { get; set; }
    }
}
