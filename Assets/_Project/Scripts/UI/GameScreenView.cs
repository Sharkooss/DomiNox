using DomiNox.Run;
using DomiNox.Scoring;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class GameScreenView : MonoBehaviour
    {
        private GameFlowController controller;
        private ScorePanelView scorePanel;
        private DomiNexBarView dominexBar;
        private GridView gridView;
        private HandView handView;
        private ActionButtonsView actionButtons;
        private ShopView shopView;
        private LayoutElement shopLayout;
        private Text feedback;
        private Vector2 lastPointerPosition;
        private bool hasPointerPreview;
        private DominoView activeDragView;

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

        private void Update()
        {
            if (controller.Run.Phase != RunPhase.PlayingLevel)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                RotateSelection(true);
            }

            if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
            {
                RotateSelection(false);
            }

            if (hasPointerPreview && activeDragView != null && Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                DropDraggedDomino(Mouse.current.position.ReadValue());
            }
        }

        private void BuildLayout()
        {
            EnsureEventSystem();
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            UiFactory.ConfigureCanvasScaler(canvas.GetComponent<CanvasScaler>());

            var root = new GameObject("GameRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            root.transform.SetParent(canvas.transform, false);
            Stretch(root.GetComponent<RectTransform>());
            var rootLayout = root.GetComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(12, 12, 12, 12);
            rootLayout.spacing = 8f;

            dominexBar = new GameObject("DomiNexBar", typeof(RectTransform), typeof(LayoutElement)).AddComponent<DomiNexBarView>();
            dominexBar.transform.SetParent(root.transform, false);
            dominexBar.GetComponent<LayoutElement>().preferredHeight = 76f;
            dominexBar.Initialize();

            var top = new GameObject("Top", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            top.transform.SetParent(root.transform, false);
            top.GetComponent<LayoutElement>().flexibleHeight = 1f;
            var topLayout = top.GetComponent<HorizontalLayoutGroup>();
            topLayout.spacing = 10f;
            topLayout.childAlignment = TextAnchor.MiddleCenter;

            scorePanel = new GameObject("ScorePanel", typeof(RectTransform), typeof(LayoutElement)).AddComponent<ScorePanelView>();
            scorePanel.transform.SetParent(top.transform, false);
            scorePanel.GetComponent<LayoutElement>().preferredWidth = 230f;
            scorePanel.Initialize();

            gridView = new GameObject("Grid", typeof(RectTransform), typeof(LayoutElement)).AddComponent<GridView>();
            gridView.transform.SetParent(top.transform, false);
            gridView.GetComponent<LayoutElement>().preferredWidth = 372f;
            gridView.Initialize(controller);

            actionButtons = new GameObject("Actions", typeof(RectTransform), typeof(LayoutElement)).AddComponent<ActionButtonsView>();
            actionButtons.transform.SetParent(top.transform, false);
            actionButtons.GetComponent<LayoutElement>().preferredWidth = 180f;
            actionButtons.Initialize(controller);

            shopView = new GameObject("Shop", typeof(RectTransform), typeof(LayoutElement)).AddComponent<ShopView>();
            shopView.transform.SetParent(top.transform, false);
            shopLayout = shopView.GetComponent<LayoutElement>();
            shopLayout.preferredWidth = 1040f;
            shopLayout.flexibleWidth = 1f;
            shopView.Initialize(controller);

            feedback = UiFactory.CreateText(root.transform, "Feedback", string.Empty, 18, TextAnchor.MiddleCenter);
            feedback.color = new Color(0.95f, 0.85f, 0.42f);

            handView = new GameObject("Hand", typeof(RectTransform), typeof(LayoutElement)).AddComponent<HandView>();
            handView.transform.SetParent(root.transform, false);
            handView.GetComponent<LayoutElement>().preferredHeight = 62f;
            handView.Initialize(controller, SetActiveDragView, UpdateDragPreview, DropDraggedDomino);
        }

        private void Render(RunState run, ScoreResult score, string message)
        {
            var isShop = run.Phase == RunPhase.Shop;
            gridView.gameObject.SetActive(!isShop);
            actionButtons.gameObject.SetActive(!isShop);
            handView.gameObject.SetActive(!isShop);
            shopView.gameObject.SetActive(isShop);

            scorePanel.Render(run, score);
            if (!isShop)
            {
                gridView.Render(run.CurrentLevel.Grid);
                handView.Render(run.CurrentLevel.Hand, controller.SelectedDomino, controller.CurrentOrientation, controller.SelectedForDiscard);
                actionButtons.Render(controller);
            }

            shopView.Render(run);
            dominexBar.Render(run);
            feedback.text = message;
            if (!isShop && hasPointerPreview)
            {
                RefreshPointerPreview();
            }
        }

        private void UpdateDragPreview(Vector2 screenPosition)
        {
            lastPointerPosition = screenPosition;
            hasPointerPreview = true;
            RefreshPointerPreview();
        }

        private void DropDraggedDomino(Vector2 screenPosition)
        {
            if (!hasPointerPreview && activeDragView == null)
            {
                return;
            }

            var draggedView = activeDragView;
            hasPointerPreview = false;
            activeDragView = null;
            draggedView?.ForceClearDragGhost();
            if (gridView.TryGetCellAtScreenPosition(screenPosition, out var x, out var y))
            {
                controller.TryPlaceSelected(x, y);
                return;
            }

            gridView.Render(controller.Run.CurrentLevel.Grid);
        }

        private void SetActiveDragView(DominoView dominoView)
        {
            activeDragView = dominoView;
            activeDragView.SetOrientation(controller.CurrentOrientation);
        }

        private void RefreshPointerPreview()
        {
            if (controller.SelectedDomino == null || !gridView.TryGetCellAtScreenPosition(lastPointerPosition, out var x, out var y))
            {
                activeDragView?.SetDragGhostOverGrid(false);
                gridView.Render(controller.Run.CurrentLevel.Grid);
                return;
            }

            activeDragView?.SetDragGhostOverGrid(true);
            var valid = controller.CanPlaceSelected(x, y);
            gridView.RenderPreview(
                controller.Run.CurrentLevel.Grid,
                controller.SelectedDomino,
                new DomiNox.Grid.GridPosition(x, y),
                controller.CurrentOrientation,
                valid);
        }

        private void RotateSelection(bool clockwise)
        {
            if (clockwise)
            {
                controller.RotateRightWithoutNotify();
            }
            else
            {
                controller.RotateLeftWithoutNotify();
            }

            actionButtons.Render(controller);
            if (hasPointerPreview && activeDragView != null)
            {
                activeDragView.SetOrientation(controller.CurrentOrientation);
            }
            else
            {
                handView.Render(controller.Run.CurrentLevel.Hand, controller.SelectedDomino, controller.CurrentOrientation, controller.SelectedForDiscard);
            }
            RefreshPointerPreview();
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
