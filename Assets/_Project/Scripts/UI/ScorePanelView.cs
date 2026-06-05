using System.Linq;
using DomiNox.Patterns;
using DomiNox.Run;
using DomiNox.Scoring;
using DomiNox.Utilities;
using UnityEngine;
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
        private Text actionsValue;
        private Text discardsValue;
        private Text creditsValue;
        private Text placedValue;
        private Text breakdown;
        private GameObject patternOverlay;
        private Transform patternListRoot;

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

            var resources = CreateSection("ResourceSection", 116f, new Color(0.08f, 0.1f, 0.13f));
            actionsValue = CreateMetric(resources, "Actions", "AP");
            discardsValue = CreateMetric(resources, "Discards", "Discards");
            creditsValue = CreateMetric(resources, "Credits", "Credits");
            placedValue = CreateMetric(resources, "Placed", "Placed");

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

            blindTitle.text = run.Phase == RunPhase.Shop ? "SHOP" : "BIG BLIND";
            blindScore.text = $"Score at least\n{level.Quota}";
            levelInfo.text = $"Floor {level.FloorIndex}  |  Level {level.LevelIndex}";
            roundScore.text = $"Round score\n{score.FinalScore}";
            countValue.text = score.Count.ToString();
            multValue.text = score.Mult.ToString();
            actionsValue.text = level.ActionPoints.ToString();
            discardsValue.text = level.DiscardsRemaining.ToString();
            creditsValue.text = $"${run.Credits}";
            placedValue.text = $"{placed}/{level.MaxPlacedDominoes}";

            var patterns = score.DetectedPatterns.Count == 0 ? "Aucun pattern" : string.Join(", ", score.DetectedPatterns);
            breakdown.text = $"{patterns}\n\n{string.Join("\n", score.BreakdownLines.Take(5))}";
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
            rect.sizeDelta = new Vector2(720f, 500f);
            patternOverlay.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.12f, 0.98f);
            var outline = patternOverlay.GetComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.5f, 0.62f);
            outline.effectDistance = new Vector2(3f, -3f);

            var layout = patternOverlay.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 14, 16);
            layout.spacing = 10f;

            var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(patternOverlay.transform, false);
            header.GetComponent<LayoutElement>().preferredHeight = 40f;
            var headerLayout = header.GetComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleCenter;

            var title = UiFactory.CreateText(header.transform, "Title", "Patterns actuels", 20, TextAnchor.MiddleLeft);
            title.color = new Color(0.98f, 0.84f, 0.34f);
            title.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var close = UiFactory.CreateButton(header.transform, "Close", "Fermer");
            close.GetComponent<LayoutElement>().preferredWidth = 110f;
            close.onClick.AddListener(() => patternOverlay.SetActive(false));

            var intro = UiFactory.CreateText(patternOverlay.transform, "Intro", "Ces patterns sont detectes automatiquement quand tu valides ton score.", 13, TextAnchor.MiddleLeft);
            intro.color = new Color(0.7f, 0.76f, 0.84f);
            intro.GetComponent<LayoutElement>().preferredHeight = 28f;

            patternListRoot = new GameObject("PatternList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement)).transform;
            patternListRoot.SetParent(patternOverlay.transform, false);
            patternListRoot.GetComponent<LayoutElement>().flexibleHeight = 1f;
            var listLayout = patternListRoot.GetComponent<VerticalLayoutGroup>();
            listLayout.spacing = 8f;

            foreach (var pattern in PatternCatalog.All)
            {
                CreatePatternRow(pattern);
            }

            patternOverlay.SetActive(false);
        }

        private void ShowPatternOverlay()
        {
            patternOverlay.SetActive(true);
            patternOverlay.transform.SetAsLastSibling();
        }

        private void CreatePatternRow(PatternInfo pattern)
        {
            var row = new GameObject($"Pattern_{pattern.Name}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(patternListRoot, false);
            row.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.16f, 0.96f);
            var outline = row.GetComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.24f, 0.32f);
            outline.effectDistance = new Vector2(2f, -2f);
            row.GetComponent<LayoutElement>().preferredHeight = 72f;
            var layout = row.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 7, 7);
            layout.spacing = 3f;

            var name = UiFactory.CreateText(row.transform, "Name", pattern.Name, 16, TextAnchor.MiddleLeft);
            name.color = new Color(0.98f, 0.84f, 0.34f);
            var requirement = UiFactory.CreateText(row.transform, "Requirement", $"Comment l'obtenir: {pattern.Requirement}", 12, TextAnchor.MiddleLeft);
            requirement.color = new Color(0.78f, 0.84f, 0.92f);
            var effect = UiFactory.CreateText(row.transform, "Effect", $"Effet: {pattern.Effect}", 12, TextAnchor.MiddleLeft);
            effect.color = new Color(0.58f, 0.78f, 1f);
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
