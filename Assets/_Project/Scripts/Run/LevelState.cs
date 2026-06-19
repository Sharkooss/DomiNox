using DomiNox.Core;
using DomiNox.Dominoes;
using DomiNox.Bosses;
using DomiNox.Grid;
using System.Collections.Generic;

namespace DomiNox.Run
{
    public sealed class LevelState
    {
        public int FloorIndex { get; set; } = 1;
        public int LevelIndex { get; set; } = 1;
        public int Quota { get; set; } = LevelQuotaService.GetQuota(1, 1);
        public int CurrentScore { get; set; }
        public int MaxHands { get; set; } = 3;
        public int HandsRemaining { get; set; } = 3;
        public int MaxDiscards { get; set; } = GameConstants.PhaseOneDiscards;
        public int DiscardsRemaining { get; set; } = GameConstants.PhaseOneDiscards;
        public int DiscardsUsed { get; set; }
        public int MaxPlacedDominoes { get; set; } = GameConstants.PhaseOneMaxPlacedDominoes;
        public HandState Hand { get; }
        public GridState Grid { get; }
        public List<DominoInstance> PlayedThisLevel { get; } = new List<DominoInstance>();
        public List<DominoInstance> DiscardedThisLevel { get; } = new List<DominoInstance>();
        public BossLevelState Boss { get; set; }
        public bool IsWon { get; set; }
        public bool IsLost { get; set; }
        public bool AwardedQuotaOver50JackpotGain { get; set; }
        public bool AwardedQuotaOver100JackpotGain { get; set; }

        public LevelState()
        {
            Hand = new HandState(GameConstants.StartingHandSize);
            Grid = new GridState();
        }
    }
}
