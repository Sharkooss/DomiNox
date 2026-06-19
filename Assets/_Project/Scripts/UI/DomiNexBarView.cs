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

            detailRoot = new GameObject("SelectedItemDetail", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image), typeof(Outline), typeof(CanvasGroup)).transform;
            detailRoot.SetParent(canvas.transform, false);
            detailRect = (RectTransform)detailRoot;
            detailRect.sizeDelta = new Vector2(310f, 220f);
            detailRoot.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.11f, 0.98f);
            detailRoot.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
            var detailLayout = detailRoot.GetComponent<VerticalLayoutGroup>();
            detailLayout.padding = new RectOffset(12, 12, 10, 10);
            detailLayout.spacing = 6f;
            detailLayout.childAlignment = TextAnchor.UpperCenter;
            detailRoot.gameObject.SetActive(false);
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
            slotRects.Clear();

            title.text = $"DomiNex\n{run.ActiveDomiNexCount}/{run.MaxDomiNexSlots}";
            currentRun = run;

            for (var i = 0; i < run.MaxDomiNexSlots; i++)
            {
                var instance = run.DomiNexInventory.GetInstanceAt(i);
                if (instance == null)
                {
                    CreateEmptySlot(i);
                }
                else
                {
                    CreateDomiNexCard(i, instance);
                }
            }

            if (run.DomiNexInventory.Active.Count == 0)
            {
                hoverCard.gameObject.SetActive(false);
            }

            consumablesTitle.text = $"Consumables {run.ActiveConsumableCount}/{run.MaxConsumableSlots}";
            for (var i = 0; i < run.MaxConsumableSlots; i++)
            {
                CreateConsumableSlot(i, run);
            }

            RenderUseButton(run);
            RenderSelectedDetail(run);
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
            useButton.gameObject.SetActive(false);
            return;

