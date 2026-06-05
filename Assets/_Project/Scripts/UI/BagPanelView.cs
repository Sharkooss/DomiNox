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
            background.color = new Color(0.08f, 0.1f, 0.13f, 0.96f);
            button.targetGraphic = background;
            var outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.28f, 0.38f);
            outline.effectDistance = new Vector2(2f, -2f);
            button.onClick.AddListener(ToggleOverlay);

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var title = UiFactory.CreateText(transform, "Title", "Sac", 16, TextAnchor.MiddleCenter);
            title.color = new Color(0.98f, 0.84f, 0.34f);
            var icon = UiFactory.CreateText(transform, "Icon", "[::]", 22, TextAnchor.MiddleCenter);
            icon.color = new Color(0.68f, 0.76f, 0.86f);
            countText = UiFactory.CreateText(transform, "Count", string.Empty, 14, TextAnchor.MiddleCenter);

            BuildOverlay();
        }

        public void Render(RunState run)
        {
            countText.text = $"{run.Bag.RemainingCount()} / 28";
            if (overlay.activeSelf)
            {
                RenderOverlay(run);
            }
        }

        private void ToggleOverlay()
        {
            overlay.SetActive(!overlay.activeSelf);
            if (overlay.activeSelf)
            {
                RenderOverlay(controller.Run);
            }
        }

        private void BuildOverlay()
        {
            overlay = new GameObject("BagOverlay", typeof(RectTransform), typeof(Image), typeof(Outline));
            overlay.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)overlay.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(860f, 520f);
            overlay.GetComponent<Image>().color = new Color(0.07f, 0.1f, 0.12f, 0.98f);
            overlay.GetComponent<Outline>().effectColor = new Color(0.35f, 0.5f, 0.62f);
            overlay.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);

            var layout = overlay.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 14);
            layout.spacing = 10f;

            var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(overlay.transform, false);
            header.GetComponent<LayoutElement>().preferredHeight = 42f;
            var headerLayout = header.GetComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleCenter;

            var title = UiFactory.CreateText(header.transform, "Title", "Reserve de dominos", 20, TextAnchor.MiddleLeft);
            title.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var close = UiFactory.CreateButton(header.transform, "Close", "Fermer");
            close.GetComponent<LayoutElement>().preferredWidth = 110f;
            close.onClick.AddListener(() => overlay.SetActive(false));

            listRoot = new GameObject("DominoList", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement)).transform;
            listRoot.SetParent(overlay.transform, false);
            listRoot.GetComponent<LayoutElement>().flexibleHeight = 1f;
            var grid = listRoot.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 7;
            grid.cellSize = new Vector2(106f, 72f);
            grid.spacing = new Vector2(8f, 8f);
            overlay.SetActive(false);
        }

        private void RenderOverlay(RunState run)
        {
            foreach (Transform child in listRoot)
            {
                Destroy(child.gameObject);
            }

            var remainingIds = new HashSet<string>(run.Bag.RemainingDominoes.Select(domino => domino.InstanceId));
            var discardedIds = new HashSet<string>(run.Bag.DiscardedDominoes.Select(domino => domino.InstanceId));
            var handIds = new HashSet<string>(run.CurrentLevel.Hand.Dominoes.Select(domino => domino.InstanceId));
            var placedIds = new HashSet<string>(run.CurrentLevel.Grid.GetPlacedDominoes().Select(placed => placed.Domino.InstanceId));

            foreach (var domino in DominoFactory.CreateDoubleSixSet())
            {
                var status = GetStatus(domino, remainingIds, handIds, placedIds, discardedIds);
                CreateDominoStatus(domino, status, status == "Bag");
            }
        }

        private static string GetStatus(DominoInstance domino, HashSet<string> remainingIds, HashSet<string> handIds, HashSet<string> placedIds, HashSet<string> discardedIds)
        {
            if (remainingIds.Contains(domino.InstanceId))
            {
                return "Bag";
            }

            if (handIds.Contains(domino.InstanceId))
            {
                return "Main";
            }

            if (placedIds.Contains(domino.InstanceId))
            {
                return "Pose";
            }

            if (discardedIds.Contains(domino.InstanceId))
            {
                return "Defausse";
            }

            return "Sorti";
        }

        private void CreateDominoStatus(DominoInstance domino, string status, bool available)
        {
            var card = new GameObject($"Domino_{domino.InstanceId}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            card.transform.SetParent(listRoot, false);
            card.GetComponent<Image>().color = available ? Color.white : new Color(0.28f, 0.3f, 0.34f, 0.88f);

            var value = UiFactory.CreateText(card.transform, "Value", domino.ToString(), 18, TextAnchor.MiddleCenter);
            value.color = available ? Color.black : new Color(0.62f, 0.66f, 0.72f);
            value.rectTransform.anchorMin = new Vector2(0f, 0.28f);
            value.rectTransform.anchorMax = Vector2.one;
            value.rectTransform.offsetMin = Vector2.zero;
            value.rectTransform.offsetMax = Vector2.zero;

            var label = UiFactory.CreateText(card.transform, "Status", status, 11, TextAnchor.MiddleCenter);
            label.color = available ? new Color(0.12f, 0.22f, 0.34f) : new Color(0.72f, 0.76f, 0.82f);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = new Vector2(1f, 0.3f);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }
    }
}
