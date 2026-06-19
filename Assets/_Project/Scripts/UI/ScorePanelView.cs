using System.Collections.Generic;
using System.Linq;
using DomiNox.Core;
using DomiNox.Patterns;
using DomiNox.Run;
using DomiNox.Scoring;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class ScorePanelView : MonoBehaviour
    {
        private Text blindTitle;
        private Text blindScore;
        private Text levelInfo;
        private Text roundScore;
        private Text countValue;
        private Text multValue;
        private Text handsValue;
        private Text discardsValue;
        private Text creditsValue;
        private Text placedValue;
        private Text bagValue;
        private Text bossValue;
        private GameObject bossTooltip;
        private Text bossTooltipText;
        private Text breakdown;
        private GameObject patternOverlay;
        private Button valuePatternsTab;
        private Button designPatternsTab;
        private Button combosTab;
        private Button floorTab;
        private GameObject valuePatternsPage;
        private GameObject designPatternsPage;
        private GameObject combosPage;
        private GameObject floorPage;
        private Transform floorContent;
        private readonly Dictionary<string, Text> patternLevelLabels = new Dictionary<string, Text>();
        private readonly Dictionary<string, Text> patternEffectLabels = new Dictionary<string, Text>();
        private readonly Dictionary<string, GameObject> secretPatternRows = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> secretComboRows = new Dictionary<string, GameObject>();
        private RunState currentRun;

        public void Initialize()
        {
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 7f;
            layout.childAlignment = TextAnchor.UpperLeft;

            var logo = CreateSection("LogoSection", 54f, new Color(0.08f, 0.1f, 0.14f));
            var logoText = UiFactory.CreateText(logo, "Logo", "DomiNox", 20, TextAnchor.MiddleCenter);
            logoText.color = new Color(0.98f, 0.84f, 0.34f);

            var blind = CreateSection("BlindSection", 78f, new Color(0.1f, 0.12f, 0.15f));
            blindTitle = UiFactory.CreateText(blind, "BlindTitle", string.Empty, 16, TextAnchor.MiddleCenter);
            blindTitle.color = new Color(1f, 0.78f, 0.25f);
            blindScore = UiFactory.CreateText(blind, "BlindScore", string.Empty, 14, TextAnchor.MiddleCenter);
            levelInfo = UiFactory.CreateText(blind, "LevelInfo", string.Empty, 12, TextAnchor.MiddleCenter);
            levelInfo.color = new Color(0.65f, 0.72f, 0.8f);

            var score = CreateSection("ScoreSection", 106f, new Color(0.08f, 0.11f, 0.16f));
            roundScore = UiFactory.CreateText(score, "RoundScore", string.Empty, 14, TextAnchor.MiddleCenter);
            var formula = new GameObject("Formula", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            formula.transform.SetParent(score, false);
            formula.GetComponent<LayoutElement>().preferredHeight = 46f;
            var formulaLayout = formula.GetComponent<HorizontalLayoutGroup>();
            formulaLayout.spacing = 6f;
            formulaLayout.childAlignment = TextAnchor.MiddleCenter;
            countValue = CreateFormulaPill(formula.transform, "Count", new Color(0.06f, 0.54f, 1f));
            var multiply = UiFactory.CreateText(formula.transform, "Multiply", "x", 20, TextAnchor.MiddleCenter);
            multiply.GetComponent<LayoutElement>().preferredWidth = 16f;
            multValue = CreateFormulaPill(formula.transform, "Mult", new Color(1f, 0.25f, 0.22f));

            var resources = CreateSection("ResourceSection", 190f, new Color(0.08f, 0.1f, 0.13f));
            handsValue = CreateMetric(resources, "Hands", "Hands");
            discardsValue = CreateMetric(resources, "Discards", "Discards");
            creditsValue = CreateMetric(resources, "Credits", "Credits");
            placedValue = CreateMetric(resources, "Placed", "Placed");
            bagValue = CreateMetric(resources, "Bag", "Bag");
            bossValue = CreateMetric(resources, "Boss", "Boss");
            BuildBossTooltip();

            var details = CreateSection("DetailsSection", 0f, new Color(0.07f, 0.09f, 0.12f));
            details.GetComponent<LayoutElement>().flexibleHeight = 1f;
            var patternHeader = new GameObject("PatternHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            patternHeader.transform.SetParent(details, false);
            patternHeader.GetComponent<LayoutElement>().preferredHeight = 26f;
            var patternHeaderLayout = patternHeader.GetComponent<HorizontalLayoutGroup>();
            patternHeaderLayout.spacing = 6f;
            patternHeaderLayout.childAlignment = TextAnchor.MiddleCenter;
            var patternTitle = UiFactory.CreateText(patternHeader.transform, "Title", "Patterns", 13, TextAnchor.MiddleLeft);
            patternTitle.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var infoButton = UiFactory.CreateButton(patternHeader.transform, "InfoButton", "Run Info");
            infoButton.GetComponent<LayoutElement>().preferredWidth = 92f;
            infoButton.GetComponent<LayoutElement>().preferredHeight = 24f;
            infoButton.onClick.AddListener(ShowPatternOverlay);
            breakdown = UiFactory.CreateText(details, "Breakdown", string.Empty, 12, TextAnchor.UpperLeft);

            BuildPatternOverlay();
        }

        public void Render(RunState run, ScoreResult score, ScoringPreview preview, bool isScoring)
        {
            var level = run.CurrentLevel;
            currentRun = run;
            if (patternOverlay != null && patternOverlay.activeSelf)
            {
                RefreshPatternLevels();
            }
            var placedDominoes = level.Grid.GetPlacedDominoes();
            var placed = GameFlowController.CountPlacedAgainstLimit(level);
            var lightPlaced = placedDominoes.Count - placed;

            blindTitle.text = run.Phase == RunPhase.Shop ? "SHOP" : run.Phase == RunPhase.FloorProgress ? "FLOOR MAP" : "BIG BLIND";
            blindScore.text = $"Score\n{level.CurrentScore} / {level.Quota}";
            levelInfo.text = $"Floor {level.FloorIndex}  |  Level {level.LevelIndex}";
            if (run.Phase == RunPhase.PlayingLevel && isScoring)
            {
                roundScore.text = $"Scoring\n{level.CurrentScore}/{level.Quota}";
                countValue.text = "0";
                multValue.text = "1";
            }
            else if (run.Phase == RunPhase.PlayingLevel && preview != null)
            {
                roundScore.text = preview.IsCombo ? $"Combo\n{preview.PatternName}" : $"Pattern\n{preview.PatternName} Lv. {preview.Level}";
                countValue.text = preview.CountBonus.ToString();
                multValue.text = preview.MultBonus.ToString();
            }
            else
            {
                roundScore.text = $"Level score\n{level.CurrentScore}/{level.Quota}";
                countValue.text = score.Count.ToString();
                multValue.text = score.Mult.ToString();
            }
            handsValue.text = $"{level.HandsRemaining}/{level.MaxHands}";
            discardsValue.text = $"{level.DiscardsRemaining}/{level.MaxDiscards}";
            creditsValue.text = $"${run.Credits}";
            placedValue.text = lightPlaced > 0 ? $"{placed}/{level.MaxPlacedDominoes} (+{lightPlaced} Light)" : $"{placed}/{level.MaxPlacedDominoes}";
            bagValue.text = $"{run.Bag.RemainingCount()} left";
            var displayedBoss = level.Boss?.Definition ?? run.CurrentFloorBoss;
            bossValue.text = displayedBoss == null ? "-" : displayedBoss.Name;
            bossValue.color = displayedBoss == null ? Color.white : new Color(1f, 0.72f, 0.28f);
            bossTooltipText.text = level.Boss == null
                ? displayedBoss == null ? "Aucun boss actif." : displayedBoss.Description
                : level.Boss.GetEffectSummary();

            if (run.Phase == RunPhase.PlayingLevel && !isScoring && preview != null)
            {
                var title = preview.IsCombo ? "Combo" : "Pattern";
                var detail = string.IsNullOrWhiteSpace(preview.Detail) ? $"{preview.PatternName} Lv. {preview.Level}" : preview.Detail;
                breakdown.text = $"Level Score\n{level.CurrentScore} / {level.Quota}\n\n{title}\n{preview.PatternName}\n{detail}\n\n{preview.CountBonus} Tile x {preview.MultBonus} Mult";
            }
            else
            {
                var patterns = score.DetectedPatterns.Count == 0 ? "Aucun pattern" : string.Join(", ", score.DetectedPatterns);
                breakdown.text = $"{patterns}\n\n{string.Join("\n", score.BreakdownLines.Take(5))}";
            }
        }

        public void RenderScoringState(string title, int count, int mult, float finalMultiplier, int? finalScore = null, string detail = null)
        {
            roundScore.text = finalScore.HasValue ? $"Final Score\n{finalScore.Value}" : title;
            countValue.text = count.ToString();
            multValue.text = finalMultiplier == 1f ? mult.ToString() : $"{mult} x{finalMultiplier:0.##}";
            breakdown.text = detail ?? $"Tile: {count}\nMult: {mult}";
        }

        public RectTransform GetPatternAnchor()
        {
            return roundScore.rectTransform;
        }

        private void BuildBossTooltip()
        {
            var trigger = bossValue.gameObject.AddComponent<EventTrigger>();
            AddHoverEvent(trigger, EventTriggerType.PointerEnter, () => SetBossTooltipVisible(true));
            AddHoverEvent(trigger, EventTriggerType.PointerExit, () => SetBossTooltipVisible(false));

            var canvas = GetComponentInParent<Canvas>();
            bossTooltip = new GameObject("BossTooltip", typeof(RectTransform), typeof(Image), typeof(Outline));
            bossTooltip.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)bossTooltip.transform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(320f, 118f);
            bossTooltip.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.08f, 0.98f);
            var outline = bossTooltip.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 0.35f, 0.22f);
            outline.effectDistance = new Vector2(2f, -2f);

            bossTooltipText = UiFactory.CreateText(bossTooltip.transform, "Text", string.Empty, 14, TextAnchor.MiddleLeft);
            bossTooltipText.color = new Color(0.94f, 0.9f, 0.82f);
            bossTooltipText.rectTransform.anchorMin = Vector2.zero;
            bossTooltipText.rectTransform.anchorMax = Vector2.one;
            bossTooltipText.rectTransform.offsetMin = new Vector2(14f, 10f);
            bossTooltipText.rectTransform.offsetMax = new Vector2(-14f, -10f);
            bossTooltip.SetActive(false);
        }

        private static void AddHoverEvent(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction action)
        {
            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }

        private void SetBossTooltipVisible(bool visible)
        {
            if (bossTooltip == null)
            {
                return;
            }

            bossTooltip.SetActive(visible && bossValue.text != "-");
            if (bossTooltip.activeSelf)
            {
                PositionBossTooltip();
                bossTooltip.transform.SetAsLastSibling();
            }
        }

        private void PositionBossTooltip()
        {
            var canvas = GetComponentInParent<Canvas>();
            var canvasRect = (RectTransform)canvas.transform;
            var tooltipRect = (RectTransform)bossTooltip.transform;
            var bossRect = bossValue.rectTransform;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, bossRect.TransformPoint(new Vector3(bossRect.rect.xMax, bossRect.rect.center.y, 0f)));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.worldCamera, out var localPoint);
            var position = localPoint + new Vector2(18f, 56f);
            var halfWidth = canvasRect.rect.width * 0.5f;
            var halfHeight = canvasRect.rect.height * 0.5f;
            position.x = Mathf.Clamp(position.x, -halfWidth + 12f, halfWidth - tooltipRect.sizeDelta.x - 12f);
            position.y = Mathf.Clamp(position.y, -halfHeight + tooltipRect.sizeDelta.y + 12f, halfHeight - 12f);
            tooltipRect.anchoredPosition = position;
        }

        private void BuildPatternOverlay()
        {
            var canvas = GetComponentInParent<Canvas>();
            patternOverlay = new GameObject("PatternHelpOverlay", typeof(RectTransform), typeof(Image), typeof(Outline));
            patternOverlay.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)patternOverlay.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(980f, 650f);
            patternOverlay.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.12f, 0.98f);
            var outline = patternOverlay.GetComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.5f, 0.62f);
            outline.effectDistance = new Vector2(3f, -3f);

            var layout = patternOverlay.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 18, 22);
            layout.spacing = 12f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(patternOverlay.transform, false);
            header.GetComponent<LayoutElement>().preferredHeight = 40f;
            var headerLayout = header.GetComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = false;

            var title = UiFactory.CreateText(header.transform, "Title", "Patterns actuels", 20, TextAnchor.MiddleLeft);
            title.color = new Color(0.98f, 0.84f, 0.34f);
            title.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var close = UiFactory.CreateButton(header.transform, "Close", "Fermer");
            close.GetComponent<LayoutElement>().preferredWidth = 110f;
            close.onClick.AddListener(() => patternOverlay.SetActive(false));

            var intro = UiFactory.CreateText(patternOverlay.transform, "Intro", "Les patterns sont detectes automatiquement a la validation. Le scoring retient le meilleur pattern de valeur, le meilleur design, puis les bonus compatibles.", 13, TextAnchor.MiddleLeft);
            intro.color = new Color(0.7f, 0.76f, 0.84f);
            intro.GetComponent<LayoutElement>().preferredHeight = 36f;

            var tabs = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            tabs.transform.SetParent(patternOverlay.transform, false);
            tabs.GetComponent<LayoutElement>().preferredHeight = 44f;
            var tabsLayout = tabs.GetComponent<HorizontalLayoutGroup>();
            tabsLayout.spacing = 10f;
            tabsLayout.childAlignment = TextAnchor.MiddleLeft;
            tabsLayout.childForceExpandWidth = false;
            tabsLayout.childForceExpandHeight = false;
            valuePatternsTab = CreateTabButton(tabs.transform, "Patterns de valeur");
            designPatternsTab = CreateTabButton(tabs.transform, "Patterns de design");
            combosTab = CreateTabButton(tabs.transform, "Combos");
            floorTab = CreateTabButton(tabs.transform, "Floor");
            valuePatternsTab.onClick.AddListener(() => ShowPatternTab(true));
            designPatternsTab.onClick.AddListener(() => ShowPatternTab(false));
            combosTab.onClick.AddListener(ShowCombosTab);
            floorTab.onClick.AddListener(ShowFloorTab);

            var pagesRoot = new GameObject("Pages", typeof(RectTransform), typeof(LayoutElement));
            pagesRoot.transform.SetParent(patternOverlay.transform, false);
            pagesRoot.GetComponent<LayoutElement>().flexibleHeight = 1f;

            var valueContent = CreatePatternScrollPage(pagesRoot.transform, "ValuePatternsPage", out valuePatternsPage);
            foreach (var pattern in PatternCatalog.ValuePatterns)
            {
                CreatePatternRow(valueContent, pattern, 78f, false);
            }

            var designContent = CreatePatternScrollPage(pagesRoot.transform, "DesignPatternsPage", out designPatternsPage);
            foreach (var pattern in PatternCatalog.DesignPatterns)
            {
                CreatePatternRow(designContent, pattern, 94f, true);
            }

            foreach (var pattern in PatternCatalog.BonusPatterns)
            {
                CreatePatternRow(designContent, pattern, 94f, true);
            }

            var comboContent = CreatePatternScrollPage(pagesRoot.transform, "CombosPage", out combosPage);
            foreach (var combo in PatternComboCatalog.All)
            {
                CreateComboRow(comboContent, combo);
            }

            floorContent = CreatePatternScrollPage(pagesRoot.transform, "FloorPage", out floorPage);

            ShowPatternTab(true);
            patternOverlay.SetActive(false);
        }

        private void ShowPatternOverlay()
        {
            RefreshPatternLevels();
            patternOverlay.SetActive(true);
            patternOverlay.transform.SetAsLastSibling();
        }

        private Button CreateTabButton(Transform parent, string label)
        {
            var button = UiFactory.CreateButton(parent, $"{label}Tab", label);
            button.GetComponent<LayoutElement>().preferredWidth = 210f;
            button.GetComponent<LayoutElement>().preferredHeight = 40f;
            return button;
        }

        private Transform CreatePatternScrollPage(Transform parent, string name, out GameObject page)
        {
            page = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(LayoutElement));
            page.transform.SetParent(parent, false);
            Stretch((RectTransform)page.transform);
            page.GetComponent<LayoutElement>().flexibleHeight = 1f;
            page.GetComponent<LayoutElement>().preferredHeight = 420f;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(page.transform, false);
            Stretch((RectTransform)viewport.transform);
            viewport.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.1f, 0.35f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = new Vector2(0f, 0f);
            contentRect.offsetMax = new Vector2(0f, 0f);

            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(4, 12, 4, 12);
            contentLayout.spacing = 10f;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = page.GetComponent<ScrollRect>();
            scroll.viewport = (RectTransform)viewport.transform;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            return content.transform;
        }

        private void ShowPatternTab(bool showValuePatterns)
        {
            valuePatternsPage.SetActive(showValuePatterns);
            designPatternsPage.SetActive(!showValuePatterns);
            combosPage.SetActive(false);
            floorPage.SetActive(false);
            valuePatternsTab.GetComponent<Image>().color = showValuePatterns ? new Color(0.24f, 0.34f, 0.5f) : new Color(0.18f, 0.22f, 0.28f);
            designPatternsTab.GetComponent<Image>().color = showValuePatterns ? new Color(0.18f, 0.22f, 0.28f) : new Color(0.24f, 0.34f, 0.5f);
            combosTab.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f);
            floorTab.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f);
        }

        private void ShowCombosTab()
        {
            valuePatternsPage.SetActive(false);
            designPatternsPage.SetActive(false);
            combosPage.SetActive(true);
            floorPage.SetActive(false);
            valuePatternsTab.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f);
            designPatternsTab.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f);
            combosTab.GetComponent<Image>().color = new Color(0.24f, 0.34f, 0.5f);
            floorTab.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f);
        }

        private void ShowFloorTab()
        {
            valuePatternsPage.SetActive(false);
            designPatternsPage.SetActive(false);
            combosPage.SetActive(false);
            floorPage.SetActive(true);
            valuePatternsTab.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f);
            designPatternsTab.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f);
            combosTab.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f);
            floorTab.GetComponent<Image>().color = new Color(0.24f, 0.34f, 0.5f);
            RefreshFloorPage();
        }

        private void CreatePatternRow(Transform parent, PatternInfo pattern, float height, bool showDiagram)
        {
            var row = new GameObject($"Pattern_{pattern.Name}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.16f, 0.96f);
            var outline = row.GetComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.24f, 0.32f);
            outline.effectDistance = new Vector2(2f, -2f);
            row.GetComponent<LayoutElement>().preferredHeight = height;
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 10, 10);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;

            if (showDiagram)
            {
                CreatePatternDiagram(row.transform, pattern);
            }

            if (pattern.IsSecret)
            {
                secretPatternRows[pattern.Id] = row;
                row.SetActive(false);
            }

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            content.transform.SetParent(row.transform, false);
            content.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4f;

            var titleRow = new GameObject("TitleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            titleRow.transform.SetParent(content.transform, false);
            titleRow.GetComponent<LayoutElement>().preferredHeight = 22f;
            var titleLayout = titleRow.GetComponent<HorizontalLayoutGroup>();
            titleLayout.childAlignment = TextAnchor.MiddleCenter;
            var name = UiFactory.CreateText(titleRow.transform, "Name", pattern.Name, 16, TextAnchor.MiddleLeft);
            name.color = new Color(0.98f, 0.84f, 0.34f);
            name.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var level = UiFactory.CreateText(titleRow.transform, "Level", "Lv. 1", 13, TextAnchor.MiddleRight);
            level.color = new Color(0.95f, 0.85f, 0.42f);
            level.GetComponent<LayoutElement>().preferredWidth = 58f;
            patternLevelLabels[pattern.Id] = level;
            var requirement = UiFactory.CreateText(content.transform, "Requirement", pattern.Requirement, 12, TextAnchor.MiddleLeft);
            requirement.color = new Color(0.78f, 0.84f, 0.92f);
            var effect = UiFactory.CreateText(content.transform, "Effect", pattern.Effect, 12, TextAnchor.MiddleLeft);
            effect.color = new Color(0.58f, 0.78f, 1f);
            patternEffectLabels[pattern.Id] = effect;
        }

        private void CreateComboRow(Transform parent, PatternComboDefinition combo)
        {
            var row = new GameObject($"Combo_{combo.Id}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = new Color(0.1f, 0.09f, 0.15f, 0.96f);
            var outline = row.GetComponent<Outline>();
            outline.effectColor = combo.Difficulty == PatternComboDifficulty.Legendary ? new Color(1f, 0.72f, 0.25f) : new Color(0.32f, 0.22f, 0.46f);
            outline.effectDistance = new Vector2(2f, -2f);
            row.GetComponent<LayoutElement>().preferredHeight = 74f;
            var layout = row.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 8, 8);
            layout.spacing = 3f;

            var name = UiFactory.CreateText(row.transform, "Name", $"{combo.Name}  |  {combo.Difficulty}", 15, TextAnchor.MiddleLeft);
            name.color = new Color(0.98f, 0.84f, 0.34f);
            var patterns = UiFactory.CreateText(row.transform, "Patterns", $"{PatternCatalog.GetById(combo.ValuePatternId)?.Name} + {PatternCatalog.GetById(combo.DesignPatternId)?.Name}", 12, TextAnchor.MiddleLeft);
            patterns.color = new Color(0.78f, 0.84f, 0.92f);
            var bonus = UiFactory.CreateText(row.transform, "Bonus", $"+{combo.CountBonus} Tile, +{combo.MultBonus} Mult", 12, TextAnchor.MiddleLeft);
            bonus.color = new Color(0.72f, 0.55f, 1f);
            var designPattern = PatternCatalog.GetById(combo.DesignPatternId);
            if (designPattern?.IsSecret == true)
            {
                secretComboRows[combo.Id] = row;
                row.SetActive(false);
            }
        }

        private void RefreshPatternLevels()
        {
            foreach (var pair in patternLevelLabels)
            {
                var level = currentRun?.PatternLevels.GetLevel(pair.Key) ?? 1;
                pair.Value.text = $"Lv. {level}";
                if (patternEffectLabels.TryGetValue(pair.Key, out var effectLabel))
                {
                    var pattern = PatternCatalog.GetById(pair.Key);
                    if (pattern != null)
                    {
                        var bonus = PatternScalingService.GetScaledPatternBonus(pattern, level);
                        effectLabel.text = $"+{bonus.Count} Tile, +{bonus.Mult} Mult";
                    }
                }
            }

            foreach (var pair in secretPatternRows)
            {
                pair.Value.SetActive(CollectableVisibilityService.IsPatternVisibleInRunInfo(PatternCatalog.GetById(pair.Key), currentRun));
            }

            foreach (var pair in secretComboRows)
            {
                var combo = PatternComboCatalog.All.FirstOrDefault(item => item.Id == pair.Key);
                pair.Value.SetActive(CollectableVisibilityService.IsComboVisibleInRunInfo(combo, currentRun));
            }

            if (floorPage != null && floorPage.activeSelf)
            {
                RefreshFloorPage();
            }
        }

        private void RefreshFloorPage()
        {
            if (floorContent == null || currentRun?.CurrentLevel == null)
            {
                return;
            }

            Clear(floorContent);
            var floorIndex = currentRun.CurrentLevel.FloorIndex;
            var firstLevel = ((floorIndex - 1) * GameConstants.LevelsPerFloor) + 1;
            AddFloorHeader(floorContent, $"Floor {floorIndex}", currentRun.CurrentFloorBoss);
            for (var offset = 0; offset < GameConstants.LevelsPerFloor; offset++)
            {
                var globalLevel = firstLevel + offset;
                var levelInFloor = offset + 1;
                var isBoss = levelInFloor == GameConstants.LevelsPerFloor;
                var status = globalLevel < currentRun.CurrentLevel.LevelIndex ? "Cleared" : globalLevel == currentRun.CurrentLevel.LevelIndex ? "Current" : "Upcoming";
                var title = isBoss
                    ? $"Boss - {currentRun.CurrentFloorBoss?.Name ?? "Unknown Boss"}"
                    : $"Table {levelInFloor}";
                var quota = LevelQuotaService.GetQuotaForGlobalLevel(globalLevel);
                var rule = isBoss && currentRun.CurrentFloorBoss != null ? currentRun.CurrentFloorBoss.Description : "Classic table";
                CreateFloorRow(floorContent, title, status, quota, rule, isBoss);
            }
        }

        private void AddFloorHeader(Transform parent, string title, DomiNox.Bosses.BossDefinition boss)
        {
            var header = new GameObject("FloorHeader", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(parent, false);
            header.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.14f, 0.96f);
            header.GetComponent<LayoutElement>().preferredHeight = 92f;
            var layout = header.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 10, 10);
            layout.spacing = 4f;
            var titleText = UiFactory.CreateText(header.transform, "Title", title, 22, TextAnchor.MiddleLeft);
            titleText.color = new Color(0.98f, 0.84f, 0.34f);
            var bossText = UiFactory.CreateText(header.transform, "Boss", boss == null ? "Boss: unknown" : $"Upcoming Boss: {boss.Name}\nRule: {boss.Description}", 13, TextAnchor.MiddleLeft);
            bossText.color = new Color(1f, 0.72f, 0.28f);
        }

        private void CreateFloorRow(Transform parent, string title, string status, int quota, string rule, bool isBoss)
        {
            var row = new GameObject($"Floor_{title}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = status == "Current" ? new Color(0.12f, 0.16f, 0.23f, 0.98f) : new Color(0.08f, 0.1f, 0.14f, 0.96f);
            var outline = row.GetComponent<Outline>();
            outline.effectColor = isBoss ? new Color(1f, 0.35f, 0.22f) : status == "Current" ? new Color(0.98f, 0.84f, 0.34f) : new Color(0.18f, 0.24f, 0.32f);
            outline.effectDistance = new Vector2(2f, -2f);
            row.GetComponent<LayoutElement>().preferredHeight = 86f;
            var layout = row.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 8, 8);
            layout.spacing = 3f;
            var titleText = UiFactory.CreateText(row.transform, "Title", $"{title} - {status}", 16, TextAnchor.MiddleLeft);
            titleText.color = isBoss ? new Color(1f, 0.55f, 0.32f) : new Color(0.98f, 0.84f, 0.34f);
            var quotaText = UiFactory.CreateText(row.transform, "Quota", $"Score at least {quota}", 13, TextAnchor.MiddleLeft);
            quotaText.color = new Color(0.82f, 0.86f, 0.92f);
            var ruleText = UiFactory.CreateText(row.transform, "Rule", rule, 12, TextAnchor.MiddleLeft);
            ruleText.color = new Color(0.65f, 0.72f, 0.8f);
        }

        private void CreatePatternDiagram(Transform parent, PatternInfo pattern)
        {
            var diagram = new GameObject("Diagram", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            diagram.transform.SetParent(parent, false);
            var layout = diagram.GetComponent<LayoutElement>();
            var rows = pattern.DiagramRows.Length == 0 ? new[] { "...", ".X.", "..." } : pattern.DiagramRows;
            var rowCount = rows.Length;
            var columnCount = rows.Max(row => row.Length);
            var cellSize = pattern.Id == PatternNames.ChristCrossId ? 10f : 11f;
            layout.preferredWidth = (cellSize + 2f) * columnCount;
            layout.preferredHeight = (cellSize + 2f) * rowCount;
            var grid = diagram.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columnCount;
            grid.cellSize = new Vector2(cellSize, cellSize);
            grid.spacing = new Vector2(2f, 2f);

            for (var y = 0; y < rowCount; y++)
            {
                var row = rows[y];
                for (var x = 0; x < columnCount; x++)
                {
                    var marker = x < row.Length ? row[x] : '.';
                    var cell = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                    cell.transform.SetParent(diagram.transform, false);
                    cell.GetComponent<Image>().color = marker == '.'
                        ? new Color(0.16f, 0.19f, 0.24f, 0.9f)
                        : new Color(0.28f, 0.58f, 1f, 0.95f);
                }
            }
        }

        private Transform CreateSection(string name, float height, Color color)
        {
            var section = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            section.transform.SetParent(transform, false);
            section.GetComponent<Image>().color = color;
            var outline = section.GetComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.22f, 0.28f);
            outline.effectDistance = new Vector2(2f, -2f);
            section.GetComponent<LayoutElement>().preferredHeight = height;
            var layout = section.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 7, 7);
            layout.spacing = 5f;
            layout.childAlignment = TextAnchor.UpperCenter;
            return section.transform;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Clear(Transform root)
        {
            foreach (Transform child in root)
            {
                Destroy(child.gameObject);
            }
        }

        private Text CreateFormulaPill(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            var layout = go.GetComponent<LayoutElement>();
            layout.preferredWidth = 58f;
            layout.preferredHeight = 42f;
            var text = UiFactory.CreateText(go.transform, "Value", string.Empty, 22, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return text;
        }

        private Text CreateMetric(Transform parent, string name, string label)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.GetComponent<LayoutElement>().preferredHeight = 22f;
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var labelText = UiFactory.CreateText(row.transform, "Label", label, 12, TextAnchor.MiddleLeft);
            labelText.color = new Color(0.62f, 0.68f, 0.76f);
            labelText.GetComponent<LayoutElement>().preferredWidth = 92f;
            var valueText = UiFactory.CreateText(row.transform, "Value", string.Empty, 18, TextAnchor.MiddleRight);
            valueText.color = new Color(1f, 0.78f, 0.25f);
            valueText.GetComponent<LayoutElement>().preferredWidth = 48f;
            return valueText;
        }
    }
}
