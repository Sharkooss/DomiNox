using System.Linq;
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
        private Text dominex;
        private Text breakdown;

        public void Initialize()
        {
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            stats = UiFactory.CreateText(transform, "Stats", string.Empty, 20, TextAnchor.UpperLeft);
            dominex = UiFactory.CreateText(transform, "DomiNex", string.Empty, 15, TextAnchor.UpperLeft);
            breakdown = UiFactory.CreateText(transform, "Breakdown", string.Empty, 16, TextAnchor.UpperLeft);
        }

        public void Render(RunState run, ScoreResult score)
        {
            var level = run.CurrentLevel;
            stats.text = $"Phase: {run.Phase}\nFloor {level.FloorIndex}\nLevel {level.LevelIndex}\nQuota: {level.Quota}\nCount: {score.Count}\nMult: {score.Mult}\nScore: {score.FinalScore}\nCredits: {run.Credits}\nActions: {level.ActionPoints}\nDiscards: {level.DiscardsRemaining}\nDominos places: {level.Grid.GetPlacedDominoes().Count}/{level.MaxPlacedDominoes}";
            dominex.text = $"DomiNex actifs:\n{string.Join("\n", run.DomiNexInventory.Active.Select(definition => $"- {definition.Name}"))}";
            var patterns = score.DetectedPatterns.Count == 0 ? "Aucun pattern" : string.Join(", ", score.DetectedPatterns);
            breakdown.text = $"Patterns: {patterns}\n\n{string.Join("\n", score.BreakdownLines.Take(8))}";
        }
    }
}
