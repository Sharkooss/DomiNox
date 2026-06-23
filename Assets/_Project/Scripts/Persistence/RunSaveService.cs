using System.IO;
using DomiNox.Run;
using UnityEngine;

namespace DomiNox.Persistence
{
    // Persists the in-progress run to disk as JSON so it can be resumed after quitting.
    public static class RunSaveService
    {
        private const string FileName = "run_save.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave()
        {
            return File.Exists(FilePath);
        }

        public static void Save(RunState run)
        {
            if (run == null)
            {
                return;
            }

            try
            {
                var data = RunSaveMapper.ToData(run);
                File.WriteAllText(FilePath, JsonUtility.ToJson(data));
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"RunSaveService: failed to save run ({exception.Message}).");
            }
        }

        public static bool TryLoad(out RunSaveData data)
        {
            data = null;
            if (!HasSave())
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(FilePath);
                var loaded = JsonUtility.FromJson<RunSaveData>(json);
                if (loaded == null || loaded.version != RunSaveData.CurrentVersion)
                {
                    return false;
                }

                data = loaded;
                return true;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"RunSaveService: failed to load run ({exception.Message}).");
                return false;
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"RunSaveService: failed to delete save ({exception.Message}).");
            }
        }
    }
}
