using System.Collections.Generic;
using DomiNox.Consumables;
using DomiNox.Dominex;
using DomiNox.Patterns;
using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class DomiNexBarView : MonoBehaviour
    {
        private Transform slotsRoot;
        private Transform consumablesRoot;
        private Text title;
        private Text consumablesTitle;
        private Button useButton;
        private DomiNexDetailCardView hoverCard;
        private RectTransform hoverRect;
        private Canvas canvas;
        private RunState currentRun;
        private GameFlowController controller;
        private readonly Dictionary<string, RectTransform> activeCards = new Dictionary<string, RectTransform>();

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.08f, 0.14f, 0.88f);
            var outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.18f, 0.32f);
            outline.effectDistance = new Vector2(2f, -2f);

            canvas = GetComponentInParent<Canvas>();

            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            title = UiFactory.CreateText(transform, "Title", "DomiNex", 14, TextAnchor.MiddleCenter);
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

            var separator = UiFactory.CreateText(transform, "Separator", "|", 22, TextAnchor.MiddleCenter);
            separator.color = new Color(0.25f, 0.32f, 0.42f);
            separator.GetComponent<LayoutElement>().preferredWidth = 12f;

            var consumablesPanel = new GameObject("ConsumablesPanel", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            consumablesPanel.transform.SetParent(transform, false);
            consumablesPanel.GetComponent<LayoutElement>().preferredWidth = 230f;
            var consumablesLayout = consumablesPanel.GetComponent<VerticalLayoutGroup>();
            consumablesLayout.spacing = 3f;
            consumablesLayout.childAlignment = TextAnchor.MiddleCenter;

            consumablesTitle = UiFactory.CreateText(consumablesPanel.transform, "ConsumablesTitle", "Consumables", 12, TextAnchor.MiddleCenter);
            consumablesTitle.color = new Color(0.76f, 0.9f, 1f);
            consumablesTitle.GetComponent<LayoutElement>().preferredHeight = 16f;
            consumablesRoot = new GameObject("ConsumableSlots", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            consumablesRoot.SetParent(consumablesPanel.transform, false);
            consumablesRoot.GetComponent<LayoutElement>().preferredHeight = 34f;
            var consumableSlotsLayout = consumablesRoot.GetComponent<HorizontalLayoutGroup>();
            consumableSlotsLayout.spacing = 6f;
            consumableSlotsLayout.childAlignment = TextAnchor.MiddleCenter;
            consumableSlotsLayout.childForceExpandWidth = false;

            useButton = UiFactory.CreateButton(consumablesPanel.transform, "UseConsumable", "Use");
            useButton.GetComponent<LayoutElement>().preferredWidth = 82f;
            useButton.GetComponent<LayoutElement>().preferredHeight = 24f;

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

            foreach (Transform child in consumablesRoot)
            {
                Destroy(child.gameObject);
            }

            activeCards.Clear();

            title.text = $"DomiNex\n{run.ActiveDomiNexCount}/{run.MaxDomiNexSlots}";
            currentRun = run;

            foreach (var definition in run.DomiNexInventory.Active)
            {
                CreateDomiNexCard(definition);
            }

            for (var i = run.DomiNexInventory.Active.Count; i < run.MaxDomiNexSlots; i++)
            {
                CreateEmptySlot(i);
            }

            if (run.DomiNexInventory.Active.Count == 0)
            {
                hoverCard.gameObject.SetActive(false);
            }

            consumablesTitle.text = $"Consumables {run.ActiveConsumableCount}/{DomiNox.Core.GameConstants.MaxConsumableSlots}";
            for (var i = 0; i < DomiNox.Core.GameConstants.MaxConsumableSlots; i++)
            {
                CreateConsumableSlot(i, run);
            }

            RenderUseButton(run);
        }

        private void CreateConsumableSlot(int index, RunState run)
        {
            var id = index < run.Consumables.ActiveIds.Count ? run.Consumables.ActiveIds[index] : null;
            var definition = GemTileRegistry.GetById(id);
            var slot = new GameObject($"Consumable_{index}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement), typeof(Button));
            slot.transform.SetParent(consumablesRoot, false);
            slot.GetComponent<Image>().color = definition == null ? new Color(0.09f, 0.11f, 0.16f, 0.95f) : new Color(0.09f, 0.18f, 0.2f, 0.98f);
            var outline = slot.GetComponent<Outline>();
            outline.effectColor = controllerSelected(index) ? new Color(1f, 0.84f, 0.25f) : new Color(0.22f, 0.42f, 0.48f);
            outline.effectDistance = new Vector2(2f, -2f);
            slot.GetComponent<LayoutElement>().preferredWidth = 48f;
            slot.GetComponent<LayoutElement>().preferredHeight = 32f;
            var button = slot.GetComponent<Button>();
            button.interactable = definition != null;
            var capturedIndex = index;
            button.onClick.AddListener(() => controller.SelectConsumable(capturedIndex));
            var label = UiFactory.CreateText(slot.transform, "Label", definition == null ? "+" : definition.IconId, 12, TextAnchor.MiddleCenter);
            label.color = definition == null ? new Color(0.35f, 0.42f, 0.52f) : new Color(0.76f, 1f, 0.94f);
            Stretch(label.rectTransform);

            bool controllerSelected(int slotIndex) => controller?.SelectedConsumableIndex == slotIndex;
        }

        private void RenderUseButton(RunState run)
        {
            var selected = controller == null ? -1 : controller.SelectedConsumableIndex;
            useButton.gameObject.SetActive(selected >= 0 && selected < run.Consumables.Count);
            useButton.onClick.RemoveAllListeners();
            if (!useButton.gameObject.activeSelf)
            {
                return;
            }

            var definition = GemTileRegistry.GetById(run.Consumables.ActiveIds[selected]);
            var canUse = GemTileRegistry.CanUseConsumable(definition, run);
            useButton.GetComponentInChildren<Text>().text = canUse ? "Use" : "Max Level";
            useButton.interactable = canUse;
            useButton.onClick.AddListener(controller.UseSelectedConsumable);
        }

        private void CreateEmptySlot(int index)
        {
            var slot = new GameObject($"EmptyDomiNex_{index}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
            slot.transform.SetParent(slotsRoot, false);
            slot.GetComponent<Image>().color = new Color(0.09f, 0.11f, 0.16f, 0.95f);
            var outline = slot.GetComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.23f, 0.3f);
            outline.effectDistance = new Vector2(1f, -1f);
            var layout = slot.GetComponent<LayoutElement>();
            layout.preferredWidth = 48f;
            layout.preferredHeight = 48f;

            var text = UiFactory.CreateText(slot.transform, "Label", "+", 16, TextAnchor.MiddleCenter);
            text.color = new Color(0.35f, 0.42f, 0.52f);
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
            activeCards[definition.Id] = (RectTransform)card.transform;

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
            hoverCard.Render(definition, currentRun);
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

        public RectTransform GetDomiNexRect(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && activeCards.TryGetValue(id, out var rect) ? rect : null;
        }
    }
}
