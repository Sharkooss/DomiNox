using System.Collections.Generic;
using System.IO;
using System.Linq;
using DomiNox.Run;
using UnityEngine;

namespace DomiNox.Persistence
{
    // Loads, updates and persists the cross-run MetaProfile (discovery + best progress).
    public static class MetaProfileService
    {
        private const string FileName = "meta_profile.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static MetaProfile Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var loaded = JsonUtility.FromJson<MetaProfile>(File.ReadAllText(FilePath));
                    if (loaded != null && loaded.version == MetaProfile.CurrentVersion)
                    {
                        return loaded;
                    }
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"MetaProfileService: failed to load profile ({exception.Message}).");
            }

            return new MetaProfile();
        }

        public static void Save(MetaProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(profile));
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"MetaProfileService: failed to save profile ({exception.Message}).");
            }
        }

        // Merges the current run's discoveries and progress into the profile, returning the updated profile.
        public static MetaProfile RecordFromRun(RunState run, MetaProfile profile = null)
        {
            profile ??= Load();
            if (run == null)
            {
                return profile;
            }

            var level = run.CurrentLevel;
            if (level != null)
            {
                profile.bestFloorReached = System.Math.Max(profile.bestFloorReached, level.FloorIndex);
                profile.bestLevelReached = System.Math.Max(profile.bestLevelReached, level.LevelIndex);
            }

            Merge(profile.discoveredDomiNexIds, run.DomiNexInventory.Active.Select(definition => definition.Id));
            Merge(profile.discoveredSecretPatternIds, run.RevealedSecretPatterns);
            if (run.CurrentFloorBoss != null)
            {
                Merge(profile.encounteredBossIds, new[] { run.CurrentFloorBoss.Id });
            }

            return profile;
        }

        public static void RecordAndSave(RunState run)
        {
            Save(RecordFromRun(run));
        }

        private static void Merge(List<string> target, IEnumerable<string> additions)
        {
            foreach (var addition in additions)
            {
                if (!string.IsNullOrWhiteSpace(addition) && !target.Contains(addition))
                {
                    target.Add(addition);
                }
            }
        }
    }
}
