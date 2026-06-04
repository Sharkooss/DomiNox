using DomiNox.Run;
using DomiNox.Scoring;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class GameScreenView : MonoBehaviour
    {
        private GameFlowController controller;
        private ScorePanelView scorePanel;
        private GridView gridView;
        private HandView handView;
        private ActionButtonsView actionButtons;
        private Text feedback;

        private void Start()
        {
            controller = FindObjectOfType<GameFlowController>();
            controller.StateChanged += Render;
            BuildLayout();
            Render(controller.Run, new ScoreResult(0, 1, new System.Collections.Generic.List<string>(), new System.Collections.Generic.List<string>()), "Pret.");
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.StateChanged -= Render;
            }
        }

        private void BuildLayout()
        {
            EnsureEventSystem();
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var root = new GameObject("GameRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            root.transform.SetParent(canvas.transform, false);
            Stretch(root.GetComponent<RectTransform>());
            var rootLayout = root.GetComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(20, 20, 20, 20);
            rootLayout.spacing = 12f;

            var top = new GameObject("Top", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            top.transform.SetParent(root.transform, false);
            top.GetComponent<LayoutElement>().flexibleHeight = 1f;
            var topLayout = top.GetComponent<HorizontalLayoutGroup>();
            topLayout.spacing = 16f;

            scorePanel = new GameObject("ScorePanel", typeof(RectTransform), typeof(LayoutElement)).AddComponent<ScorePanelView>();
            scorePanel.transform.SetParent(top.transform, false);
            scorePanel.GetComponent<LayoutElement>().preferredWidth = 280f;
            scorePanel.Initialize();

            gridView = new GameObject("Grid", typeof(RectTransform), typeof(LayoutElement)).AddComponent<GridView>();
            gridView.transform.SetParent(top.transform, false);
            gridView.GetComponent<LayoutElement>().preferredWidth = 420f;
            gridView.Initialize(controller);

            actionButtons = new GameObject("Actions", typeof(RectTransform), typeof(LayoutElement)).AddComponent<ActionButtonsView>();
            actionButtons.transform.SetParent(top.transform, false);
            actionButtons.GetComponent<LayoutElement>().preferredWidth = 240f;
            actionButtons.Initialize(controller);

            feedback = UiFactory.CreateText(root.transform, "Feedback", string.Empty, 22, TextAnchor.MiddleCenter);
            feedback.color = new Color(0.95f, 0.85f, 0.42f);

            handView = new GameObject("Hand", typeof(RectTransform), typeof(LayoutElement)).AddComponent<HandView>();
            handView.transform.SetParent(root.transform, false);
            handView.GetComponent<LayoutElement>().preferredHeight = 76f;
            handView.Initialize(controller);
        }

        private void Render(RunState run, ScoreResult score, string message)
        {
            scorePanel.Render(run, score);
            gridView.Render(run.CurrentLevel.Grid);
            handView.Render(run.CurrentLevel.Hand, controller.SelectedDomino);
            actionButtons.Render(controller);
            feedback.text = message;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
