using System.Collections.Generic;
using DomiNox.Consumables;
using DomiNox.Dominex;
using DomiNox.Patterns;
using DomiNox.Run;
using DomiNox.Shop;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private Transform detailRoot;
        private RectTransform detailRect;
        private Canvas canvas;
        private RunState currentRun;
        private GameFlowController controller;
        private readonly Dictionary<string, RectTransform> activeCards = new Dictionary<string, RectTransform>();
        private readonly Dictionary<int, RectTransform> slotRects = new Dictionary<int, RectTransform>();

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            var background = gameObject.AddComponent<Image>();
            background.color = DomiNoxTheme.BgDeep;
            DomiNoxTheme.AddOutline(gameObject, DomiNoxTheme.BorderNormal);

            canvas = GetComponentInParent<Canvas>();

            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 6, 6);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            title = UiFactory.CreateText(transform, "Title", "DomiNex", DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            title.color = DomiNoxTheme.Gold;
            title.fontStyle = FontStyle.Bold;
            title.GetComponent<LayoutElement>().preferredWidth = 70f;

            slotsRoot = new GameObject("Slots", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            slotsRoot.SetParent(transform, false);
            slotsRoot.GetComponent<LayoutElement>().preferredWidth = 430f;
            var slotsLayout = slotsRoot.GetComponent<HorizontalLayoutGroup>();
            slotsLayout.spacing = 6f;
            slotsLayout.childAlignment = TextAnchor.MiddleLeft;
            slotsLayout.childForceExpandWidth = false;
            slotsLayout.childForceExpandHeight = false;

            var separator = UiFactory.CreateText(transform, "Sep", "|", DomiNoxTheme.FontLG, TextAnchor.MiddleCenter);
            separator.color = DomiNoxTheme.TextMuted;
            separator.GetComponent<LayoutElement>().preferredWidth = 12f;

            var consumablesPanel = new GameObject("ConsumablesPanel", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            consumablesPanel.transform.SetParent(transform, false);
            consumablesPanel.GetComponent<LayoutElement>().preferredWidth = 230f;
            var consumablesLayout = consumablesPanel.GetComponent<VerticalLayoutGroup>();
            consumablesLayout.spacing = 3f;
            consumablesLayout.childAlignment = TextAnchor.MiddleCenter;

            consumablesTitle = UiFactory.CreateText(consumablesPanel.transform, "ConsumablesTitle", "Consumables", DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            consumablesTitle.color = DomiNoxTheme.JackpotCyan;
            consumablesTitle.GetComponent<LayoutElement>().preferredHeight = 16f;

            consumablesRoot = new GameObject("ConsumableSlots", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            consumablesRoot.SetParent(consumablesPanel.transform, false);
            consumablesRoot.GetComponent<LayoutElement>().preferredHeight = 36f;
            var consumableSlotsLayout = consumablesRoot.GetComponent<HorizontalLayoutGroup>();
            consumableSlotsLayout.spacing = 6f;
            consumableSlotsLayout.childAlignment = TextAnchor.MiddleCenter;
            consumableSlotsLayout.childForceExpandWidth = false;

            useButton = UiFactory.CreateButton(consumablesPanel.transform, "UseConsumable", "Use");
            useButton.GetComponent<LayoutElement>().preferredWidth = 82f;
            useButton.GetComponent<LayoutElement>().preferredHeight = 22f;
            UiFactory.StyleButton(useButton, DomiNoxTheme.BgCard, DomiNoxTheme.JackpotCyan, 22f);

            hoverCard = new GameObject("DomiNexHoverOverlay", typeof(RectTransform), typeof(CanvasGroup)).AddComponent<DomiNexDetailCardView>();
            hoverCard.transform.SetParent(canvas.transform, false);
            hoverRect = (RectTransform)hoverCard.transform;
            hoverRect.sizeDelta = new Vector2(280f, 130f);
            hoverCard.GetComponent<CanvasGroup>().blocksRaycasts = false;
            hoverCard.Initialize("Hover a DomiNex", true);
            hoverCard.gameObject.SetActive(false);

            detailRoot = new GameObject("SelectedItemDetail", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image), typeof(CanvasGroup)).transform;
            detailRoot.SetParent(canvas.transform, false);
            detailRect = (RectTransform)detailRoot;
            detailRect.sizeDelta = new Vector2(320f, 230f);
            detailRoot.GetComponent<Image>().color = DomiNoxTheme.BgOverlay;
            DomiNoxTheme.AddOutline(detailRoot.gameObject, DomiNoxTheme.BorderNormal, 3f);
            var detailLayout = detailRoot.GetComponent<VerticalLayoutGroup>();
            detailLayout.padding = new RectOffset(14, 14, 12, 12);
            detailLayout.spacing = 7f;
            detailLayout.childAlignment = TextAnchor.UpperCenter;
            detailRoot.gameObject.SetActive(false);
        }

        public void Render(RunState run)
        {
            foreach (Transform child in slotsRoot) Destroy(child.gameObject);
            foreach (Transform child in consumablesRoot) Destroy(child.gameObject);
            activeCards.Clear();
            slotRects.Clear();
            title.text = $"DomiNex\n{run.ActiveDomiNexCount}/{run.MaxDomiNexSlots}";
            currentRun = run;

            for (var i = 0; i < run.MaxDomiNexSlots; i++)
            {
                var instance = run.DomiNexInventory.GetInstanceAt(i);
                if (instance == null) CreateEmptySlot(i);
                else CreateDomiNexCard(i, instance);
            }

            if (run.DomiNexInventory.Active.Count == 0) hoverCard.gameObject.SetActive(false);

            consumablesTitle.text = $"Consumables {run.ActiveConsumableCount}/{run.MaxConsumableSlots}";
            for (var i = 0; i < run.MaxConsumableSlots; i++) CreateConsumableSlot(i, run);

            RenderUseButton(run);
            RenderSelectedDetail(run);
        }

        private void CreateConsumableSlot(int index, RunState run)
        {
            var id = index < run.Consumables.ActiveIds.Count ? run.Consumables.ActiveIds[index] : null;
            var definition = GemTileRegistry.GetById(id);
            var slot = new GameObject($"Consumable_{index}", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Button));
            slot.transform.SetParent(consumablesRoot, false);
            var isSelected = controller?.SelectedConsumableIndex == index;
            slot.GetComponent<Image>().color = definition == null
                ? DomiNoxTheme.BgPanel
                : new Color(0.04f, 0.12f, 0.14f, 0.98f);
            DomiNoxTheme.AddOutline(slot, isSelected ? DomiNoxTheme.Gold : (definition != null ? DomiNoxTheme.JackpotCyan : DomiNoxTheme.BorderNormal), isSelected ? 3f : 2f);
            slot.GetComponent<LayoutElement>().preferredWidth = 50f;
            slot.GetComponent<LayoutElement>().preferredHeight = 34f;
            var button = slot.GetComponent<Button>();
            button.interactable = definition != null;
            var capturedIndex = index;
            button.onClick.AddListener(() => controller.SelectConsumable(capturedIndex));
            var label = UiFactory.CreateText(slot.transform, "Label", definition == null ? "+" : definition.IconId, DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            label.color = definition == null ? DomiNoxTheme.TextMuted : DomiNoxTheme.JackpotCyan;
            Stretch(label.rectTransform);
        }

        private void RenderUseButton(RunState run)
        {
            useButton.gameObject.SetActive(false);
        }

        private void CreateEmptySlot(int index)
        {
            var slot = new GameObject($"EmptyDomiNex_{index}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            slot.transform.SetParent(slotsRoot, false);
            slot.GetComponent<Image>().color = DomiNoxTheme.BgPanel;
            DomiNoxTheme.AddOutline(slot, DomiNoxTheme.BorderNormal, 1.5f);
            var layout = slot.GetComponent<LayoutElement>();
            layout.preferredWidth = 52f;
            layout.preferredHeight = 52f;
            var text = UiFactory.CreateText(slot.transform, "Label", "+", DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            text.color = DomiNoxTheme.TextMuted;
            Stretch(text.rectTransform);
            slotRects[index] = (RectTransform)slot.transform;
            slot.AddComponent<DomiNexSlotInteraction>().Initialize(controller, index, false);
        }

        private void CreateDomiNexCard(int index, ActiveDomiNexInstance instance)
        {
            var definition = instance.Definition;
            var card = new GameObject($"DomiNex_{definition.Id}", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(DomiNexHoverTarget));
            card.transform.SetParent(slotsRoot, false);
            var rarityColor = DomiNoxTheme.GetRarityColor(definition.Rarity);
            card.GetComponent<Image>().color = Color.Lerp(DomiNoxTheme.BgCard, rarityColor, 0.10f);
            var isSelected = controller.SelectedDomiNexIndex == index;
            DomiNoxTheme.AddOutline(card, isSelected ? DomiNoxTheme.Gold : rarityColor, isSelected ? 4f : 2f);
            if (isSelected) DomiNoxTheme.AddShadow(card, DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.28f), new Vector2(0f, -2f));
            var layout = card.GetComponent<LayoutElement>();
            layout.preferredWidth = 52f;
            layout.preferredHeight = 52f;
            card.GetComponent<DomiNexHoverTarget>().Initialize(definition, ShowHoverCard, HideHoverCard);
            activeCards[definition.Id] = (RectTransform)card.transform;
            slotRects[index] = (RectTransform)card.transform;
            card.AddComponent<DomiNexSlotInteraction>().Initialize(controller, index, true);

            var icon = UiFactory.CreateText(card.transform, "Icon", GetIcon(definition), DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            icon.color = rarityColor;
            icon.fontStyle = FontStyle.Bold;
            icon.rectTransform.anchorMin = new Vector2(0f, 0.28f);
            icon.rectTransform.anchorMax = Vector2.one;
            icon.rectTransform.offsetMin = Vector2.zero;
            icon.rectTransform.offsetMax = Vector2.zero;

            var initials = UiFactory.CreateText(card.transform, "Initials", GetInitials(definition.Name), DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            initials.color = DomiNoxTheme.TextSecondary;
            initials.rectTransform.anchorMin = Vector2.zero;
            initials.rectTransform.anchorMax = new Vector2(1f, 0.32f);
            initials.rectTransform.offsetMin = new Vector2(3f, 2f);
            initials.rectTransform.offsetMax = new Vector2(-3f, 0f);
        }

        private void ShowHoverCard(DomiNexDefinition definition, RectTransform source)
        {
            hoverCard.gameObject.SetActive(true);
            hoverCard.Render(definition, currentRun);
            PositionHoverCard(source);
        }

        private void HideHoverCard() => hoverCard.gameObject.SetActive(false);

        private void RenderSelectedDetail(RunState run)
        {
            Clear(detailRoot);
            var selectedDomiNex  = controller.SelectedDomiNexIndex;
            var selectedConsumable = controller.SelectedConsumableIndex;
            if (run.DomiNexInventory.GetInstanceAt(selectedDomiNex) != null)
            {
                RenderDomiNexDetail(run, selectedDomiNex);
                return;
            }
            if (selectedConsumable >= 0 && selectedConsumable < run.Consumables.Count)
            {
                RenderConsumableDetail(run, selectedConsumable);
                return;
            }
            detailRoot.gameObject.SetActive(false);
        }

        private void RenderDomiNexDetail(RunState run, int index)
        {
            var instance = run.DomiNexInventory.GetInstanceAt(index);
            if (instance == null) { detailRoot.gameObject.SetActive(false); return; }
            var definition = instance.Definition;
            var rarityColor = DomiNoxTheme.GetRarityColor(definition.Rarity);
            detailRoot.GetComponent<Outline>().effectColor = rarityColor;
            AddText(detailRoot, definition.Name, DomiNoxTheme.FontLG, DomiNoxTheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddText(detailRoot, definition.Rarity.ToString().ToUpperInvariant(), DomiNoxTheme.FontXS, rarityColor, TextAnchor.MiddleCenter);
            var dynamicText = definition.GetCurrentEffectText(run);
            AddText(detailRoot, string.IsNullOrWhiteSpace(dynamicText) ? definition.Description : $"{definition.Description}\n{dynamicText}", DomiNoxTheme.FontSM, DomiNoxTheme.TextSecondary, TextAnchor.UpperLeft, flexible: true);
            if (definition.Tags.Count > 0)
                AddText(detailRoot, string.Join("  ·  ", definition.Tags), DomiNoxTheme.FontXS, DomiNoxTheme.TextMuted, TextAnchor.MiddleCenter);
            UiFactory.CreateSeparator(detailRoot, 1f, DomiNoxTheme.BorderNormal);
            var sellValue = SellValueService.GetDomiNexSellValue(instance);
            var sell = UiFactory.CreateButton(detailRoot, "SellDomiNex", $"Sell ${sellValue}");
            sell.GetComponent<LayoutElement>().preferredHeight = 34f;
            UiFactory.StyleButton(sell, DomiNoxTheme.BgPanel, DomiNoxTheme.Warning, 34f);
            sell.onClick.AddListener(controller.SellSelectedDomiNex);
            detailRoot.gameObject.SetActive(true);
            PositionDetail(slotRects.TryGetValue(index, out var source) ? source : (RectTransform)transform);
        }

        private void RenderConsumableDetail(RunState run, int index)
        {
            var instance = run.Consumables.GetInstanceAt(index);
            var definition = GemTileRegistry.GetById(instance?.DefinitionId);
            if (definition == null) { detailRoot.gameObject.SetActive(false); return; }
            detailRoot.GetComponent<Outline>().effectColor = DomiNoxTheme.JackpotCyan;
            var pattern = PatternCatalog.GetById(definition.TargetPatternId);
            var currentLevel = run.PatternLevels.GetLevel(definition.TargetPatternId);
            var nextLevel = System.Math.Min(currentLevel + 1, DomiNox.Core.GameConstants.MaxPatternLevel);
            AddText(detailRoot, definition.Name, DomiNoxTheme.FontLG, DomiNoxTheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddText(detailRoot, "Gem Tile", DomiNoxTheme.FontXS, DomiNoxTheme.JackpotCyan, TextAnchor.MiddleCenter);
            AddText(detailRoot, $"{definition.Description}\nTarget: {pattern?.Name ?? definition.TargetPatternId}\nLv.{currentLevel} → Lv.{nextLevel}", DomiNoxTheme.FontSM, DomiNoxTheme.TextSecondary, TextAnchor.UpperLeft, flexible: true);
            var buttonRow = new GameObject("Actions", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            buttonRow.SetParent(detailRoot, false);
            buttonRow.GetComponent<LayoutElement>().preferredHeight = 36f;
            var rowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childForceExpandWidth = true;
            var use = UiFactory.CreateButton(buttonRow, "UseConsumable", run.PatternLevels.IsMaxLevel(definition.TargetPatternId) ? "Max Level" : "Use");
            use.interactable = !run.PatternLevels.IsMaxLevel(definition.TargetPatternId);
            UiFactory.StyleButton(use, DomiNoxTheme.BgCard, DomiNoxTheme.Success);
            use.onClick.AddListener(controller.UseSelectedConsumable);
            var sellValue = SellValueService.GetConsumableSellValue(instance);
            var sell = UiFactory.CreateButton(buttonRow, "SellConsumable", $"Sell ${sellValue}");
            UiFactory.StyleButton(sell, DomiNoxTheme.BgCard, DomiNoxTheme.Warning);
            sell.onClick.AddListener(controller.SellSelectedConsumable);
            detailRoot.gameObject.SetActive(true);
            PositionDetail((RectTransform)consumablesRoot);
        }

        private void PositionDetail(RectTransform source)
        {
            var sourceCenter = source.TransformPoint(new Vector3(source.rect.center.x, source.rect.yMin, 0f));
            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, sourceCenter);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPoint, canvas.worldCamera, out var localPoint);
            var canvasRect = ((RectTransform)canvas.transform).rect;
            var x = Mathf.Clamp(localPoint.x, canvasRect.xMin + detailRect.sizeDelta.x * 0.5f + 12f, canvasRect.xMax - detailRect.sizeDelta.x * 0.5f - 12f);
            var y = Mathf.Clamp(localPoint.y - 128f, canvasRect.yMin + detailRect.sizeDelta.y * 0.5f + 12f, canvasRect.yMax - detailRect.sizeDelta.y * 0.5f - 12f);
            detailRect.anchoredPosition = new Vector2(x, y);
        }

        private void PositionHoverCard(RectTransform source)
        {
            var sourceCenter = source.TransformPoint(new Vector3(source.rect.center.x, source.rect.yMin, 0f));
            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, sourceCenter);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPoint, canvas.worldCamera, out var localPoint);
            hoverRect.anchoredPosition = localPoint + new Vector2(0f, -72f);
        }

        private static string GetIcon(DomiNexDefinition definition)
        {
            if (HasTag(definition, "mult"))    return "xM";
            if (HasTag(definition, "count"))   return "+C";
            if (HasTag(definition, "credits")) return "$$";
            if (HasTag(definition, "discard")) return "DF";
            if (HasTag(definition, "seven"))   return "7";
            if (HasTag(definition, "limit"))   return "+1";
            return "DX";
        }

        private static bool HasTag(DomiNexDefinition def, string tag)
        {
            foreach (var t in def.Tags) if (t == tag) return true;
            return false;
        }

        private static string GetInitials(string name)
        {
            var parts = name.Split(' ');
            if (parts.Length == 1) return parts[0].Length <= 2 ? parts[0].ToUpperInvariant() : parts[0].Substring(0, 2).ToUpperInvariant();
            return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
        }

        private static void AddText(Transform parent, string value, int size, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal, bool flexible = false)
        {
            var text = UiFactory.CreateText(parent, "Text", value, size, anchor);
            text.color = color;
            text.fontStyle = style;
            if (flexible) text.GetComponent<LayoutElement>().flexibleHeight = 1f;
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

        public RectTransform GetDomiNexRect(string id) =>
            !string.IsNullOrWhiteSpace(id) && activeCards.TryGetValue(id, out var rect) ? rect : null;
    }

    public sealed class DomiNexSlotInteraction : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        private const float DragThresholdPixels = 6f;
        private static readonly List<DomiNexSlotInteraction> ActiveSlots = new List<DomiNexSlotInteraction>();
        private static int draggedIndex = -1;
        private static bool dragStarted;
        private static bool suppressNextClick;
        private static GameObject dragGhost;
        private GameFlowController controller;
        private int index;
        private bool occupied;
        private Vector2 dragStartPosition;
        private Outline outline;
        private Color normalOutlineColor;
        private Vector2 normalOutlineDistance;

        public void Initialize(GameFlowController flowController, int slotIndex, bool hasDomiNex)
        {
            controller = flowController;
            index = slotIndex;
            occupied = hasDomiNex;
            outline = GetComponent<Outline>();
            if (outline != null) { normalOutlineColor = outline.effectColor; normalOutlineDistance = outline.effectDistance; }
        }

        private void OnEnable() { if (!ActiveSlots.Contains(this)) ActiveSlots.Add(this); }
        private void OnDisable() { ActiveSlots.Remove(this); ClearAllDomiNexDragVisualStates(); }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (suppressNextClick) { suppressNextClick = false; return; }
            controller.SelectDomiNex(occupied ? index : -1);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragStartPosition = eventData.position;
            draggedIndex = occupied ? index : -1;
            dragStarted = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (draggedIndex < 0) return;
            if (!dragStarted && Vector2.Distance(dragStartPosition, eventData.position) < DragThresholdPixels) return;
            if (!dragStarted) { dragStarted = true; CreateDragGhost(eventData); }
            if (dragGhost != null) dragGhost.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            suppressNextClick = dragStarted;
            ClearAllDomiNexDragVisualStates();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (draggedIndex >= 0 && dragStarted)
            {
                var from = draggedIndex;
                ClearAllDomiNexDragVisualStates();
                controller.SwapDomiNexSlots(from, index);
                return;
            }
            ClearAllDomiNexDragVisualStates();
        }

        public void OnPointerExit(PointerEventData eventData) => RestoreVisualState();

        private void CreateDragGhost(PointerEventData eventData)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            dragGhost = new GameObject("DomiNexDragGhost", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            dragGhost.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)dragGhost.transform;
            rect.sizeDelta = ((RectTransform)transform).rect.size;
            rect.position = eventData.position;
            dragGhost.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.35f);
            var group = dragGhost.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.alpha = 0.85f;
            dragGhost.transform.SetAsLastSibling();
        }

        public static void ClearAllDomiNexDragVisualStates()
        {
            foreach (var slot in ActiveSlots.ToArray())
                if (slot != null) slot.RestoreVisualState();
            if (dragGhost != null) { Destroy(dragGhost); dragGhost = null; }
            draggedIndex = -1;
            dragStarted = false;
        }

        private void RestoreVisualState()
        {
            if (outline == null) return;
            outline.effectColor = normalOutlineColor;
            outline.effectDistance = normalOutlineDistance;
        }
    }
}
