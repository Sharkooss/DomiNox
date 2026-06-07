using DomiNox.Core;
using DomiNox.Dominoes;
using DomiNox.Bosses;
using DomiNox.Grid;

namespace DomiNox.Run
{
    public sealed class LevelState
    {
        public int FloorIndex { get; set; } = 1;
        public int LevelIndex { get; set; } = 1;
        public int Quota { get; set; } = LevelQuotaService.GetQuota(1, 1);
        public int CurrentScore { get; set; }
        public int DiscardsRemaining { get; set; } = GameConstants.PhaseOneDiscards;
        public int DiscardsUsed { get; set; }
        public int MaxPlacedDominoes { get; set; } = GameConstants.PhaseOneMaxPlacedDominoes;
        public HandState Hand { get; }
        public GridState Grid { get; }
        public BossLevelState Boss { get; set; }
        public bool IsWon { get; set; }
        public bool IsLost { get; set; }

        public LevelState()
        {
            Hand = new HandState(GameConstants.StartingHandSize);
            Grid = new GridState();
        }
    }
}
