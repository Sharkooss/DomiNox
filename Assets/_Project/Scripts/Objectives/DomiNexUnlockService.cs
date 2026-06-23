using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Objectives
{
    // Decides whether a DomiNex may appear in the shop/packs given what the player has unlocked.
    public static class DomiNexUnlockService
    {
        public static bool IsLockedByDefault(string domiNexId)
        {
            return ObjectiveRegistry.IsRewardLockedByDefault(domiNexId);
        }

        public static bool IsAvailable(string domiNexId, IReadOnlyCollection<string> unlockedIds)
        {
            return !IsLockedByDefault(domiNexId) || (unlockedIds != null && unlockedIds.Contains(domiNexId));
        }
    }
}
