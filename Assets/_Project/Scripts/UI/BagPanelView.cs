using System.Collections.Generic;
using System.Linq;
using DomiNox.Dominoes;
using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class BagPanelView : MonoBehaviour
    {
        private GameFlowController controller;
        private Canvas canvas;
        private Text countText;
        private GameObject overlay;
        private Transform listRoot;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            canvas = GetComponentInParent<Canvas>();

            var button = gameObject.AddComponent<Button>();
            var background = gameObject.AddComponent<Image>();
            background.color = DomiNoxTheme.BgCard;
            button.targetGraphic = background;
            DomiNoxTheme.AddOutline(gameObject, DomiNoxTheme.BorderNormal);
            button.onClick.AddListener(ToggleOverlay);

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 5, 5);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var titleText = UiFactory.CreateText(transform, "Title", "BAG", DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            titleText.color = DomiNoxTheme.Gold;
            titleText.fontStyle = FontStyle.Bold;
            var icon = UiFactory.CreateText(transform, "Icon", "[ ]", DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            icon.color = DomiNoxTheme.TextMuted;
            countText = UiFactory.CreateText(transform, "Count", string.Empty, DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            countText.color = DomiNoxTheme.TextSecondary;

            BuildOverlay();
        }

        public void Render(RunState run)
        {
            countText.text = $"{run.Bag.RemainingCount()} / {run.TotalDominoCount}";
            if (overlay.activeSelf) RenderOverlay(run);
        }

        private void ToggleOverlay()
        {
            overlay.SetActive(!overlay.activeSelf);
            if (overlay.activeSelf) RenderOverlay(controller.Run);
        }

        private void BuildOverlay()
        {
            overlay = new GameObject("BagOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)overlay.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(880f, 540f);
            overlay.GetComponent<Image>().color = DomiNoxTheme.BgOverlay;
            DomiNoxTheme.AddOutline(overlay, DomiNoxTheme.CountBlueDim, 3f);
            DomiNoxTheme.AddShadow(overlay, new Color(0f, 0f, 0f, 0.5f), new Vector2(4f, -4f));

            var layout = overlay.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 14, 16);
            layout.spacing = 12f;

            var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(overlay.transform, false);
            header.GetComponent<LayoutElement>().preferredHeight = 44f;
            var headerLayout = header.GetComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleCenter;

            var titleText = UiFactory.CreateText(header.transform, "Title", "Domino Reserve", DomiNoxTheme.FontLG, TextAnchor.MiddleLeft);
            titleText.color = DomiNoxTheme.Gold;
            titleText.fontStyle = FontStyle.Bold;
            titleText.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var close = UiFactory.CreateButton(header.transform, "Close", "Close");
            close.GetComponent<LayoutElement>().preferredWidth = 100f;
            UiFactory.StyleButton(close, DomiNoxTheme.BgCard, DomiNoxTheme.TextSecondary);
            close.onClick.AddListener(() => overlay.SetActive(false));

            UiFactory.CreateSeparator(overlay.transform, 1f, DomiNoxTheme.BorderNormal);

            listRoot = new GameObject("DominoList", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement)).transform;
            listRoot.SetParent(overlay.transform, false);
            listRoot.GetComponent<LayoutElement>().flexibleHeight = 1f;
            var grid = listRoot.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 7;
            grid.cellSize = new Vector2(108f, 74f);
            grid.spacing = new Vector2(8f, 8f);
            overlay.SetActive(false);
        }

        private void RenderOverlay(RunState run)
        {
            foreach (Transform child in listRoot) Destroy(child.gameObject);
            var remainingIds  = new HashSet<string>(run.Bag.RemainingDominoes.Select(d => d.InstanceId));
            var discardedIds  = new HashSet<string>(run.Bag.DiscardedDominoes.Concat(run.CurrentLevel.DiscardedThisLevel).Select(d => d.InstanceId));
            var handIds       = new HashSet<string>(run.CurrentLevel.Hand.Dominoes.Select(d => d.InstanceId));
            var placedIds     = new HashSet<string>(run.CurrentLevel.Grid.GetPlacedDominoes().Select(p => p.Domino.InstanceId));
            var playedIds     = new HashSet<string>(run.CurrentLevel.PlayedThisLevel.Select(d => d.InstanceId));

            foreach (var domino in run.Bag.RemainingDominoes
                .Concat(run.CurrentLevel.Hand.Dominoes)
                .Concat(run.CurrentLevel.Grid.GetPlacedDominoes().Select(p => p.Domino))
                .Concat(run.CurrentLevel.PlayedThisLevel)
                .Concat(run.CurrentLevel.DiscardedThisLevel)
                .Concat(run.Bag.DiscardedDominoes)
                .GroupBy(d => d.InstanceId).Select(g => g.First())
                .OrderBy(d => d.Definition.Left).ThenBy(d => d.Definition.Right).ThenBy(d => d.InstanceId))
            {
                var status = GetStatus(domino, remainingIds, handIds, placedIds, discardedIds, playedIds);
                CreateDominoStatus(domino, status, status == "Bag");
            }
        }

        private static string GetStatus(DominoInstance domino, HashSet<string> remaining, HashSet<string> hand, HashSet<string> placed, HashSet<string> discarded, HashSet<string> played)
        {
            if (remaining.Contains(domino.InstanceId))  return "Bag";
            if (hand.Contains(domino.InstanceId))       return "Hand";
            if (placed.Contains(domino.InstanceId))     return "Grid";
            if (played.Contains(domino.InstanceId))     return "Played";
            if (discarded.Contains(domino.InstanceId))  return "Discarded";
            return "Out";
        }

        private void CreateDominoStatus(DominoInstance domino, string status, bool available)
        {
            var card = new GameObject($"Domino_{domino.InstanceId}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            card.transform.SetParent(listRoot, false);
            var modifier = DominoModifierRegistry.GetById(domino.ModifierId);
            var baseColor = available ? DomiNoxTheme.IvoryWhite : new Color(0.18f, 0.20f, 0.24f, 0.88f);
            card.GetComponent<Image>().color = modifier != null
                ? Color.Lerp(baseColor, modifier.ColorTheme, 0.22f)
                : baseColor;
            if (available) DomiNoxTheme.AddShadow(card, new Color(0f, 0f, 0f, 0.3f), new Vector2(1f, -1f));

            var value = UiFactory.CreateText(card.transform, "Value", modifier == null ? domino.ToString() : $"{domino} {modifier.Badge}", DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            value.color = available ? DomiNoxTheme.IvoryDark : DomiNoxTheme.TextMuted;
            value.fontStyle = FontStyle.Bold;
            value.raycastTarget = false;
            value.rectTransform.anchorMin = new Vector2(0f, 0.28f);
            value.rectTransform.anchorMax = Vector2.one;
            value.rectTransform.offsetMin = Vector2.zero;
            value.rectTransform.offsetMax = Vector2.zero;

            var statusColor = available ? new Color(0.10f, 0.22f, 0.36f) : DomiNoxTheme.TextMuted;
            var label = UiFactory.CreateText(card.transform, "Status", status, DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            label.color = statusColor;
            label.raycastTarget = false;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = new Vector2(1f, 0.30f);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }
    }
}
