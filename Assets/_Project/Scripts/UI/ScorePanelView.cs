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
        private Text stats;
        private Transform dominexRoot;
        private DomiNexDetailCardView hoverCard;
        private Text breakdown;

        public void Initialize()
        {
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            stats = UiFactory.CreateText(transform, "Stats", string.Empty, 20, TextAnchor.UpperLeft);

            var dominexTitle = UiFactory.CreateText(transform, "DomiNexTitle", "DomiNex actifs:", 16, TextAnchor.UpperLeft);
            dominexTitle.color = new Color(0.98f, 0.86f, 0.38f);

            dominexRoot = new GameObject("DomiNexList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement)).transform;
            dominexRoot.SetParent(transform, false);
            dominexRoot.GetComponent<VerticalLayoutGroup>().spacing = 4f;
            dominexRoot.GetComponent<LayoutElement>().preferredHeight = 120f;

            hoverCard = new GameObject("DomiNexHoverCard", typeof(RectTransform), typeof(LayoutElement)).AddComponent<DomiNexDetailCardView>();
            hoverCard.transform.SetParent(transform, false);
            hoverCard.GetComponent<LayoutElement>().preferredHeight = 210f;
            hoverCard.Initialize("Survole un DomiNex actif");
            hoverCard.gameObject.SetActive(false);

            breakdown = UiFactory.CreateText(transform, "Breakdown", string.Empty, 16, TextAnchor.UpperLeft);
        }

        public void Render(RunState run, ScoreResult score)
        {
            var level = run.CurrentLevel;
            stats.text = $"Phase: {run.Phase}\nFloor {level.FloorIndex}\nLevel {level.LevelIndex}\nQuota: {level.Quota}\nCount: {score.Count}\nMult: {score.Mult}\nScore: {score.FinalScore}\nCredits: {run.Credits}\nActions: {level.ActionPoints}\nDiscards: {level.DiscardsRemaining}\nDominos places: {level.Grid.GetPlacedDominoes().Count}/{level.MaxPlacedDominoes}";
            RenderActiveDomiNex(run);
            var patterns = score.DetectedPatterns.Count == 0 ? "Aucun pattern" : string.Join(", ", score.DetectedPatterns);
            breakdown.text = $"Patterns: {patterns}\n\n{string.Join("\n", score.BreakdownLines.Take(8))}";
        }

        private void RenderActiveDomiNex(RunState run)
        {
            foreach (Transform child in dominexRoot)
            {
                Destroy(child.gameObject);
            }

            if (run.DomiNexInventory.Active.Count == 0)
            {
                var empty = UiFactory.CreateText(dominexRoot, "Empty", "Aucun", 15, TextAnchor.UpperLeft);
                empty.color = new Color(0.6f, 0.65f, 0.72f);
                return;
            }

            foreach (var definition in run.DomiNexInventory.Active)
            {
                CreateActiveDomiNexRow(definition);
            }
        }

        private void CreateActiveDomiNexRow(DomiNexDefinition definition)
        {
            var row = new GameObject($"DomiNex_{definition.Id}", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(DomiNexHoverTarget));
            row.transform.SetParent(dominexRoot, false);
            row.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.16f, 0.95f);
            row.GetComponent<LayoutElement>().preferredHeight = 24f;
            row.GetComponent<DomiNexHoverTarget>().Initialize(definition, ShowHoverCard, HideHoverCard);

            var text = UiFactory.CreateText(row.transform, "Label", $"[{definition.Rarity}] {definition.Name}", 14, TextAnchor.MiddleLeft);
            text.color = DomiNexUiStyles.GetRarityColor(definition.Rarity);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(6f, 0f);
            text.rectTransform.offsetMax = new Vector2(-6f, 0f);
        }

        private void ShowHoverCard(DomiNexDefinition definition)
        {
            hoverCard.gameObject.SetActive(true);
            hoverCard.Render(definition);
        }

        private void HideHoverCard()
        {
            hoverCard.gameObject.SetActive(false);
        }
    }
}