#pragma warning disable CS0162
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
#pragma warning restore CS0162
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
            slotRects[index] = (RectTransform)slot.transform;
            slot.AddComponent<DomiNexSlotInteraction>().Initialize(controller, index, false);
        }

        private void CreateDomiNexCard(int index, ActiveDomiNexInstance instance)
        {
            var definition = instance.Definition;
            var card = new GameObject($"DomiNex_{definition.Id}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement), typeof(DomiNexHoverTarget));
            card.transform.SetParent(slotsRoot, false);
            card.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.18f, 0.98f);
            var rarityColor = DomiNexUiStyles.GetRarityColor(definition.Rarity);
            var outline = card.GetComponent<Outline>();
            outline.effectColor = controller.SelectedDomiNexIndex == index ? new Color(1f, 0.84f, 0.25f) : rarityColor;
            outline.effectDistance = controller.SelectedDomiNexIndex == index ? new Vector2(4f, -4f) : new Vector2(2f, -2f);
            var layout = card.GetComponent<LayoutElement>();
            layout.preferredWidth = 48f;
            layout.preferredHeight = 48f;
            card.GetComponent<DomiNexHoverTarget>().Initialize(definition, ShowHoverCard, HideHoverCard);
            activeCards[definition.Id] = (RectTransform)card.transform;
            slotRects[index] = (RectTransform)card.transform;
            card.AddComponent<DomiNexSlotInteraction>().Initialize(controller, index, true);

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

        private void RenderSelectedDetail(RunState run)
        {
            Clear(detailRoot);
            var selectedDomiNex = controller.SelectedDomiNexIndex;
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
            if (instance == null)
            {
                detailRoot.gameObject.SetActive(false);
                return;
            }

            var definition = instance.Definition;
            var rarityColor = DomiNexUiStyles.GetRarityColor(definition.Rarity);
            detailRoot.GetComponent<Outline>().effectColor = rarityColor;
            AddText(detailRoot, definition.Name, 18, Color.white, TextAnchor.MiddleCenter);
            AddText(detailRoot, definition.Rarity.ToString(), 12, rarityColor, TextAnchor.MiddleCenter);
            var dynamicText = definition.GetCurrentEffectText(run);
            AddText(detailRoot, string.IsNullOrWhiteSpace(dynamicText) ? definition.Description : $"{definition.Description}\n{dynamicText}", 12, new Color(0.82f, 0.86f, 0.92f), TextAnchor.UpperLeft, true);
            if (definition.Tags.Count > 0)
            {
                AddText(detailRoot, string.Join("  |  ", definition.Tags), 10, new Color(0.65f, 0.72f, 0.8f), TextAnchor.MiddleCenter);
            }

            var sellValue = SellValueService.GetDomiNexSellValue(instance);
            var sell = UiFactory.CreateButton(detailRoot, "SellDomiNex", $"Sell ${sellValue}");
            sell.GetComponent<LayoutElement>().preferredHeight = 34f;
            sell.onClick.AddListener(controller.SellSelectedDomiNex);
            detailRoot.gameObject.SetActive(true);
            PositionDetail(slotRects.TryGetValue(index, out var source) ? source : (RectTransform)transform);
        }

        private void RenderConsumableDetail(RunState run, int index)
        {
            var instance = run.Consumables.GetInstanceAt(index);
            var definition = GemTileRegistry.GetById(instance?.DefinitionId);
            if (definition == null)
            {
                detailRoot.gameObject.SetActive(false);
                return;
            }

            detailRoot.GetComponent<Outline>().effectColor = new Color(0.28f, 0.75f, 0.68f);
            var pattern = PatternCatalog.GetById(definition.TargetPatternId);
            var currentLevel = run.PatternLevels.GetLevel(definition.TargetPatternId);
            var nextLevel = System.Math.Min(currentLevel + 1, DomiNox.Core.GameConstants.MaxPatternLevel);
            AddText(detailRoot, definition.Name, 18, Color.white, TextAnchor.MiddleCenter);
            AddText(detailRoot, "Gem Tile", 12, new Color(0.76f, 1f, 0.94f), TextAnchor.MiddleCenter);
            AddText(detailRoot, $"{definition.Description}\nTarget: {pattern?.Name ?? definition.TargetPatternId}\nCurrent: Lv.{currentLevel}\nAfter use: Lv.{nextLevel}", 12, new Color(0.82f, 0.86f, 0.92f), TextAnchor.UpperLeft, true);
            var buttonRow = new GameObject("Actions", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            buttonRow.SetParent(detailRoot, false);
            buttonRow.GetComponent<LayoutElement>().preferredHeight = 36f;
            var rowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childForceExpandWidth = true;
            var use = UiFactory.CreateButton(buttonRow, "UseConsumable", run.PatternLevels.IsMaxLevel(definition.TargetPatternId) ? "Max Level" : "Use");
            use.interactable = !run.PatternLevels.IsMaxLevel(definition.TargetPatternId);
            use.onClick.AddListener(controller.UseSelectedConsumable);
            var sellValue = SellValueService.GetConsumableSellValue(instance);
            var sell = UiFactory.CreateButton(buttonRow, "SellConsumable", $"Sell ${sellValue}");
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

        private static void Clear(Transform root)
        {
            foreach (Transform child in root)
            {
                Destroy(child.gameObject);
            }
        }

        private static void AddText(Transform parent, string value, int size, Color color, TextAnchor anchor, bool flexible = false)
        {
            var text = UiFactory.CreateText(parent, "Text", value, size, anchor);
            text.color = color;
            if (flexible)
            {
                text.GetComponent<LayoutElement>().flexibleHeight = 1f;
            }
        }

        public RectTransform GetDomiNexRect(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && activeCards.TryGetValue(id, out var rect) ? rect : null;
        }
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
            if (outline != null)
            {
                normalOutlineColor = outline.effectColor;
                normalOutlineDistance = outline.effectDistance;
            }
        }

        private void OnEnable()
        {
            if (!ActiveSlots.Contains(this))
            {
                ActiveSlots.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveSlots.Remove(this);
            ClearAllDomiNexDragVisualStates();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (suppressNextClick)
            {
                suppressNextClick = false;
                return;
            }

            if (occupied)
            {
                controller.SelectDomiNex(index);
            }
            else
            {
                controller.SelectDomiNex(-1);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragStartPosition = eventData.position;
            draggedIndex = occupied ? index : -1;
            dragStarted = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (draggedIndex < 0)
            {
                return;
            }

            if (!dragStarted && Vector2.Distance(dragStartPosition, eventData.position) < DragThresholdPixels)
            {
                return;
            }

            if (!dragStarted)
            {
                dragStarted = true;
                CreateDragGhost(eventData);
            }

            if (dragGhost != null)
            {
                dragGhost.transform.position = eventData.position;
            }
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

        public void OnPointerExit(PointerEventData eventData)
        {
            RestoreVisualState();
        }

        private void CreateDragGhost(PointerEventData eventData)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            dragGhost = new GameObject("DomiNexDragGhost", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            dragGhost.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)dragGhost.transform;
            rect.sizeDelta = ((RectTransform)transform).rect.size;
            rect.position = eventData.position;
            dragGhost.GetComponent<Image>().color = new Color(1f, 0.84f, 0.25f, 0.35f);
            var group = dragGhost.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.alpha = 0.85f;
            dragGhost.transform.SetAsLastSibling();
        }

        public static void ClearAllDomiNexDragVisualStates()
        {
            foreach (var slot in ActiveSlots.ToArray())
            {
                if (slot != null)
                {
                    slot.RestoreVisualState();
                }
            }

            if (dragGhost != null)
            {
                Destroy(dragGhost);
                dragGhost = null;
            }

            draggedIndex = -1;
            dragStarted = false;
        }

        private void RestoreVisualState()
        {
            if (outline == null)
            {
                return;
            }

            outline.effectColor = normalOutlineColor;
            outline.effectDistance = normalOutlineDistance;
        }
    }
}
