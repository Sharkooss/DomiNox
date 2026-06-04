#if UNITY_EDITOR
using DomiNox.Run;
using DomiNox.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

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
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Main Camera", typeof(Camera));
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            new GameObject("MainMenuView", typeof(MainMenuView));
            EditorSceneManager.SaveScene(scene, MainMenuPath);
        }

        private static void BuildGame()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Main Camera", typeof(Camera));
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            new GameObject("GameFlowController", typeof(GameFlowController));
            new GameObject("GameScreenView", typeof(GameScreenView));
            EditorSceneManager.SaveScene(scene, GamePath);
        }
    }
}
#endif
