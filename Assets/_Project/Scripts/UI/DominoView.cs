using System;
using DomiNox.Dominoes;
using DomiNox.Grid;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class DominoView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public const float HorizontalWidth = 62f;
        public const float HorizontalHeight = 40f;
        public const float VerticalWidth = 40f;
        public const float VerticalHeight = 62f;

        private Image background;
        private Outline outline;
        private Text label;
        private DominoInstance domino;
        private DominoOrientation orientation = DominoOrientation.HorizontalRight;
        private bool isSelected;
        private bool isHovered;
        private bool isBanned;
        private bool isLocked;
        private Action<DominoInstance> clicked;
        private Func<DominoInstance, DominoView, bool> dragStarted;
        private Action<Vector2> dragged;
        private Action<Vector2> dragEnded;
        private GameObject dragGhost;
        private RectTransform dragGhostRect;
        private Text dragGhostLabel;

        public void Initialize(
            DominoInstance dominoInstance,
            Action<DominoInstance> onClicked,
            Func<DominoInstance, DominoView, bool> onDragStarted,
            Action<Vector2> onDragged,
            Action<Vector2> onDragEnded)
        {
            domino = dominoInstance;
            clicked = onClicked;
            dragStarted = onDragStarted;
            dragged = onDragged;
            dragEnded = onDragEnded;
            background = gameObject.AddComponent<Image>();
            background.color = Color.white;
            outline = gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(1f, -1f);
            outline.effectColor = new Color(0f, 0f, 0f, 0f);
            label = UiFactory.CreateText(transform, "Label", domino.ToString(), 17, TextAnchor.MiddleCenter);
            label.color = Color.black;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            ApplyOrientation();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            ApplyVisualState();
        }

        public void SetBossState(bool banned, bool locked)
        {
            isBanned = banned;
            isLocked = locked;
            ApplyVisualState();
        }

        private void ApplyVisualState()
        {
            if (background == null || label == null)
            {
                return;
            }

            label.color = Color.black;
            label.text = GetDisplayText();
            outline.effectColor = isSelected ? new Color(1f, 0.42f, 0.12f) : isHovered ? new Color(0.52f, 0.82f, 1f) : new Color(0f, 0f, 0f, 0f);
            outline.effectDistance = isSelected ? new Vector2(3f, -3f) : new Vector2(2f, -2f);

            if (isSelected && isBanned)
            {
                background.color = new Color(1f, 0.48f, 0.34f);
                label.color = Color.black;
                return;
            }

            if (isSelected)
            {
                background.color = GetBaseColor(new Color(1f, 0.86f, 0.25f));
                return;
            }

            if (isBanned)
            {
                background.color = new Color(0.32f, 0.32f, 0.36f);
                label.color = new Color(0.72f, 0.72f, 0.78f);
                return;
            }

            if (isLocked)
            {
                background.color = new Color(0.72f, 0.84f, 1f);
                label.text = $"{GetDisplayText()}\nLOCK";
                return;
            }

            background.color = GetBaseColor(Color.white);
        }

        private Color GetBaseColor(Color fallback)
        {
            var modifier = DominoModifierRegistry.GetById(domino?.ModifierId);
            if (modifier == null)
            {
                return fallback;
            }

            outline.effectColor = isSelected ? outline.effectColor : modifier.ColorTheme;
            return Color.Lerp(fallback, modifier.ColorTheme, domino.ModifierId == DominoModifierRegistry.GlassId ? 0.35f : 0.22f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            transform.localScale = Vector3.one * 1.06f;
            ApplyVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            transform.localScale = Vector3.one;
            ApplyVisualState();
        }

        public void SetOrientation(DominoOrientation dominoOrientation)
        {
            orientation = dominoOrientation;
            ApplyOrientation();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            clicked?.Invoke(domino);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (dragStarted?.Invoke(domino, this) != true)
            {
                return;
            }

            CreateDragGhost(eventData.position);
            dragged?.Invoke(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhost != null)
            {
                dragGhost.transform.position = eventData.position;
            }

            dragged?.Invoke(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DestroyDragGhost();
            dragEnded?.Invoke(eventData.position);
        }

        private void OnDisable()
        {
            DestroyDragGhostImmediateSafe();
        }

        private void OnDestroy()
        {
            DestroyDragGhostImmediateSafe();
        }

        private void CreateDragGhost(Vector2 position)
        {
            DestroyDragGhost();
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            dragGhost = new GameObject("DominoDragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            dragGhost.transform.SetParent(canvas.transform, false);
            dragGhost.transform.position = position;
            dragGhostRect = dragGhost.GetComponent<RectTransform>();
            dragGhostRect.sizeDelta = new Vector2(HorizontalWidth, HorizontalHeight);
            dragGhost.GetComponent<CanvasGroup>().blocksRaycasts = false;
            dragGhost.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.82f);

            dragGhostLabel = UiFactory.CreateText(dragGhost.transform, "Label", GetDisplayText(), 18, TextAnchor.MiddleCenter);
            dragGhostLabel.color = Color.black;
            dragGhostLabel.rectTransform.anchorMin = Vector2.zero;
            dragGhostLabel.rectTransform.anchorMax = Vector2.one;
            dragGhostLabel.rectTransform.offsetMin = Vector2.zero;
            dragGhostLabel.rectTransform.offsetMax = Vector2.zero;
            ApplyRectOrientation(dragGhostRect, dragGhostLabel.rectTransform);
        }

        public void SetDragGhostOverGrid(bool overGrid)
        {
            if (dragGhost != null && dragGhost.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = overGrid ? 0.5f : 1f;
            }
        }

        public void ForceClearDragGhost()
        {
            DestroyDragGhost();
        }

        private void DestroyDragGhost()
        {
            if (dragGhost != null)
            {
                Destroy(dragGhost);
                ClearDragGhostReferences();
            }
        }

        private void DestroyDragGhostImmediateSafe()
        {
            if (dragGhost != null)
            {
                Destroy(dragGhost);
                ClearDragGhostReferences();
            }
        }

        private void ClearDragGhostReferences()
        {
            dragGhost = null;
            dragGhostRect = null;
            dragGhostLabel = null;
        }

        private void ApplyOrientation()
        {
            if (label == null)
            {
                return;
            }

            ApplyVisualState();
            ApplyRectOrientation((RectTransform)transform, label.rectTransform);

            if (dragGhostLabel != null && dragGhostRect != null)
            {
                dragGhostLabel.text = GetDisplayText();
                ApplyRectOrientation(dragGhostRect, dragGhostLabel.rectTransform);
            }
        }

        private string GetDisplayText()
        {
            var first = GridState.GetCellValue(domino, orientation, 0);
            var second = GridState.GetCellValue(domino, orientation, 1);
            var modifier = DominoModifierRegistry.GetById(domino?.ModifierId);
            var badge = modifier == null ? string.Empty : $" {modifier.Badge}";
            return GridState.IsHorizontal(orientation) ? $"{first}|{second}{badge}" : $"{first}\n-\n{second}{badge}";
        }

        private void ApplyRectOrientation(RectTransform rect, RectTransform textRect)
        {
            if (GridState.IsHorizontal(orientation))
            {
                rect.sizeDelta = new Vector2(HorizontalWidth, HorizontalHeight);
                textRect.localRotation = Quaternion.identity;
                return;
            }

            rect.sizeDelta = new Vector2(VerticalWidth, VerticalHeight);
            textRect.localRotation = Quaternion.identity;
        }
    }
}
