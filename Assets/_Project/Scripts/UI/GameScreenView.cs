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
        private BagPanelView bagPanel;
        private LevelRewardView levelRewardView;
        private RunLostView runLostView;
        private BossIntroView bossIntroView;
        private ShopView shopView;
        private LayoutElement shopLayout;
        private Text feedback;
        private Text utilityInfo;
        private GameObject centerGameplayPanel;
        private GameObject gridPanel;
        private GameObject handPanel;
        private GameObject rightUtilityPanel;
        private GameObject modalOverlayRoot;
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
            if (controller.Run.Phase != RunPhase.PlayingLevel || controller.BossIntroActive)
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

            var root = new GameObject("RootHUD", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            root.transform.SetParent(canvas.transform, false);
            Stretch(root.GetComponent<RectTransform>());
            var rootLayout = root.GetComponent<HorizontalLayoutGroup>();
            rootLayout.padding = new RectOffset(14, 14, 14, 14);
            rootLayout.spacing = 14f;
            rootLayout.childAlignment = TextAnchor.MiddleCenter;
            rootLayout.childForceExpandWidth = false;
            rootLayout.childForceExpandHeight = true;

            scorePanel = new GameObject("ScorePanel", typeof(RectTransform), typeof(LayoutElement)).AddComponent<ScorePanelView>();
            scorePanel.transform.SetParent(root.transform, false);
            var scoreLayout = scorePanel.GetComponent<LayoutElement>();
            scoreLayout.preferredWidth = 240f;
            scoreLayout.flexibleHeight = 1f;
            scorePanel.Initialize();

            centerGameplayPanel = new GameObject("CenterGameplayPanel", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            centerGameplayPanel.transform.SetParent(root.transform, false);
            var centerLayoutElement = centerGameplayPanel.GetComponent<LayoutElement>();
            centerLayoutElement.preferredWidth = 900f;
            centerLayoutElement.flexibleWidth = 1f;
            centerLayoutElement.flexibleHeight = 1f;
            var centerLayout = centerGameplayPanel.GetComponent<VerticalLayoutGroup>();
            centerLayout.spacing = 10f;
            centerLayout.childAlignment = TextAnchor.UpperCenter;
            centerLayout.childForceExpandWidth = true;
            centerLayout.childForceExpandHeight = false;

            dominexBar = new GameObject("TopDominexPanel", typeof(RectTransform), typeof(LayoutElement)).AddComponent<DomiNexBarView>();
            dominexBar.transform.SetParent(centerGameplayPanel.transform, false);
            dominexBar.GetComponent<LayoutElement>().preferredHeight = 66f;
            dominexBar.Initialize();

            gridPanel = CreatePanel("GridPanel", centerGameplayPanel.transform, new Color(0.06f, 0.08f, 0.11f, 0.72f));
            var gridPanelLayout = gridPanel.GetComponent<LayoutElement>();
            gridPanelLayout.preferredHeight = 466f;
            var gridPanelGroup = gridPanel.AddComponent<HorizontalLayoutGroup>();
            gridPanelGroup.padding = new RectOffset(12, 12, 12, 12);
            gridPanelGroup.childAlignment = TextAnchor.MiddleCenter;
            gridPanelGroup.childForceExpandWidth = false;
            gridPanelGroup.childForceExpandHeight = false;

            gridView = new GameObject("Grid", typeof(RectTransform), typeof(LayoutElement)).AddComponent<GridView>();
            gridView.transform.SetParent(gridPanel.transform, false);
            var gridLayout = gridView.GetComponent<LayoutElement>();
            gridLayout.preferredWidth = 428f;
            gridLayout.preferredHeight = 428f;
            gridView.Initialize(controller);

            handPanel = CreatePanel("PlayerHandPanel", centerGameplayPanel.transform, new Color(0.07f, 0.09f, 0.12f, 0.92f));
            handPanel.GetComponent<LayoutElement>().preferredHeight = 112f;
            var handPanelLayout = handPanel.AddComponent<VerticalLayoutGroup>();
            handPanelLayout.padding = new RectOffset(10, 10, 8, 10);
            handPanelLayout.spacing = 5f;
            handPanelLayout.childAlignment = TextAnchor.UpperCenter;
            var handTitle = UiFactory.CreateText(handPanel.transform, "Title", "Main du joueur", 14, TextAnchor.MiddleCenter);
            handTitle.color = new Color(0.72f, 0.78f, 0.86f);

            handView = new GameObject("Hand", typeof(RectTransform), typeof(LayoutElement)).AddComponent<HandView>();
            handView.transform.SetParent(handPanel.transform, false);
            handView.GetComponent<LayoutElement>().preferredHeight = 68f;
            handView.Initialize(controller, SetActiveDragView, UpdateDragPreview, DropDraggedDomino);

            actionButtons = new GameObject("Actions", typeof(RectTransform), typeof(LayoutElement)).AddComponent<ActionButtonsView>();
            actionButtons.transform.SetParent(centerGameplayPanel.transform, false);
            actionButtons.GetComponent<LayoutElement>().preferredHeight = 52f;
            actionButtons.Initialize(controller);

            shopView = new GameObject("Shop", typeof(RectTransform), typeof(LayoutElement)).AddComponent<ShopView>();
            shopView.transform.SetParent(centerGameplayPanel.transform, false);
            shopLayout = shopView.GetComponent<LayoutElement>();
            shopLayout.preferredHeight = 620f;
            shopLayout.flexibleWidth = 1f;
            shopLayout.flexibleHeight = 1f;
            shopView.Initialize(controller);

            modalOverlayRoot = new GameObject("ModalOverlayRoot", typeof(RectTransform), typeof(Image));
            modalOverlayRoot.transform.SetParent(canvas.transform, false);
            Stretch(modalOverlayRoot.GetComponent<RectTransform>());
            modalOverlayRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.28f);

            bossIntroView = new GameObject("BossIntroView", typeof(RectTransform)).AddComponent<BossIntroView>();
            bossIntroView.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)bossIntroView.transform);
            bossIntroView.Initialize(controller);

            levelRewardView = new GameObject("LevelRewardView", typeof(RectTransform), typeof(LayoutElement)).AddComponent<LevelRewardView>();
            levelRewardView.transform.SetParent(modalOverlayRoot.transform, false);
            var rewardRect = (RectTransform)levelRewardView.transform;
            rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
            rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
            rewardRect.pivot = new Vector2(0.5f, 0.5f);
            rewardRect.sizeDelta = new Vector2(560f, 470f);
            levelRewardView.Initialize(controller);

            runLostView = new GameObject("RunLostView", typeof(RectTransform), typeof(LayoutElement)).AddComponent<RunLostView>();
            runLostView.transform.SetParent(modalOverlayRoot.transform, false);
            var lostRect = (RectTransform)runLostView.transform;
            lostRect.anchorMin = new Vector2(0.5f, 0.5f);
            lostRect.anchorMax = new Vector2(0.5f, 0.5f);
            lostRect.pivot = new Vector2(0.5f, 0.5f);
            lostRect.sizeDelta = new Vector2(520f, 360f);
            runLostView.Initialize();

            feedback = UiFactory.CreateText(centerGameplayPanel.transform, "Feedback", string.Empty, 18, TextAnchor.MiddleCenter);
            feedback.color = new Color(0.95f, 0.85f, 0.42f);

            rightUtilityPanel = CreatePanel("RightUtilityPanel", root.transform, new Color(0.06f, 0.08f, 0.11f, 0.88f));
            var rightLayoutElement = rightUtilityPanel.GetComponent<LayoutElement>();
            rightLayoutElement.preferredWidth = 220f;
            rightLayoutElement.flexibleHeight = 1f;
            var rightLayout = rightUtilityPanel.AddComponent<VerticalLayoutGroup>();
            rightLayout.padding = new RectOffset(10, 10, 10, 10);
            rightLayout.spacing = 10f;
            rightLayout.childAlignment = TextAnchor.UpperCenter;

            utilityInfo = UiFactory.CreateText(rightUtilityPanel.transform, "UtilityInfo", string.Empty, 14, TextAnchor.UpperLeft);
            utilityInfo.GetComponent<LayoutElement>().preferredHeight = 130f;
            bagPanel = new GameObject("BagPanel", typeof(RectTransform), typeof(LayoutElement)).AddComponent<BagPanelView>();
            bagPanel.transform.SetParent(rightUtilityPanel.transform, false);
            var bagLayout = bagPanel.GetComponent<LayoutElement>();
            bagLayout.preferredWidth = 150f;
            bagLayout.preferredHeight = 118f;
            bagPanel.Initialize(controller);
        }

        private void Render(RunState run, ScoreResult score, string message)
        {
            var isShop = run.Phase == RunPhase.Shop;
            var isPlaying = run.Phase == RunPhase.PlayingLevel;
            var isReward = run.Phase == RunPhase.LevelReward;
            var isLost = run.Phase == RunPhase.RunLost;
            gridPanel.SetActive(isPlaying);
            actionButtons.gameObject.SetActive(isPlaying);
            handPanel.SetActive(isPlaying);
            rightUtilityPanel.SetActive(true);
            shopView.gameObject.SetActive(isShop);
            modalOverlayRoot.SetActive(isReward || isLost);
            if (isReward || isLost)
            {
                modalOverlayRoot.transform.SetAsLastSibling();
            }
            levelRewardView.gameObject.SetActive(isReward);
            runLostView.gameObject.SetActive(isLost);

            scorePanel.Render(run, score);
            bagPanel.Render(run);
            bossIntroView.Render(run);
            if (isPlaying)
            {
                gridView.Render(run.CurrentLevel.Grid);
                handView.Render(run.CurrentLevel.Hand, controller.SelectedDomino, controller.CurrentOrientation, controller.SelectedForDiscard);
                actionButtons.Render(controller);
                utilityInfo.text = $"Rotation\nA/E : {controller.CurrentOrientation}\n\nControle\nDrag un domino vers la grille.\nClique un domino pour le marquer en defausse.";
            }

            if (isShop)
            {
                utilityInfo.text = "Shop\nAchete un DomiNex ou passe au niveau suivant.\n\nLa reserve reste disponible pour verifier le sac.";
            }

            if (isReward)
            {
                levelRewardView.Render(run);
                utilityInfo.text = "Cash Out\nRecupere tes credits avant d'ouvrir le shop.\n\nLes bonus viennent des discards restants et des interets.";
            }

            if (isLost)
            {
                runLostView.Render(run);
                utilityInfo.text = "Run perdue\nRetourne au menu principal pour relancer une partie.";
            }

            shopView.Render(run);
            dominexBar.Render(run);
            feedback.gameObject.SetActive(isPlaying || isShop);
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

        private bool SetActiveDragView(DominoView dominoView)
        {
            if (controller.SelectedDomino == null || controller.BossIntroActive)
            {
                return false;
            }

            activeDragView = dominoView;
            activeDragView.SetOrientation(controller.CurrentOrientation);
            return true;
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

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            var outline = panel.GetComponent<Outline>();
            outline.effectColor = new Color(0.16f, 0.24f, 0.34f);
            outline.effectDistance = new Vector2(2f, -2f);
            return panel;
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
