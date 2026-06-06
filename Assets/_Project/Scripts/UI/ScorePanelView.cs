using System.Linq;
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
        private Text discardsValue;
        private Text creditsValue;
        private Text placedValue;
        private Text bossValue;
        private GameObject bossTooltip;
        private Text bossTooltipText;
        private Text breakdown;
        private GameObject patternOverlay;
        private Button valuePatternsTab;
        private Button designPatternsTab;
        private GameObject valuePatternsPage;
        private GameObject designPatternsPage;

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

            var resources = CreateSection("ResourceSection", 144f, new Color(0.08f, 0.1f, 0.13f));
            discardsValue = CreateMetric(resources, "Discards", "Discards");
            creditsValue = CreateMetric(resources, "Credits", "Credits");
            placedValue = CreateMetric(resources, "Placed", "Placed");
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
            var infoButton = UiFactory.CreateButton(patternHeader.transform, "InfoButton", "?");
            infoButton.GetComponent<LayoutElement>().preferredWidth = 28f;
            infoButton.GetComponent<LayoutElement>().preferredHeight = 24f;
            infoButton.onClick.AddListener(ShowPatternOverlay);
            breakdown = UiFactory.CreateText(details, "Breakdown", string.Empty, 12, TextAnchor.UpperLeft);

            BuildPatternOverlay();
        }

        public void Render(RunState run, ScoreResult score)
        {
            var level = run.CurrentLevel;
            var placed = level.Grid.GetPlacedDominoes().Count;

            blindTitle.text = run.Phase == RunPhase.Shop ? "SHOP" : run.Phase == RunPhase.FloorProgress ? "FLOOR MAP" : "BIG BLIND";
            blindScore.text = $"Score at least\n{level.Quota}";
            levelInfo.text = $"Floor {level.FloorIndex}  |  Level {level.LevelIndex}";
            roundScore.text = $"Round score\n{score.FinalScore}";
            countValue.text = score.Count.ToString();
            multValue.text = score.Mult.ToString();
            discardsValue.text = level.DiscardsRemaining.ToString();
            creditsValue.text = $"${run.Credits}";
            placedValue.text = $"{placed}/{level.MaxPlacedDominoes}";
            var displayedBoss = level.Boss?.Definition ?? run.CurrentFloorBoss;
            bossValue.text = displayedBoss == null ? "-" : displayedBoss.Name;
            bossValue.color = displayedBoss == null ? Color.white : new Color(1f, 0.72f, 0.28f);
            bossTooltipText.text = level.Boss == null
                ? displayedBoss == null ? "Aucun boss actif." : displayedBoss.Description
                : level.Boss.GetEffectSummary();

            var patterns = score.DetectedPatterns.Count == 0 ? "Aucun pattern" : string.Join(", ", score.DetectedPatterns);
            breakdown.text = $"{patterns}\n\n{string.Join("\n", score.BreakdownLines.Take(5))}";
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
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(260f, 40f);
            rect.sizeDelta = new Vector2(360f, 126f);
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
                bossTooltip.transform.SetAsLastSibling();
            }
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
            valuePatternsTab.onClick.AddListener(() => ShowPatternTab(true));
            designPatternsTab.onClick.AddListener(() => ShowPatternTab(false));

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

            ShowPatternTab(true);
            patternOverlay.SetActive(false);
        }

        private void ShowPatternOverlay()
        {
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
            valuePatternsTab.GetComponent<Image>().color = showValuePatterns ? new Color(0.24f, 0.34f, 0.5f) : new Color(0.18f, 0.22f, 0.28f);
            designPatternsTab.GetComponent<Image>().color = showValuePatterns ? new Color(0.18f, 0.22f, 0.28f) : new Color(0.24f, 0.34f, 0.5f);
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

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            content.transform.SetParent(row.transform, false);
            content.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4f;

            var name = UiFactory.CreateText(content.transform, "Name", pattern.Name, 16, TextAnchor.MiddleLeft);
            name.color = new Color(0.98f, 0.84f, 0.34f);
            var requirement = UiFactory.CreateText(content.transform, "Requirement", pattern.Requirement, 12, TextAnchor.MiddleLeft);
            requirement.color = new Color(0.78f, 0.84f, 0.92f);
            var effect = UiFactory.CreateText(content.transform, "Effect", pattern.Effect, 12, TextAnchor.MiddleLeft);
            effect.color = new Color(0.58f, 0.78f, 1f);
        }

        private void CreatePatternDiagram(Transform parent, PatternInfo pattern)
        {
            var diagram = new GameObject("Diagram", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            diagram.transform.SetParent(parent, false);
            var layout = diagram.GetComponent<LayoutElement>();
            layout.preferredWidth = 39f;
            layout.preferredHeight = 39f;
            var grid = diagram.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(11f, 11f);
            grid.spacing = new Vector2(2f, 2f);

            var rows = pattern.DiagramRows.Length == 0 ? new[] { "...", ".X.", "..." } : pattern.DiagramRows;
            for (var y = 0; y < 3; y++)
            {
                var row = y < rows.Length ? rows[y] : "...";
                for (var x = 0; x < 3; x++)
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
