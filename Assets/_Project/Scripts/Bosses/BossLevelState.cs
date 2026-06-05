using System.Collections.Generic;

namespace DomiNox.Bosses
{
    public sealed class BossLevelState
    {
        public BossDefinition Definition { get; }
        public int? BannedValue { get; set; }
        public HashSet<string> LockedDominoIds { get; } = new HashSet<string>();

        public BossLevelState(BossDefinition definition)
        {
            Definition = definition;
        }
    }
}
