using DomiNox.Core;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class MainMenuView : MonoBehaviour
    {
        private void Start()
        {
            EnsureEventSystem();
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasComponent = canvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            UiFactory.ConfigureCanvasScaler(canvas.GetComponent<CanvasScaler>());

            var root = new GameObject("MainMenuRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            root.transform.SetParent(canvas.transform, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = root.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 24f;

            var title = UiFactory.CreateText(root.transform, "Title", "DomiNox", 56, TextAnchor.MiddleCenter);
            title.color = new Color(0.95f, 0.85f, 0.42f);

            var subtitle = UiFactory.CreateText(root.transform, "Subtitle", "Phase 1 Core Prototype", 24, TextAnchor.MiddleCenter);
            subtitle.color = new Color(0.82f, 0.86f, 0.92f);

            var playButton = UiFactory.CreateButton(root.transform, "PlayButton", "Jouer");
            playButton.GetComponent<LayoutElement>().preferredWidth = 220f;
            playButton.GetComponent<LayoutElement>().preferredHeight = 64f;
            playButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Game));
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }
    }
}
