using DomiNox.Dominex;
using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class DomiNexBarView : MonoBehaviour
    {
        private Transform slotsRoot;
        private DomiNexDetailCardView hoverCard;
        private RectTransform hoverRect;
        private Canvas canvas;

        public void Initialize()
        {
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.08f, 0.14f, 0.88f);
            var outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.18f, 0.32f);
            outline.effectDistance = new Vector2(2f, -2f);

            canvas = GetComponentInParent<Canvas>();

            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var title = UiFactory.CreateText(transform, "Title", "DomiNex", 14, TextAnchor.MiddleCenter);
            title.color = new Color(0.98f, 0.84f, 0.34f);
            title.GetComponent<LayoutElement>().preferredWidth = 72f;

            slotsRoot = new GameObject("Slots", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            slotsRoot.SetParent(transform, false);
            slotsRoot.GetComponent<LayoutElement>().preferredWidth = 430f;
            var slotsLayout = slotsRoot.GetComponent<HorizontalLayoutGroup>();
            slotsLayout.spacing = 6f;
            slotsLayout.childAlignment = TextAnchor.MiddleLeft;
            slotsLayout.childForceExpandWidth = false;
            slotsLayout.childForceExpandHeight = false;

            hoverCard = new GameObject("DomiNexHoverOverlay", typeof(RectTransform), typeof(CanvasGroup)).AddComponent<DomiNexDetailCardView>();
            hoverCard.transform.SetParent(canvas.transform, false);
            hoverRect = (RectTransform)hoverCard.transform;
            hoverRect.sizeDelta = new Vector2(260f, 118f);
            hoverCard.GetComponent<CanvasGroup>().blocksRaycasts = false;
            hoverCard.Initialize("Survole un DomiNex", true);
            hoverCard.gameObject.SetActive(false);
        }

        public void Render(RunState run)
        {
            foreach (Transform child in slotsRoot)
            {
                Destroy(child.gameObject);
            }

            if (run.DomiNexInventory.Active.Count == 0)
            {
                CreateEmptySlot();
                hoverCard.gameObject.SetActive(false);
                return;
            }

            foreach (var definition in run.DomiNexInventory.Active)
            {
                CreateDomiNexCard(definition);
            }
        }

        private void CreateEmptySlot()
        {
            var slot = new GameObject("EmptyDomiNex", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            slot.transform.SetParent(slotsRoot, false);
            slot.GetComponent<Image>().color = new Color(0.09f, 0.11f, 0.16f, 0.95f);
            var layout = slot.GetComponent<LayoutElement>();
            layout.preferredWidth = 170f;
            layout.preferredHeight = 44f;

            var text = UiFactory.CreateText(slot.transform, "Label", "Aucun DomiNex actif", 13, TextAnchor.MiddleCenter);
            text.color = new Color(0.55f, 0.62f, 0.72f);
            Stretch(text.rectTransform);
        }

        private void CreateDomiNexCard(DomiNexDefinition definition)
        {
            var card = new GameObject($"DomiNex_{definition.Id}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement), typeof(DomiNexHoverTarget));
            card.transform.SetParent(slotsRoot, false);
            card.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.18f, 0.98f);
            var rarityColor = DomiNexUiStyles.GetRarityColor(definition.Rarity);
            var outline = card.GetComponent<Outline>();
            outline.effectColor = rarityColor;
            outline.effectDistance = new Vector2(2f, -2f);
            var layout = card.GetComponent<LayoutElement>();
            layout.preferredWidth = 48f;
            layout.preferredHeight = 48f;
            card.GetComponent<DomiNexHoverTarget>().Initialize(definition, ShowHoverCard, HideHoverCard);

            var icon = UiFactory.CreateText(card.transform, "Icon", GetIcon(definition), 17, TextAnchor.MiddleCenter);
            icon.color = rarityColor;
            icon.rectTransform.anchorMin = new Vector2(0f, 0.26f);
            icon.rectTransform.anchorMax = Vector2.one;
            icon.rectTransform.offsetMin = Vector2.zero;
            icon.rectTransform.offsetMax = Vector2.zero;

            var initials = UiFactory.CreateText(card.transform, "Initials", GetInitials(definition.Name), 9, TextAnchor.MiddleCenter);
            initials.color = Color.white;
            initials.rectTransform.anchorMin = Vector2.zero;
            initials.rectTransform.anchorMax = new Vector2(1f, 0.3f);
            initials.rectTransform.offsetMin = new Vector2(3f, 2f);
            initials.rectTransform.offsetMax = new Vector2(-3f, 0f);
        }

        private void ShowHoverCard(DomiNexDefinition definition, RectTransform source)
        {
            hoverCard.gameObject.SetActive(true);
            hoverCard.Render(definition);
            PositionHoverCard(source);
        }

        private void HideHoverCard()
        {
            hoverCard.gameObject.SetActive(false);
        }

        private void PositionHoverCard(RectTransform source)
        {
            var sourceCenter = source.TransformPoint(new Vector3(source.rect.center.x, source.rect.yMin, 0f));
            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, sourceCenter);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPoint, canvas.worldCamera, out var localPoint);
            hoverRect.anchoredPosition = localPoint + new Vector2(0f, -70f);
        }

        private static string GetIcon(DomiNexDefinition definition)
        {
            if (HasTag(definition, "mult"))
            {
                return "xM";
            }

            if (HasTag(definition, "count"))
            {
                return "+C";
            }

            if (HasTag(definition, "credits"))
            {
                return "$$";
            }

            if (HasTag(definition, "discard"))
            {
                return "DF";
            }

            if (HasTag(definition, "seven"))
            {
                return "7";
            }

            if (HasTag(definition, "limit"))
            {
                return "+1";
            }

            return "DX";
        }

        private static bool HasTag(DomiNexDefinition definition, string tag)
        {
            foreach (var current in definition.Tags)
            {
                if (current == tag)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetInitials(string name)
        {
            var parts = name.Split(' ');
            if (parts.Length == 1)
            {
                return parts[0].Length <= 2 ? parts[0].ToUpperInvariant() : parts[0].Substring(0, 2).ToUpperInvariant();
            }

            return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
