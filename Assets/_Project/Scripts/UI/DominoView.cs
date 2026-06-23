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
        public const float HorizontalWidth  = 72f;
        public const float HorizontalHeight = 46f;
        public const float VerticalWidth    = 46f;
        public const float VerticalHeight   = 72f;

        private Image background;
        private Outline outline;
        private Shadow shadow;
        private Text label;
        private Image bannedOverlay;
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
            background.color = DomiNoxTheme.IvoryWhite;
            outline = gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = new Color(0f, 0f, 0f, 0f);
            shadow = DomiNoxTheme.AddShadow(gameObject, new Color(0f, 0f, 0f, 0.45f), new Vector2(2f, -2f));

            bannedOverlay = new GameObject("BannedOverlay", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            bannedOverlay.transform.SetParent(transform, false);
            var bannedRect = bannedOverlay.rectTransform;
            bannedRect.anchorMin = Vector2.zero;
            bannedRect.anchorMax = Vector2.one;
            bannedRect.offsetMin = Vector2.zero;
            bannedRect.offsetMax = Vector2.zero;
            bannedOverlay.color = new Color(1f, 0.15f, 0.1f, 0.28f);
            bannedOverlay.raycastTarget = false;
            bannedOverlay.gameObject.SetActive(false);

            label = UiFactory.CreateText(transform, "Label", domino.ToString(), DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            label.color = DomiNoxTheme.IvoryDark;
            label.fontStyle = FontStyle.Bold;
            label.raycastTarget = false;
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
            if (background == null || label == null) return;

            bannedOverlay.gameObject.SetActive(false);
            label.text = GetDisplayText();
            label.color = DomiNoxTheme.IvoryDark;

            if (isSelected && isBanned)
            {
                background.color = new Color(0.88f, 0.68f, 0.62f);
                outline.effectColor = DomiNoxTheme.MultRed;
                outline.effectDistance = new Vector2(3f, -3f);
                bannedOverlay.gameObject.SetActive(true);
                return;
            }

            if (isSelected)
            {
                background.color = GetBaseColor(new Color(1f, 0.92f, 0.60f));
                outline.effectColor = DomiNoxTheme.Gold;
                outline.effectDistance = new Vector2(4f, -4f);
                transform.localScale = Vector3.one * 1.05f;
                return;
            }

            if (isBanned)
            {
                background.color = new Color(0.20f, 0.20f, 0.22f);
                label.color = DomiNoxTheme.TextMuted;
                outline.effectColor = DomiNoxTheme.MultRed;
                outline.effectDistance = new Vector2(1f, -1f);
                bannedOverlay.gameObject.SetActive(true);
                transform.localScale = Vector3.one;
                return;
            }

            if (isLocked)
            {
                background.color = new Color(0.78f, 0.88f, 1.00f);
                label.color = new Color(0.10f, 0.20f, 0.42f);
                label.text = $"{GetDisplayText()} [L]";
                outline.effectColor = DomiNoxTheme.CountBlue;
                outline.effectDistance = new Vector2(2f, -2f);
                transform.localScale = Vector3.one;
                return;
            }

            if (isHovered && !isSelected)
            {
                outline.effectColor = DomiNoxTheme.CountBlueDim;
                outline.effectDistance = new Vector2(2f, -2f);
                transform.localScale = Vector3.one * 1.03f;
            }
            else if (!isSelected)
            {
                outline.effectColor = new Color(0f, 0f, 0f, 0f);
                outline.effectDistance = new Vector2(1f, -1f);
                transform.localScale = Vector3.one;
            }

            background.color = GetBaseColor(DomiNoxTheme.IvoryWhite);
        }

        private Color GetBaseColor(Color fallback)
        {
            var modifier = DominoModifierRegistry.GetById(domino?.ModifierId);
            if (modifier == null) return fallback;
            if (!isSelected)
            {
                outline.effectColor = Color.Lerp(modifier.ColorTheme, DomiNoxTheme.BorderNormal, 0.25f);
                outline.effectDistance = new Vector2(2f, -2f);
            }
            return Color.Lerp(fallback, modifier.ColorTheme, domino.ModifierId == DominoModifierRegistry.GlassId ? 0.30f : 0.18f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            ApplyVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            ApplyVisualState();
        }

        public void SetOrientation(DominoOrientation dominoOrientation)
        {
            orientation = dominoOrientation;
            ApplyOrientation();
        }

        public void OnPointerClick(PointerEventData eventData) => clicked?.Invoke(domino);

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (dragStarted?.Invoke(domino, this) != true) return;
            CreateDragGhost(eventData.position);
            dragged?.Invoke(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhost != null) dragGhost.transform.position = eventData.position;
            dragged?.Invoke(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DestroyDragGhost();
            dragEnded?.Invoke(eventData.position);
        }

        private void OnDisable() => DestroyDragGhostImmediateSafe();
        private void OnDestroy() => DestroyDragGhostImmediateSafe();

        private void CreateDragGhost(Vector2 position)
        {
            DestroyDragGhost();
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            dragGhost = new GameObject("DominoDragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(Shadow));
            dragGhost.transform.SetParent(canvas.transform, false);
            dragGhost.transform.position = position;
            dragGhostRect = dragGhost.GetComponent<RectTransform>();
            dragGhostRect.sizeDelta = new Vector2(HorizontalWidth, HorizontalHeight);
            dragGhost.GetComponent<CanvasGroup>().blocksRaycasts = false;
            dragGhost.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.IvoryWhite, 0.88f);
            var ghostShadow = dragGhost.GetComponent<Shadow>();
            ghostShadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            ghostShadow.effectDistance = new Vector2(3f, -3f);

            dragGhostLabel = UiFactory.CreateText(dragGhost.transform, "Label", GetDisplayText(), DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            dragGhostLabel.color = DomiNoxTheme.IvoryDark;
            dragGhostLabel.fontStyle = FontStyle.Bold;
            dragGhostLabel.raycastTarget = false;
            dragGhostLabel.rectTransform.anchorMin = Vector2.zero;
            dragGhostLabel.rectTransform.anchorMax = Vector2.one;
            dragGhostLabel.rectTransform.offsetMin = Vector2.zero;
            dragGhostLabel.rectTransform.offsetMax = Vector2.zero;
            ApplyRectOrientation(dragGhostRect, dragGhostLabel.rectTransform);
        }

        public void SetDragGhostOverGrid(bool overGrid)
        {
            if (dragGhost != null && dragGhost.TryGetComponent<CanvasGroup>(out var cg))
                cg.alpha = overGrid ? 0.45f : 1f;
        }

        public void ForceClearDragGhost() => DestroyDragGhost();

        private void DestroyDragGhost()
        {
            if (dragGhost != null) { Destroy(dragGhost); ClearDragGhostReferences(); }
        }

        private void DestroyDragGhostImmediateSafe()
        {
            if (dragGhost != null) { Destroy(dragGhost); ClearDragGhostReferences(); }
        }

        private void ClearDragGhostReferences()
        {
            dragGhost = null;
            dragGhostRect = null;
            dragGhostLabel = null;
        }

        private void ApplyOrientation()
        {
            if (label == null) return;
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
            var first  = GridState.GetCellValue(domino, orientation, 0);
            var second = GridState.GetCellValue(domino, orientation, 1);
            var modifier = DominoModifierRegistry.GetById(domino?.ModifierId);
            var badge = modifier == null ? string.Empty : $" {modifier.Badge}";
            return GridState.IsHorizontal(orientation)
                ? $"{first} | {second}{badge}"
                : $"{first}\n—\n{second}{badge}";
        }

        private void ApplyRectOrientation(RectTransform rect, RectTransform textRect)
        {
            if (GridState.IsHorizontal(orientation))
            {
                rect.sizeDelta = new Vector2(HorizontalWidth, HorizontalHeight);
                textRect.localRotation = Quaternion.identity;
            }
            else
            {
                rect.sizeDelta = new Vector2(VerticalWidth, VerticalHeight);
                textRect.localRotation = Quaternion.identity;
            }
        }
    }
}
