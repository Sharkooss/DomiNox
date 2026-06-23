using System.Collections;
using DomiNox.Run;
using DomiNox.Scoring;
using DomiNox.Patterns;
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
        private FloorProgressView floorProgressView;
        private HandView handView;
        private ActionButtonsView actionButtons;
        private BagPanelView bagPanel;
        private JackpotMeterView jackpotMeterView;
        private LevelRewardView levelRewardView;
        private RunLostView runLostView;
        private BossIntroView bossIntroView;
        private ShopView shopView;
        private LayoutElement shopLayout;
        private Text feedback;
        private FloatingTextService floatingText;
        private FeedbackService feedbackService;
        private GameObject centerGameplayPanel;
        private GameObject gridPanel;
        private GameObject handPanel;
        private GameObject rightUtilityPanel;
        private GameObject modalOverlayRoot;
        private Vector2 lastPointerPosition;
        private bool hasPointerPreview;
        private bool scoringAnimationRunning;
        private DominoView activeDragView;
        private string lastFeedbackMessage;

        private void Start()
        {
            controller = FindAnyObjectByType<GameFlowController>();
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
            if (controller.Run.Phase != RunPhase.PlayingLevel || controller.BossIntroActive || controller.IsScoring)
            {
                return;
            }

            if (WasKeyPressed(Key.E))
            {
                RotateSelection(true);
            }

            if (WasKeyPressed(Key.A) || WasKeyPressed(Key.Q))
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

            floatingText = new GameObject("FloatingTextService").AddComponent<FloatingTextService>();
            floatingText.transform.SetParent(canvas.transform, false);
            floatingText.Initialize(canvas.GetComponent<Canvas>());
            feedbackService = new GameObject("FeedbackService").AddComponent<FeedbackService>();
            feedbackService.transform.SetParent(canvas.transform, false);
            feedbackService.Initialize(canvas.GetComponent<Canvas>(), floatingText);

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
            dominexBar.GetComponent<LayoutElement>().preferredHeight = 88f;
            dominexBar.Initialize(controller);

            floorProgressView = new GameObject("FloorProgressView", typeof(RectTransform), typeof(LayoutElement)).AddComponent<FloorProgressView>();
            floorProgressView.transform.SetParent(centerGameplayPanel.transform, false);
            var floorProgressLayout = floorProgressView.GetComponent<LayoutElement>();
            floorProgressLayout.preferredHeight = 540f;
            floorProgressLayout.flexibleWidth = 1f;
            floorProgressView.Initialize(controller);

            gridPanel = CreatePanel("GridPanel", centerGameplayPanel.transform, DomiNoxTheme.BgPanel);
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

            handPanel = CreatePanel("PlayerHandPanel", centerGameplayPanel.transform, DomiNoxTheme.BgPanel);
            handPanel.GetComponent<LayoutElement>().preferredHeight = 112f;
            var handPanelLayout = handPanel.AddComponent<VerticalLayoutGroup>();
            handPanelLayout.padding = new RectOffset(10, 10, 8, 10);
            handPanelLayout.spacing = 5f;
            handPanelLayout.childAlignment = TextAnchor.UpperCenter;
            var handTitle = UiFactory.CreateText(handPanel.transform, "Title", "PLAYER HAND", DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            handTitle.color = DomiNoxTheme.TextMuted;
            handTitle.fontStyle = FontStyle.Bold;

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
            modalOverlayRoot.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(Color.black, 0.55f);

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

            feedback = UiFactory.CreateText(centerGameplayPanel.transform, "Feedback", string.Empty, DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            feedback.color = DomiNoxTheme.Gold;

            rightUtilityPanel = CreatePanel("RightUtilityPanel", root.transform, DomiNoxTheme.BgPanel);
            var rightLayoutElement = rightUtilityPanel.GetComponent<LayoutElement>();
            rightLayoutElement.preferredWidth = 220f;
            rightLayoutElement.flexibleHeight = 1f;
            var rightLayout = rightUtilityPanel.AddComponent<VerticalLayoutGroup>();
            rightLayout.padding = new RectOffset(10, 10, 10, 10);
            rightLayout.spacing = 10f;
            rightLayout.childAlignment = TextAnchor.UpperCenter;
            rightLayout.childControlWidth = true;
            rightLayout.childControlHeight = true;
            rightLayout.childForceExpandWidth = true;
            rightLayout.childForceExpandHeight = false;

            // Jackpot at the top, enlarged for emphasis.
            jackpotMeterView = new GameObject("JackpotMeter", typeof(RectTransform), typeof(LayoutElement)).AddComponent<JackpotMeterView>();
            jackpotMeterView.transform.SetParent(rightUtilityPanel.transform, false);
            var jackpotLayout = jackpotMeterView.GetComponent<LayoutElement>();
            jackpotLayout.preferredHeight = 300f;
            jackpotLayout.flexibleWidth = 1f;
            jackpotMeterView.Initialize(controller);

            // Flexible spacer pushes the bag down to the bottom.
            var rightSpacer = new GameObject("RightSpacer", typeof(RectTransform), typeof(LayoutElement));
            rightSpacer.transform.SetParent(rightUtilityPanel.transform, false);
            rightSpacer.GetComponent<LayoutElement>().flexibleHeight = 1f;

            // Small bag, anchored bottom-right.
            var bagRow = new GameObject("BagRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            bagRow.transform.SetParent(rightUtilityPanel.transform, false);
            bagRow.GetComponent<LayoutElement>().preferredHeight = 64f;
            var bagRowLayout = bagRow.GetComponent<HorizontalLayoutGroup>();
            bagRowLayout.childAlignment = TextAnchor.MiddleRight;
            bagRowLayout.childControlWidth = true;
            bagRowLayout.childControlHeight = true;
            bagRowLayout.childForceExpandWidth = false;
            bagRowLayout.childForceExpandHeight = false;

            bagPanel = new GameObject("BagPanel", typeof(RectTransform), typeof(LayoutElement)).AddComponent<BagPanelView>();
            bagPanel.transform.SetParent(bagRow.transform, false);
            var bagLayout = bagPanel.GetComponent<LayoutElement>();
            bagLayout.preferredWidth = 96f;
            bagLayout.preferredHeight = 60f;
            bagPanel.Initialize(controller);
        }

        private void Render(RunState run, ScoreResult score, string message)
        {
            var isShop = run.Phase == RunPhase.Shop;
            var isFloorProgress = run.Phase == RunPhase.FloorProgress;
            var isPlaying = run.Phase == RunPhase.PlayingLevel;
            var isReward = run.Phase == RunPhase.LevelReward;
            var isLost = run.Phase == RunPhase.RunLost;
            var isWon = run.Phase == RunPhase.RunWon;
            var isRunEnd = isLost || isWon;
            floorProgressView.gameObject.SetActive(isFloorProgress);
            gridPanel.SetActive(isPlaying);
            actionButtons.gameObject.SetActive(isPlaying);
            handPanel.SetActive(isPlaying);
            rightUtilityPanel.SetActive(true);
            shopView.gameObject.SetActive(isShop);
            modalOverlayRoot.SetActive(isReward || isRunEnd);
            if (isReward || isRunEnd)
            {
                modalOverlayRoot.transform.SetAsLastSibling();
            }
            levelRewardView.gameObject.SetActive(isReward);
            runLostView.gameObject.SetActive(isRunEnd);

            var preview = isPlaying && !controller.IsScoring ? controller.CalculateCurrentScoringPreview() : null;
            scorePanel.Render(run, score, preview, controller.IsScoring);
            bagPanel.Render(run);
            jackpotMeterView.Render(run);
            bossIntroView.Render(run);
            if (isFloorProgress)
            {
                floorProgressView.Render(run);
            }

            if (isPlaying)
            {
                gridView.Render(run.CurrentLevel.Grid);
                handView.Render(run.CurrentLevel.Hand, controller.SelectedDomino, controller.CurrentOrientation, controller.SelectedForDiscard);
                actionButtons.Render(controller);
            }

            if (isReward)
            {
                levelRewardView.Render(run);
            }

            if (isRunEnd)
            {
                runLostView.Render(run);
            }

            shopView.Render(run);
            dominexBar.Render(run);
            if (isPlaying && controller.IsScoring && !scoringAnimationRunning)
            {
                StartCoroutine(AnimateScoring(controller.LastScoreResult));
            }

            feedback.gameObject.SetActive(isPlaying || isShop || isFloorProgress);
            feedback.text = message;
            RenderMessageFeedback(message, isShop, isPlaying);
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
            if (controller.SelectedDomino == null || controller.BossIntroActive || controller.IsScoring)
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
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            DomiNoxTheme.AddOutline(panel, DomiNoxTheme.BorderNormal);
            return panel;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }

        private IEnumerator AnimateScoring(ScoreResult result)
        {
            scoringAnimationRunning = true;
            hasPointerPreview = false;
            activeDragView?.ForceClearDragGhost();
            activeDragView = null;

            var currentCount = 0;
            var currentMult = 1;
            var finalMultiplier = 1f;
            scorePanel.RenderScoringState("Scoring\nReady", currentCount, currentMult, finalMultiplier, detail: "Tile: 0\nMult: 1");
            yield return new WaitForSeconds(0.18f);

            foreach (var step in result.Steps)
            {
                if (step.Type == ScoringStepType.Reset)
                {
                    currentCount = 0;
                    currentMult = 1;
                    finalMultiplier = 1f;
                    scorePanel.RenderScoringState("Scoring\nStart", currentCount, currentMult, finalMultiplier, detail: "Tile: 0\nMult: 1");
                    feedbackService.Flash(new Color(0.2f, 0.45f, 1f), 0.08f);
                    yield return new WaitForSeconds(0.15f);
                    continue;
                }

                if (step.Type == ScoringStepType.FinalScore)
                {
                    yield return RollFinalScore(currentCount, currentMult, finalMultiplier, result.FinalScore);
                    var projectedLevelScore = controller.Run.CurrentLevel.CurrentScore + result.FinalScore;
                    var cleared = projectedLevelScore >= controller.Run.CurrentLevel.Quota;
                    feedbackService.Floating(cleared ? "CLEARED" : "FAILED", scorePanel.GetPatternAnchor(), cleared ? FloatingTextType.Credits : FloatingTextType.Warning);
                    feedbackService.Flash(cleared ? new Color(0.25f, 1f, 0.35f) : new Color(1f, 0.15f, 0.12f), cleared ? 0.18f : 0.12f);
                    feedbackService.CasinoBurst(scorePanel.GetPatternAnchor(), cleared ? new Color(1f, 0.78f, 0.25f) : new Color(1f, 0.25f, 0.18f), cleared ? 16 : 8);
                    yield return new WaitForSeconds(0.55f);
                    continue;
                }

                var anchor = GetStepAnchor(step);
                feedbackService.Pulse(anchor, step.Type == ScoringStepType.PatternComboCountBonus || step.Type == ScoringStepType.PatternComboMultBonus ? 1.2f : 1.1f);
                if (step.Type == ScoringStepType.DominoCount || step.Type == ScoringStepType.DominoModifier || step.Type == ScoringStepType.DomiNexCount || step.Type == ScoringStepType.DomiNexMult || step.Type == ScoringStepType.DomiNexMultiplier)
                {
                    feedbackService.Shake(anchor, 3.5f);
                }

                if (step.CountDelta != 0)
                {
                    currentCount += step.CountDelta;
                    feedbackService.Floating($"{step.CountDelta:+#;-#;0} Tile", anchor, FloatingTextType.Count);
                    feedbackService.CasinoBurst(anchor, new Color(0.2f, 0.65f, 1f), 5);
                }

                if (step.MultDelta != 0)
                {
                    currentMult += step.MultDelta;
                    feedbackService.Floating($"{step.MultDelta:+#;-#;0} Mult", anchor, FloatingTextType.Mult);
                    feedbackService.CasinoBurst(anchor, new Color(1f, 0.28f, 0.24f), 5);
                }

                if (step.MultiplierValue != 0f && step.MultiplierValue != 1f)
                {
                    finalMultiplier *= step.MultiplierValue;
                    feedbackService.Floating($"x{step.MultiplierValue:0.##}", anchor, FloatingTextType.Multiplier);
                    feedbackService.Flash(new Color(0.78f, 0.38f, 1f), 0.1f);
                }

                if (step.Type == ScoringStepType.PatternComboCountBonus || step.Type == ScoringStepType.PatternComboMultBonus)
                {
                    feedbackService.Floating("COMBO!", anchor, FloatingTextType.Credits);
                    feedbackService.CasinoBurst(anchor, new Color(1f, 0.78f, 0.25f), 12);
                }

                    scorePanel.RenderScoringState($"Scoring\n{step.SourceName}", currentCount, currentMult, finalMultiplier, detail: $"Level: {controller.Run.CurrentLevel.CurrentScore}/{controller.Run.CurrentLevel.Quota}\n{step.SourceName}\n{step.Description}\n\nTile: {currentCount}\nMult: {currentMult}");
                yield return new WaitForSeconds(0.32f);
            }

            foreach (var effect in result.PostScoringEffects)
            {
                var anchor = dominexBar.GetDomiNexRect(effect.SourceId) ?? scorePanel.GetPatternAnchor();
                feedbackService.Pulse(anchor, effect.Triggered ? 1.18f : 1.08f);
                feedbackService.Shake(anchor, effect.Triggered ? 6f : 2f);
                feedbackService.Floating(effect.Description, anchor, effect.Triggered ? FloatingTextType.Multiplier : FloatingTextType.Warning);
                if (effect.ActionType == PostScoringActionType.DestroySelf && effect.Triggered)
                {
                    feedbackService.Floating("Gros Michel broke!", anchor, FloatingTextType.Warning);
                    feedbackService.Flash(new Color(1f, 0.25f, 0.15f), 0.12f);
                }

                yield return new WaitForSeconds(0.45f);
            }

            if (result.DesignPatternId == PatternNames.ChristCrossId || result.DesignPatternId == PatternNames.BigLoopId)
            {
                var alreadyRevealed = controller.Run.RevealedSecretPatterns.Contains(result.DesignPatternId);
                if (!alreadyRevealed)
                {
                    feedbackService.Floating($"Secret Pattern Discovered\n{result.DesignPatternName}", scorePanel.GetPatternAnchor(), FloatingTextType.Multiplier);
                    feedbackService.Flash(new Color(0.75f, 0.25f, 1f), 0.16f);
                    feedbackService.CasinoBurst(scorePanel.GetPatternAnchor(), new Color(1f, 0.78f, 0.25f), 18);
                    yield return new WaitForSeconds(0.35f);
                }
            }

            scoringAnimationRunning = false;
            controller.CompleteScoringAnimation();
        }

        private RectTransform GetStepAnchor(ScoringStep step)
        {
            if (step.Domino != null)
            {
                return gridView.GetDominoRect(step.Domino) ?? scorePanel.GetPatternAnchor();
            }

            if (step.Type == ScoringStepType.DomiNexCount || step.Type == ScoringStepType.DomiNexMult || step.Type == ScoringStepType.DomiNexMultiplier)
            {
                return dominexBar.GetDomiNexRect(step.SourceId) ?? scorePanel.GetPatternAnchor();
            }

            return scorePanel.GetPatternAnchor();
        }

        private IEnumerator RollFinalScore(int count, int mult, float finalMultiplier, int finalScore)
        {
            const float duration = 0.45f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var displayed = Mathf.RoundToInt(Mathf.Lerp(0, finalScore, 1f - Mathf.Pow(1f - t, 3f)));
                var projected = controller.Run.CurrentLevel.CurrentScore + displayed;
                scorePanel.RenderScoringState("Hand Score", count, mult, finalMultiplier, displayed, $"Hand Score = {displayed}\nLevel Score: {projected}/{controller.Run.CurrentLevel.Quota}");
                yield return null;
            }

            scorePanel.RenderScoringState("Hand Score", count, mult, finalMultiplier, finalScore, $"Hand Score = {finalScore}\nLevel Score: {controller.Run.CurrentLevel.CurrentScore + finalScore}/{controller.Run.CurrentLevel.Quota}");
            feedbackService.Floating(finalScore.ToString(), scorePanel.GetPatternAnchor(), FloatingTextType.Multiplier);
        }

        private static bool WasKeyPressed(Key key)
        {
            return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
        }

        private void RenderMessageFeedback(string message, bool isShop, bool isPlaying)
        {
            if (feedbackService == null || string.IsNullOrWhiteSpace(message) || message == lastFeedbackMessage)
            {
                return;
            }

            lastFeedbackMessage = message;
            var anchor = feedback.rectTransform;
            if (message.Contains("achete") || message.Contains("vendu") || message.Contains("Cash out") || message.Contains("+"))
            {
                feedbackService.Floating(message, anchor, FloatingTextType.Credits);
                feedbackService.CasinoBurst(anchor, new Color(1f, 0.78f, 0.25f), isShop ? 10 : 6);
                feedbackService.Pulse(anchor, 1.12f);
                return;
            }

            if (message.Contains("upgraded") || message.Contains("level up"))
            {
                feedbackService.Floating(message, anchor, FloatingTextType.Multiplier);
                feedbackService.CasinoBurst(anchor, new Color(0.3f, 0.95f, 0.85f), 10);
                feedbackService.Pulse(anchor, 1.14f);
                return;
            }

            if (message.Contains("impossible") || message.Contains("invalide") || message.Contains("insuffisants") || message.Contains("Invalid"))
            {
                feedbackService.Floating(message, anchor, FloatingTextType.Warning);
                feedbackService.Shake(anchor, 8f);
                feedbackService.Flash(new Color(1f, 0.12f, 0.1f), 0.08f);
                return;
            }

            if (isPlaying && message.Contains("Domino place"))
            {
                feedbackService.Floating("Clack", anchor, FloatingTextType.Count);
                feedbackService.CasinoBurst(anchor, new Color(0.85f, 0.92f, 1f), 6);
            }
        }

        private static IEnumerator Pulse(RectTransform rect)
        {
            if (rect == null)
            {
                yield break;
            }

            var start = rect.localScale;
            const float duration = 0.16f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                rect.localScale = start * Mathf.Lerp(1f, 1.08f, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            rect.localScale = start;
        }
    }
}
