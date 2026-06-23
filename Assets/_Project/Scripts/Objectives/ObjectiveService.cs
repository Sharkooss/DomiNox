using System.Collections.Generic;
using DomiNox.Persistence;

namespace DomiNox.Objectives
{
    // Evaluates objectives against a progress context, mutating the MetaProfile to record completions
    // and unlock rewards. Returns the objectives newly completed by this evaluation.
    public static class ObjectiveService
    {
        public static List<ObjectiveDefinition> Evaluate(MetaProfile profile, ObjectiveProgressContext context)
        {
            var newlyCompleted = new List<ObjectiveDefinition>();
            if (profile == null || context == null)
            {
                return newlyCompleted;
            }

            foreach (var objective in ObjectiveRegistry.All)
            {
                if (profile.completedObjectiveIds.Contains(objective.Id) || !IsSatisfied(objective, context))
                {
                    continue;
                }

                profile.completedObjectiveIds.Add(objective.Id);
                if (!string.IsNullOrWhiteSpace(objective.RewardDomiNexId) && !profile.unlockedDomiNexIds.Contains(objective.RewardDomiNexId))
                {
                    profile.unlockedDomiNexIds.Add(objective.RewardDomiNexId);
                }

                if (objective.RewardDomiNexId == ObjectiveRegistry.SchismDomiNexId)
                {
                    profile.endlessUnlocked = true;
                }

                newlyCompleted.Add(objective);
            }

            return newlyCompleted;
        }

        private static bool IsSatisfied(ObjectiveDefinition objective, ObjectiveProgressContext context)
        {
            switch (objective.Type)
            {
                case ObjectiveType.ReachFloor:
                    return context.FloorReached >= objective.Threshold;
                case ObjectiveType.ScoreInSingleHand:
                    return context.HandScore >= objective.Threshold;
                case ObjectiveType.WinBossWithoutDiscard:
                    return context.WonBossWithoutDiscard;
                case ObjectiveType.PlayDesignPatternsInOneHand:
                    return context.DesignPatternsThisHand >= objective.Threshold;
                case ObjectiveType.OwnDomiNexCount:
                    return context.OwnedDomiNexCount >= objective.Threshold;
                default:
                    return false;
            }
        }
    }
}
