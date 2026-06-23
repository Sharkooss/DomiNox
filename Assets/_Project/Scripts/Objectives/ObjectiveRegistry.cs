using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Objectives
{
    // Single source of truth for unlock objectives. Each objective points at the DomiNex it unlocks.
    public static class ObjectiveRegistry
    {
        public const string SchismDomiNexId = "le_schisme";

        public static IReadOnlyList<ObjectiveDefinition> All { get; } = new[]
        {
            new ObjectiveDefinition("complete_floor_3", "Briser le plafond", "Termine le 3e etage.", false, SchismDomiNexId, ObjectiveType.ReachFloor, 3),
            new ObjectiveDefinition("high_roller", "High Roller", "Marque 4000 points en une seule main.", false, "the_whale", ObjectiveType.ScoreInSingleHand, 4000),
            new ObjectiveDefinition("boss_no_discard", "Tueur de geants", "Bats un niveau de boss sans utiliser de discard.", false, "giant_slayer", ObjectiveType.WinBossWithoutDiscard, 0),
            new ObjectiveDefinition("collector", "Collectionneur", "Possede 5 DomiNex en meme temps.", false, "hoarder", ObjectiveType.OwnDomiNexCount, 5),
            new ObjectiveDefinition("twin_patterns", "Ame soeur", null, true, "twin_souls", ObjectiveType.PlayDesignPatternsInOneHand, 2)
        };

        public static ObjectiveDefinition GetById(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? null : All.FirstOrDefault(objective => objective.Id == id);
        }

        // A DomiNex is locked-by-default when an objective grants it.
        public static bool IsRewardLockedByDefault(string domiNexId)
        {
            return !string.IsNullOrWhiteSpace(domiNexId) && All.Any(objective => objective.RewardDomiNexId == domiNexId);
        }

        public static ObjectiveDefinition GetObjectiveForReward(string domiNexId)
        {
            return string.IsNullOrWhiteSpace(domiNexId) ? null : All.FirstOrDefault(objective => objective.RewardDomiNexId == domiNexId);
        }
    }
}
