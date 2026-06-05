using System;
using DomiNox.Dominoes;
using DomiNox.Grid;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class DominoView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public const float HorizontalWidth = 62f;
        public const float HorizontalHeight = 40f;
        public const float VerticalWidth = 40f;
        public const float VerticalHeight = 62f;

        private Image background;
        private Text label;
        private DominoInstance domino;
        private DominoOrientation orientation = DominoOrientation.HorizontalRight;
        private Action<DominoInstance> clicked;
        private Action<DominoInstance, DominoView> dragStarted;
        private Action<Vector2> dragged;
        private Action<Vector2> dragEnded;
        private GameObject dragGhost;
        private RectTransform dragGhostRect;
        private Text dragGhostLabel;

        public void Initialize(
            DominoInstance dominoInstance,
            Action<DominoInstance> onClicked,
            Action<DominoInstance, DominoView> onDragStarted,
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
            if (background != null)
            {
                background.color = selected ? new Color(1f, 0.86f, 0.25f) : Color.white;
            }
        }

        public void SetBossState(bool banned, bool locked)
        {
            if (background == null || label == null)
            {
                return;
            }

            if (banned)
            {
                background.color = new Color(0.32f, 0.32f, 0.36f);
                label.color = new Color(0.72f, 0.72f, 0.78f);
                return;
            }

            if (locked)
            {
                background.color = new Color(0.72f, 0.84f, 1f);
                label.color = Color.black;
                label.text = $"{GetDisplayText()}\nLOCK";
                return;
            }

            label.color = Color.black;
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
            dragStarted?.Invoke(domino, this);
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

            label.text = GetDisplayText();
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
            return GridState.IsHorizontal(orientation) ? $"{first}|{second}" : $"{first}\n-\n{second}";
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
