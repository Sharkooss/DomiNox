#if UNITY_EDITOR
using System;
using System.IO;
using DomiNox.Run;
using DomiNox.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace DomiNox.Utilities.Editor
{
    public static class DomiNoxSceneBuilder
    {
        private const string MainMenuPath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string GamePath = "Assets/_Project/Scenes/Game.unity";

        [MenuItem("DomiNox/Build Phase 1 Scenes")]
        public static void BuildPhaseOneScenes()
        {
            BuildMainMenu();
            BuildGame();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(GamePath, true)
            };
            AssetDatabase.SaveAssets();
        }

        private static void BuildMainMenu()
        {
            var scene = OpenOrCreateScene(MainMenuPath);
            var changed = false;
            changed |= EnsureRootObject<Camera>("Main Camera");
            changed |= EnsureRootObject<EventSystem>("EventSystem", typeof(InputSystemUIInputModule));
            changed |= EnsureRootObject<MainMenuView>("MainMenuView");
            if (changed)
            {
                EditorSceneManager.SaveScene(scene, MainMenuPath);
            }
        }

        private static void BuildGame()
        {
            var scene = OpenOrCreateScene(GamePath);
            var changed = false;
            changed |= EnsureRootObject<Camera>("Main Camera");
            changed |= EnsureRootObject<EventSystem>("EventSystem", typeof(InputSystemUIInputModule));
            changed |= EnsureRootObject<GameFlowController>("GameFlowController");
            changed |= EnsureRootObject<GameScreenView>("GameScreenView");
            if (changed)
            {
                EditorSceneManager.SaveScene(scene, GamePath);
            }
        }

        private static Scene OpenOrCreateScene(string path)
        {
            return File.Exists(path)
                ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static bool EnsureRootObject<T>(string name, params Type[] extraComponentTypes) where T : Component
        {
            var root = GameObject.Find(name);
            if (root == null)
            {
                var componentTypes = new Type[extraComponentTypes.Length + 1];
                componentTypes[0] = typeof(T);
                Array.Copy(extraComponentTypes, 0, componentTypes, 1, extraComponentTypes.Length);
                new GameObject(name, componentTypes);
                return true;
            }

            var changed = false;
            if (root.GetComponent<T>() == null)
            {
                root.AddComponent<T>();
                changed = true;
            }

            foreach (var componentType in extraComponentTypes)
            {
                if (root.GetComponent(componentType) != null)
                {
                    continue;
                }

                root.AddComponent(componentType);
                changed = true;
            }

            return changed;
        }
    }
}
#endif
