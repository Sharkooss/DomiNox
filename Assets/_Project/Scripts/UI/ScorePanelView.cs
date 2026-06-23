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
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;

            // --- Logo ---
            var logo = CreateSection("LogoSection", 58f, DomiNoxTheme.BgDeep);
            var logoText = UiFactory.CreateText(logo, "Logo", "DOMINOX", DomiNoxTheme.FontLG, TextAnchor.MiddleCenter);
            logoText.color = DomiNoxTheme.Gold;
            logoText.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(logoText.gameObject, new Color(0f, 0f, 0f, 0.6f), new Vector2(1f, -1f));

            // --- Blind / Quota ---
            var blind = CreateSection("BlindSection", 82f, DomiNoxTheme.BgPanel);
            blindTitle = UiFactory.CreateText(blind, "BlindTitle", string.Empty, DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            blindTitle.color = DomiNoxTheme.Gold;
            blindTitle.fontStyle = FontStyle.Bold;
            blindScore = UiFactory.CreateText(blind, "BlindScore", string.Empty, DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            blindScore.color = DomiNoxTheme.TextPrimary;
            levelInfo = UiFactory.CreateText(blind, "LevelInfo", string.Empty, DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            levelInfo.color = DomiNoxTheme.TextSecondary;

            // --- Count x Mult formula ---
            var score = CreateSection("ScoreSection", 118f, DomiNoxTheme.BgCard);
            roundScore = UiFactory.CreateText(score, "RoundScore", string.Empty, DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            roundScore.color = DomiNoxTheme.TextSecondary;
            roundScore.GetComponent<LayoutElement>().preferredHeight = 22f;

            var formula = new GameObject("Formula", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            formula.transform.SetParent(score, false);
            formula.GetComponent<LayoutElement>().preferredHeight = 58f;
            var formulaLayout = formula.GetComponent<HorizontalLayoutGroup>();
            formulaLayout.spacing = 6f;
            formulaLayout.childAlignment = TextAnchor.MiddleCenter;

            var (_, countText) = UiFactory.CreatePill(formula.transform, "CountPill", DomiNoxTheme.CountBlueDim, DomiNoxTheme.CountBlue, 74f, 54f);
            countValue = countText;
            countValue.fontSize = DomiNoxTheme.FontXL;

            var multiply = UiFactory.CreateText(formula.transform, "Multiply", "×", DomiNoxTheme.FontLG, TextAnchor.MiddleCenter);
            multiply.color = DomiNoxTheme.TextMuted;
            multiply.GetComponent<LayoutElement>().preferredWidth = 18f;

            var (_, multText) = UiFactory.CreatePill(formula.transform, "MultPill", DomiNoxTheme.MultRedDim, DomiNoxTheme.MultRed, 74f, 54f);
            multValue = multText;
            multValue.fontSize = DomiNoxTheme.FontXL;

            // --- Resources ---
            var resources = CreateSection("ResourceSection", 200f, DomiNoxTheme.BgPanel);
            handsValue    = CreateMetric(resources, "Hands",    "Hands");
            discardsValue = CreateMetric(resources, "Discards", "Discards");
            creditsValue  = CreateMetric(resources, "Credits",  "Credits");
            placedValue   = CreateMetric(resources, "Placed",   "Placed");
            bagValue      = CreateMetric(resources, "Bag",      "Bag");
            bossValue     = CreateMetric(resources, "Boss",     "Boss");
            BuildBossTooltip();

            // --- Run Info button ---
            var details = CreateSection("DetailsSection", 0f, DomiNoxTheme.BgDeep);
            details.GetComponent<LayoutElement>().flexibleHeight = 1f;

            var infoButton = UiFactory.CreateButton(details, "InfoButton", "RUN INFO");
            infoButton.GetComponent<LayoutElement>().preferredHeight = 50f;
            infoButton.GetComponent<LayoutElement>().flexibleWidth = 1f;
            UiFactory.StyleButton(infoButton, DomiNoxTheme.BgCard, DomiNoxTheme.Gold, 50f);
            var infoLabel = infoButton.GetComponentInChildren<Text>();
            if (infoLabel != null)
            {
                infoLabel.fontSize = DomiNoxTheme.FontMD;
                infoLabel.fontStyle = FontStyle.Bold;
            }
            DomiNoxTheme.AddShadow(infoButton.gameObject, new Color(0f, 0f, 0f, 0.4f), new Vector2(1f, -1f));
            infoButton.onClick.AddListener(ShowPatternOverlay);

            var spacer = new GameObject("DetailsSpacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(details, false);
            spacer.GetComponent<LayoutElement>().flexibleHeight = 1f;

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
            blindScore.text = $"Score {level.CurrentScore} / {level.Quota}";
            levelInfo.text = $"Floor {level.FloorIndex}  ·  Level {level.LevelIndex}";

            if (run.Phase == RunPhase.PlayingLevel && isScoring)
            {
                roundScore.text = $"Scoring  {level.CurrentScore}/{level.Quota}";
                countValue.text = "0";
                multValue.text = "1";
            }
            else if (run.Phase == RunPhase.PlayingLevel && preview != null)
            {
                roundScore.text = preview.IsCombo ? $"Combo: {preview.PatternName}" : $"{preview.PatternName} Lv.{preview.Level}";
                countValue.text = preview.CountBonus.ToString();
                multValue.text = preview.MultBonus.ToString();
            }
            else
            {
                roundScore.text = $"Level  {level.CurrentScore}/{level.Quota}";
                countValue.text = score.Count.ToString();
                multValue.text = score.Mult.ToString();
            }

            handsValue.text    = $"{level.HandsRemaining}/{level.MaxHands}";
            discardsValue.text = $"{level.DiscardsRemaining}/{level.MaxDiscards}";
            discardsValue.color = level.DiscardsRemaining == 0 ? DomiNoxTheme.Warning : DomiNoxTheme.Gold;
            creditsValue.text  = $"${run.Credits}";
            placedValue.text   = lightPlaced > 0 ? $"{placed}/{level.MaxPlacedDominoes} +{lightPlaced}L" : $"{placed}/{level.MaxPlacedDominoes}";
            bagValue.text      = $"{run.Bag.RemainingCount()} left";

            var displayedBoss = level.Boss?.Definition ?? run.CurrentFloorBoss;
            bossValue.text  = displayedBoss == null ? "—" : displayedBoss.Name;
            bossValue.color = displayedBoss == null ? DomiNoxTheme.TextMuted : DomiNoxTheme.Warning;
            bossTooltipText.text = level.Boss == null
                ? (displayedBoss == null ? "No active boss." : displayedBoss.Description)
                : level.Boss.GetEffectSummary();
        }

        public void RenderScoringState(string title, int count, int mult, float finalMultiplier, int? finalScore = null, string detail = null)
        {
            roundScore.text = finalScore.HasValue ? $"Final Score  {finalScore.Value}" : title;
            countValue.text = count.ToString();
            multValue.text  = finalMultiplier == 1f ? mult.ToString() : $"{mult} ×{finalMultiplier:0.##}";
        }

        public RectTransform GetPatternAnchor() => roundScore.rectTransform;

        private void BuildBossTooltip()
        {
            var trigger = bossValue.gameObject.AddComponent<EventTrigger>();
            AddHoverEvent(trigger, EventTriggerType.PointerEnter, () => SetBossTooltipVisible(true));
            AddHoverEvent(trigger, EventTriggerType.PointerExit,  () => SetBossTooltipVisible(false));

            var canvas = GetComponentInParent<Canvas>();
            bossTooltip = new GameObject("BossTooltip", typeof(RectTransform), typeof(Image));
            bossTooltip.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)bossTooltip.transform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot     = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(320f, 120f);
            bossTooltip.GetComponent<Image>().color = DomiNoxTheme.BgOverlay;
            DomiNoxTheme.AddOutline(bossTooltip, DomiNoxTheme.MultRed, 2f);
            DomiNoxTheme.AddShadow(bossTooltip, new Color(0f, 0f, 0f, 0.5f), new Vector2(3f, -3f));

            bossTooltipText = UiFactory.CreateText(bossTooltip.transform, "Text", string.Empty, DomiNoxTheme.FontSM, TextAnchor.MiddleLeft);
            bossTooltipText.color = DomiNoxTheme.TextPrimary;
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
            if (bossTooltip == null) return;
            bossTooltip.SetActive(visible && bossValue.text != "—");
            if (bossTooltip.activeSelf)
            {
                PositionBossTooltip();
                bossTooltip.transform.SetAsLastSibling();
            }
        }

        private void PositionBossTooltip()
        {
            var canvas      = GetComponentInParent<Canvas>();
            var canvasRect  = (RectTransform)canvas.transform;
            var tooltipRect = (RectTransform)bossTooltip.transform;
            var bossRect    = bossValue.rectTransform;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, bossRect.TransformPoint(new Vector3(bossRect.rect.xMax, bossRect.rect.center.y, 0f)));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.worldCamera, out var localPoint);
            var position = localPoint + new Vector2(18f, 56f);
            var halfWidth  = canvasRect.rect.width  * 0.5f;
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
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(70f, 46f);
            rect.offsetMax = new Vector2(-70f, -44f);
            patternOverlay.GetComponent<Image>().color = DomiNoxTheme.BgOverlay;
            var overlayOutline = patternOverlay.GetComponent<Outline>();
            overlayOutline.effectColor = DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.5f);
            overlayOutline.effectDistance = new Vector2(2f, -2f);
            DomiNoxTheme.AddShadow(patternOverlay, new Color(0f, 0f, 0f, 0.6f), new Vector2(0f, -4f));

            const float pad = 22f;
            const float titleH = 46f;
            const float introH = 18f;
            const float tabsH = 40f;
            var introTop = pad + titleH + 6f;
            var tabsTop = introTop + introH + 8f;
            var pagesTop = tabsTop + tabsH + 10f;

            // Title strip (thin, pinned to the top).
            var header = OverlayTopStrip("OverlayHeader", pad, titleH, false);
            var title = UiFactory.CreateText(header, "Title", "Run Info", DomiNoxTheme.FontXL, TextAnchor.MiddleLeft);
            title.color = DomiNoxTheme.Gold;
            title.fontStyle = FontStyle.Bold;
            SetAnchors(title.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-130f, 0f));

            var close = UiFactory.CreateButton(header, "Close", "Fermer");
            UiFactory.StyleButton(close, DomiNoxTheme.BgCard, DomiNoxTheme.Gold, 40f);
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = new Vector2(1f, 0.5f);
            closeRect.anchorMax = new Vector2(1f, 0.5f);
            closeRect.pivot = new Vector2(1f, 0.5f);
            closeRect.sizeDelta = new Vector2(118f, 40f);
            closeRect.anchoredPosition = Vector2.zero;
            close.onClick.AddListener(() => patternOverlay.SetActive(false));

            // Separator under the title.
            var sep = OverlayTopStrip("OverlaySep", pad + titleH + 2f, 1f, true);
            sep.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.4f);

            // Intro line.
            var introStrip = OverlayTopStrip("OverlayIntro", introTop, introH, false);
            var intro = UiFactory.CreateText(introStrip, "IntroText", "Le meilleur pattern de valeur et le meilleur design sont retenus (additif par region avec la Faille).", DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            intro.color = DomiNoxTheme.TextSecondary;
            SetAnchors(intro.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Tabs strip (thin row of buttons).
            var tabsStrip = OverlayTopStrip("OverlayTabs", tabsTop, tabsH, false);
            var tabsLayout = tabsStrip.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabsLayout.spacing = 10f;
            tabsLayout.childAlignment = TextAnchor.MiddleLeft;
            tabsLayout.childControlWidth = true;
            tabsLayout.childControlHeight = true;
            tabsLayout.childForceExpandWidth = false;
            tabsLayout.childForceExpandHeight = true;

            valuePatternsTab  = CreateTabButton(tabsStrip, "Patterns de valeur");
            designPatternsTab = CreateTabButton(tabsStrip, "Patterns de design");
            combosTab         = CreateTabButton(tabsStrip, "Combos");
            floorTab          = CreateTabButton(tabsStrip, "Floor");
            valuePatternsTab.onClick.AddListener(() => ShowPatternTab(true));
            designPatternsTab.onClick.AddListener(() => ShowPatternTab(false));
            combosTab.onClick.AddListener(ShowCombosTab);
            floorTab.onClick.AddListener(ShowFloorTab);

            // Pages fill the rest -> the big central content area.
            var pagesRoot = new GameObject("Pages", typeof(RectTransform));
            pagesRoot.transform.SetParent(patternOverlay.transform, false);
            var pagesRect = (RectTransform)pagesRoot.transform;
            pagesRect.anchorMin = new Vector2(0f, 0f);
            pagesRect.anchorMax = new Vector2(1f, 1f);
            pagesRect.offsetMin = new Vector2(pad, pad);
            pagesRect.offsetMax = new Vector2(-pad, -pagesTop);

            var valueContent = CreatePatternScrollPage(pagesRoot.transform, "ValuePatternsPage", out valuePatternsPage);
            foreach (var pattern in PatternCatalog.ValuePatterns)
                CreatePatternRow(valueContent, pattern, 78f, false);

            var designContent = CreatePatternScrollPage(pagesRoot.transform, "DesignPatternsPage", out designPatternsPage);
            foreach (var pattern in PatternCatalog.DesignPatterns)
                CreatePatternRow(designContent, pattern, 94f, true);
            foreach (var pattern in PatternCatalog.BonusPatterns)
                CreatePatternRow(designContent, pattern, 94f, true);

            var comboContent = CreatePatternScrollPage(pagesRoot.transform, "CombosPage", out combosPage);
            foreach (var combo in PatternComboCatalog.All)
                CreateComboRow(comboContent, combo);

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
            button.GetComponent<LayoutElement>().preferredWidth  = 210f;
            button.GetComponent<LayoutElement>().preferredHeight = 40f;
            UiFactory.StyleButton(button, DomiNoxTheme.BgCard, DomiNoxTheme.TextSecondary, 40f);
            return button;
        }

        private Transform CreatePatternScrollPage(Transform parent, string name, out GameObject page)
        {
            page = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(LayoutElement));
            page.transform.SetParent(parent, false);
            Stretch((RectTransform)page.transform);
            page.GetComponent<LayoutElement>().flexibleHeight  = 1f;
            page.GetComponent<LayoutElement>().preferredHeight = 420f;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(page.transform, false);
            Stretch((RectTransform)viewport.transform);
            viewport.GetComponent<Image>().color          = DomiNoxTheme.WithAlpha(DomiNoxTheme.BgDeep, 0.35f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot     = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(4, 12, 4, 12);
            contentLayout.spacing = 10f;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = page.GetComponent<ScrollRect>();
            scroll.viewport        = (RectTransform)viewport.transform;
            scroll.content         = contentRect;
            scroll.horizontal      = false;
            scroll.vertical        = true;
            scroll.movementType    = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            return content.transform;
        }

        private void ShowPatternTab(bool showValuePatterns)
        {
            valuePatternsPage.SetActive(showValuePatterns);
            designPatternsPage.SetActive(!showValuePatterns);
            combosPage.SetActive(false);
            floorPage.SetActive(false);
            SetTabActive(valuePatternsTab,  showValuePatterns);
            SetTabActive(designPatternsTab, !showValuePatterns);
            SetTabActive(combosTab, false);
            SetTabActive(floorTab,  false);
        }

        private void ShowCombosTab()
        {
            valuePatternsPage.SetActive(false);
            designPatternsPage.SetActive(false);
            combosPage.SetActive(true);
            floorPage.SetActive(false);
            SetTabActive(valuePatternsTab, false);
            SetTabActive(designPatternsTab, false);
            SetTabActive(combosTab, true);
            SetTabActive(floorTab,  false);
        }

        private void ShowFloorTab()
        {
            valuePatternsPage.SetActive(false);
            designPatternsPage.SetActive(false);
            combosPage.SetActive(false);
            floorPage.SetActive(true);
            SetTabActive(valuePatternsTab, false);
            SetTabActive(designPatternsTab, false);
            SetTabActive(combosTab, false);
            SetTabActive(floorTab, true);
            RefreshFloorPage();
        }

        private static void SetTabActive(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = active ? DomiNoxTheme.CountBlueDim : DomiNoxTheme.BgCard;
            var lbl = btn.GetComponentInChildren<Text>();
            if (lbl != null) lbl.color = active ? DomiNoxTheme.CountBlue : DomiNoxTheme.TextSecondary;
        }

        private void CreatePatternRow(Transform parent, PatternInfo pattern, float height, bool showDiagram)
        {
            var row = new GameObject($"Pattern_{pattern.Name}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = DomiNoxTheme.BgCard;
            DomiNoxTheme.AddOutline(row, DomiNoxTheme.BorderNormal);
            row.GetComponent<LayoutElement>().preferredHeight = height;
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 10, 10);
            layout.spacing = 12f;
            layout.childAlignment       = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;

            if (showDiagram)
                CreatePatternDiagram(row.transform, pattern);

            if (pattern.IsSecret)
            {
                secretPatternRows[pattern.Id] = row;
                row.SetActive(false);
            }

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            content.transform.SetParent(row.transform, false);
            content.GetComponent<LayoutElement>().flexibleWidth = 1f;
            content.GetComponent<VerticalLayoutGroup>().spacing = 4f;

            var titleRow = new GameObject("TitleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            titleRow.transform.SetParent(content.transform, false);
            titleRow.GetComponent<LayoutElement>().preferredHeight = 22f;
            titleRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var nameText = UiFactory.CreateText(titleRow.transform, "Name", pattern.Name, DomiNoxTheme.FontMD, TextAnchor.MiddleLeft);
            nameText.color = DomiNoxTheme.Gold;
            nameText.fontStyle = FontStyle.Bold;
            nameText.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var level = UiFactory.CreateText(titleRow.transform, "Level", "Lv. 1", DomiNoxTheme.FontSM, TextAnchor.MiddleRight);
            level.color = DomiNoxTheme.Warning;
            level.GetComponent<LayoutElement>().preferredWidth = 58f;
            patternLevelLabels[pattern.Id] = level;

            var req = UiFactory.CreateText(content.transform, "Requirement", pattern.Requirement, DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            req.color = DomiNoxTheme.TextSecondary;

            var eff = UiFactory.CreateText(content.transform, "Effect", pattern.Effect, DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            eff.color = DomiNoxTheme.CountBlue;
            patternEffectLabels[pattern.Id] = eff;
        }

        private void CreateComboRow(Transform parent, PatternComboDefinition combo)
        {
            var row = new GameObject($"Combo_{combo.Id}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = DomiNoxTheme.BgCard;
            var borderColor = combo.Difficulty == PatternComboDifficulty.Legendary ? DomiNoxTheme.RarityLegendary : DomiNoxTheme.RarityEpic;
            DomiNoxTheme.AddOutline(row, borderColor);
            row.GetComponent<LayoutElement>().preferredHeight = 74f;
            var layout = row.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 8, 8);
            layout.spacing = 3f;

            var nameText = UiFactory.CreateText(row.transform, "Name", $"{combo.Name}  |  {combo.Difficulty}", DomiNoxTheme.FontMD, TextAnchor.MiddleLeft);
            nameText.color = DomiNoxTheme.Gold;
            nameText.fontStyle = FontStyle.Bold;

            var patterns = UiFactory.CreateText(row.transform, "Patterns", $"{PatternCatalog.GetById(combo.ValuePatternId)?.Name} + {PatternCatalog.GetById(combo.DesignPatternId)?.Name}", DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            patterns.color = DomiNoxTheme.TextSecondary;

            var bonus = UiFactory.CreateText(row.transform, "Bonus", $"+{combo.CountBonus} Tile, +{combo.MultBonus} Mult", DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            bonus.color = DomiNoxTheme.RarityEpic;

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
                pair.Value.SetActive(CollectableVisibilityService.IsPatternVisibleInRunInfo(PatternCatalog.GetById(pair.Key), currentRun));

            foreach (var pair in secretComboRows)
            {
                var combo = PatternComboCatalog.All.FirstOrDefault(item => item.Id == pair.Key);
                pair.Value.SetActive(CollectableVisibilityService.IsComboVisibleInRunInfo(combo, currentRun));
            }

            if (floorPage != null && floorPage.activeSelf)
                RefreshFloorPage();
        }

        private void RefreshFloorPage()
        {
            if (floorContent == null || currentRun?.CurrentLevel == null) return;
            Clear(floorContent);
            var floorIndex = currentRun.CurrentLevel.FloorIndex;
            var firstLevel = ((floorIndex - 1) * GameConstants.LevelsPerFloor) + 1;
            AddFloorHeader(floorContent, $"Floor {floorIndex}", currentRun.CurrentFloorBoss);
            for (var offset = 0; offset < GameConstants.LevelsPerFloor; offset++)
            {
                var globalLevel  = firstLevel + offset;
                var levelInFloor = offset + 1;
                var isBoss  = levelInFloor == GameConstants.LevelsPerFloor;
                var status  = globalLevel < currentRun.CurrentLevel.LevelIndex ? "Cleared" : globalLevel == currentRun.CurrentLevel.LevelIndex ? "Current" : "Upcoming";
                var title   = isBoss ? $"Boss — {currentRun.CurrentFloorBoss?.Name ?? "Unknown Boss"}" : $"Table {levelInFloor}";
                var quota   = LevelQuotaService.GetQuotaForGlobalLevel(globalLevel);
                var rule    = isBoss && currentRun.CurrentFloorBoss != null ? currentRun.CurrentFloorBoss.Description : "Classic table";
                CreateFloorRow(floorContent, title, status, quota, rule, isBoss);
            }
        }

        private void AddFloorHeader(Transform parent, string title, DomiNox.Bosses.BossDefinition boss)
        {
            var header = new GameObject("FloorHeader", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(parent, false);
            header.GetComponent<Image>().color = DomiNoxTheme.BgPanel;
            DomiNoxTheme.AddOutline(header, DomiNoxTheme.BorderNormal);
            header.GetComponent<LayoutElement>().preferredHeight = 92f;
            var layout = header.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 10, 10);
            layout.spacing = 4f;

            var titleText = UiFactory.CreateText(header.transform, "Title", title, DomiNoxTheme.FontLG, TextAnchor.MiddleLeft);
            titleText.color = DomiNoxTheme.Gold;
            titleText.fontStyle = FontStyle.Bold;

            var bossText = UiFactory.CreateText(header.transform, "Boss", boss == null ? "Boss: unknown" : $"Upcoming Boss: {boss.Name}\nRule: {boss.Description}", DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            bossText.color = DomiNoxTheme.Warning;
        }

        private void CreateFloorRow(Transform parent, string title, string status, int quota, string rule, bool isBoss)
        {
            var row = new GameObject($"Floor_{title}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = status == "Current" ? DomiNoxTheme.BgCard : DomiNoxTheme.BgPanel;
            var borderColor = isBoss ? DomiNoxTheme.MultRed : status == "Current" ? DomiNoxTheme.Gold : DomiNoxTheme.BorderNormal;
            DomiNoxTheme.AddOutline(row, borderColor, isBoss || status == "Current" ? 2f : 1.5f);
            row.GetComponent<LayoutElement>().preferredHeight = 86f;
            var layout = row.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 8, 8);
            layout.spacing = 3f;

            var titleText = UiFactory.CreateText(row.transform, "Title", $"{title} — {status}", DomiNoxTheme.FontMD, TextAnchor.MiddleLeft);
            titleText.color = isBoss ? DomiNoxTheme.Danger : DomiNoxTheme.Gold;
            titleText.fontStyle = FontStyle.Bold;

            var quotaText = UiFactory.CreateText(row.transform, "Quota", $"Score at least {quota}", DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            quotaText.color = DomiNoxTheme.TextPrimary;

            var ruleText = UiFactory.CreateText(row.transform, "Rule", rule, DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            ruleText.color = DomiNoxTheme.TextSecondary;
        }

        private void CreatePatternDiagram(Transform parent, PatternInfo pattern)
        {
            var diagram = new GameObject("Diagram", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            diagram.transform.SetParent(parent, false);
            var layout = diagram.GetComponent<LayoutElement>();
            var rows   = pattern.DiagramRows.Length == 0 ? new[] { "...", ".X.", "..." } : pattern.DiagramRows;
            var rowCount    = rows.Length;
            var columnCount = rows.Max(row => row.Length);
            var cellSize    = pattern.Id == PatternNames.ChristCrossId ? 10f : 11f;
            layout.preferredWidth  = (cellSize + 2f) * columnCount;
            layout.preferredHeight = (cellSize + 2f) * rowCount;
            var grid = diagram.GetComponent<GridLayoutGroup>();
            grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columnCount;
            grid.cellSize = new Vector2(cellSize, cellSize);
            grid.spacing  = new Vector2(2f, 2f);

            for (var y = 0; y < rowCount; y++)
            {
                var row = rows[y];
                for (var x = 0; x < columnCount; x++)
                {
                    var marker = x < row.Length ? row[x] : '.';
                    var cell = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                    cell.transform.SetParent(diagram.transform, false);
                    cell.GetComponent<Image>().color = marker == '.' ? DomiNoxTheme.WithAlpha(DomiNoxTheme.BorderNormal, 0.5f) : DomiNoxTheme.CountBlueDim;
                }
            }
        }

        private Transform CreateSection(string name, float height, Color color)
        {
            var section = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            section.transform.SetParent(transform, false);
            section.GetComponent<Image>().color = color;
            DomiNoxTheme.AddOutline(section, DomiNoxTheme.BorderNormal);
            section.GetComponent<LayoutElement>().preferredHeight = height;
            var layout = section.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 7, 7);
            layout.spacing = 5f;
            layout.childAlignment = TextAnchor.UpperCenter;
            return section.transform;
        }

        private RectTransform OverlayTopStrip(string name, float topOffset, float height, bool withImage)
        {
            var go = withImage
                ? new GameObject(name, typeof(RectTransform), typeof(Image))
                : new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(patternOverlay.transform, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.offsetMin = new Vector2(22f, -(topOffset + height));
            r.offsetMax = new Vector2(-22f, -topOffset);
            return r;
        }

        private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
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
            foreach (Transform child in root) Destroy(child.gameObject);
        }

        private Text CreateMetric(Transform parent, string name, string label)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.GetComponent<LayoutElement>().preferredHeight = 24f;
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var labelText = UiFactory.CreateText(row.transform, "Label", label, DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            labelText.color = DomiNoxTheme.TextSecondary;
            labelText.GetComponent<LayoutElement>().preferredWidth = 92f;

            var valueText = UiFactory.CreateText(row.transform, "Value", string.Empty, DomiNoxTheme.FontMD, TextAnchor.MiddleRight);
            valueText.color = DomiNoxTheme.Gold;
            valueText.fontStyle = FontStyle.Bold;
            valueText.GetComponent<LayoutElement>().preferredWidth = 48f;
            return valueText;
        }
    }
}
