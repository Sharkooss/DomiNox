using System;
using System.Collections.Generic;

namespace DomiNox.Persistence
{
    // Cross-run profile: tracks what the player has discovered and their best progress.
    [Serializable]
    public sealed class MetaProfile
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int bestFloorReached;
        public int bestLevelReached;
        public bool endlessUnlocked;
        public List<string> discoveredDomiNexIds = new List<string>();
        public List<string> discoveredSecretPatternIds = new List<string>();
        public List<string> encounteredBossIds = new List<string>();
        public List<string> unlockedDomiNexIds = new List<string>();
        public List<string> completedObjectiveIds = new List<string>();
    }
}
