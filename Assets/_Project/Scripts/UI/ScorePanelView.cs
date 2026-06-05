using System.Linq;
using DomiNox.Dominex;
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

        public void Initialize()
        {
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;

            var blind = CreateSection("BlindSection", 96f, new Color(0.1f, 0.12f, 0.15f));
            blindTitle = UiFactory.CreateText(blind, "BlindTitle", string.Empty, 18, TextAnchor.MiddleCenter);
            blindTitle.color = new Color(1f, 0.78f, 0.25f);
            blindScore = UiFactory.CreateText(blind, "BlindScore", string.Empty, 16, TextAnchor.MiddleCenter);
            levelInfo = UiFactory.CreateText(blind, "LevelInfo", string.Empty, 12, TextAnchor.MiddleCenter);
            levelInfo.color = new Color(0.65f, 0.72f, 0.8f);

            var score = CreateSection("ScoreSection", 114f, new Color(0.08f, 0.11f, 0.16f));
            roundScore = UiFactory.CreateText(score, "RoundScore", string.Empty, 14, TextAnchor.MiddleCenter);
            var formula = new GameObject("Formula", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            formula.transform.SetParent(score, false);
            formula.GetComponent<LayoutElement>().preferredHeight = 46f;
            var formulaLayout = formula.GetComponent<HorizontalLayoutGroup>();
            formulaLayout.spacing = 6f;
            formulaLayout.childAlignment = TextAnchor.MiddleCenter;
            countValue = CreateFormulaPill(formula.transform, "Count", new Color(0.06f, 0.54f, 1f));
            var multiply = UiFactory.CreateText(formula.transform, "Multiply", "x", 24, TextAnchor.MiddleCenter);
            multiply.GetComponent<LayoutElement>().preferredWidth = 22f;
            multValue = CreateFormulaPill(formula.transform, "Mult", new Color(1f, 0.25f, 0.22f));

            var resources = CreateSection("ResourceSection", 122f, new Color(0.08f, 0.1f, 0.13f));
            actionsValue = CreateMetric(resources, "Actions", "AP");
            discardsValue = CreateMetric(resources, "Discards", "Discards");
            creditsValue = CreateMetric(resources, "Credits", "Credits");
            placedValue = CreateMetric(resources, "Placed", "Placed");

            var details = CreateSection("DetailsSection", 150f, new Color(0.07f, 0.09f, 0.12f));
            breakdown = UiFactory.CreateText(details, "Breakdown", string.Empty, 12, TextAnchor.UpperLeft);
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
            breakdown.text = $"Patterns\n{patterns}\n\n{string.Join("\n", score.BreakdownLines.Take(5))}";
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
            layout.preferredWidth = 76f;
            layout.preferredHeight = 42f;
            var text = UiFactory.CreateText(go.transform, "Value", string.Empty, 24, TextAnchor.MiddleCenter);
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
            labelText.GetComponent<LayoutElement>().preferredWidth = 98f;
            var valueText = UiFactory.CreateText(row.transform, "Value", string.Empty, 18, TextAnchor.MiddleRight);
            valueText.color = new Color(1f, 0.78f, 0.25f);
            valueText.GetComponent<LayoutElement>().preferredWidth = 72f;
            return valueText;
        }
    }
}
