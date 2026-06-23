using System.Collections.Generic;
using System.Linq;
using DomiNox.Dominex;
using DomiNox.Persistence;

namespace DomiNox.Objectives
{
    public enum CollectionStatus
    {
        Owned,       // currently in the active run's inventory
        Discovered,  // owned at some point in a previous run
        Available,   // unlocked / not locked, but never owned
        Locked       // gated behind an objective not yet completed
    }

    public sealed class CollectionEntry
    {
        public string DomiNexId { get; }
        public string Name { get; }
        public DomiNexRarity Rarity { get; }
        public CollectionStatus Status { get; }
        public string Hint { get; }

        public CollectionEntry(string domiNexId, string name, DomiNexRarity rarity, CollectionStatus status, string hint)
        {
            DomiNexId = domiNexId;
            Name = name;
            Rarity = rarity;
            Status = status;
            Hint = hint;
        }
    }

    // Builds the data model for the collection codex (what is owned / discovered / available / locked).
    public static class CollectionService
    {
        public static List<CollectionEntry> BuildEntries(MetaProfile profile, DomiNexInventory currentInventory = null)
        {
            profile ??= new MetaProfile();
            var entries = new List<CollectionEntry>();

            foreach (var definition in DomiNexRegistry.All)
            {
                var locked = DomiNexUnlockService.IsLockedByDefault(definition.Id) && !profile.unlockedDomiNexIds.Contains(definition.Id);
                CollectionStatus status;
                string hint = null;

                if (currentInventory != null && currentInventory.Contains(definition.Id))
                {
                    status = CollectionStatus.Owned;
                }
                else if (locked)
                {
                    status = CollectionStatus.Locked;
                    hint = ObjectiveRegistry.GetObjectiveForReward(definition.Id)?.DisplayHint;
                }
                else if (profile.discoveredDomiNexIds.Contains(definition.Id))
                {
                    status = CollectionStatus.Discovered;
                }
                else
                {
                    status = CollectionStatus.Available;
                }

                entries.Add(new CollectionEntry(definition.Id, definition.Name, definition.Rarity, status, hint));
            }

            return entries;
        }

        public static int CountUnlocked(MetaProfile profile)
        {
            profile ??= new MetaProfile();
            return DomiNexRegistry.All.Count(definition =>
                !DomiNexUnlockService.IsLockedByDefault(definition.Id) || profile.unlockedDomiNexIds.Contains(definition.Id));
        }
    }
}
