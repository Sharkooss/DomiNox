using DomiNox.Core;
using DomiNox.Dominoes;
using DomiNox.Grid;

namespace DomiNox.Run
{
    public sealed class LevelState
    {
        public int FloorIndex { get; set; } = 1;
        public int LevelIndex { get; set; } = 1;
        public int Quota { get; set; } = GameConstants.PhaseOneQuota;
        public int CurrentScore { get; set; }
        public int ActionPoints { get; set; } = GameConstants.PhaseOneActionPoints;
        public int DiscardsRemaining { get; set; } = GameConstants.PhaseOneDiscards;
        public int MaxPlacedDominoes { get; set; } = GameConstants.PhaseOneMaxPlacedDominoes;
        public HandState Hand { get; }
        public GridState Grid { get; }
        public bool IsWon { get; set; }
        public bool IsLost { get; set; }

        public LevelState()
        {
            Hand = new HandState(GameConstants.StartingHandSize);
            Grid = new GridState();
        }
    }
}
