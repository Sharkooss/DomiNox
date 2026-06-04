using System;
using DomiNox.Dominoes;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class DominoView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Image background;
        private Text label;
        private DominoInstance domino;
        private Action<DominoInstance> clicked;
        private Action<DominoInstance> dragStarted;
        private Action<Vector2> dragged;
        private Action<Vector2> dragEnded;
        private GameObject dragGhost;

        public void Initialize(
            DominoInstance dominoInstance,
            Action<DominoInstance> onClicked,
            Action<DominoInstance> onDragStarted,
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
            label = UiFactory.CreateText(transform, "Label", domino.ToString(), 20, TextAnchor.MiddleCenter);
            label.color = Color.black;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }

        public void SetSelected(bool selected)
        {
            if (background != null)
            {
                background.color = selected ? new Color(1f, 0.86f, 0.25f) : Color.white;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            clicked?.Invoke(domino);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragStarted?.Invoke(domino);
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
            var rect = dragGhost.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(86f, 54f);
            dragGhost.GetComponent<CanvasGroup>().blocksRaycasts = false;
            dragGhost.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.82f);

            var ghostLabel = UiFactory.CreateText(dragGhost.transform, "Label", domino.ToString(), 22, TextAnchor.MiddleCenter);
            ghostLabel.color = Color.black;
            ghostLabel.rectTransform.anchorMin = Vector2.zero;
            ghostLabel.rectTransform.anchorMax = Vector2.one;
            ghostLabel.rectTransform.offsetMin = Vector2.zero;
            ghostLabel.rectTransform.offsetMax = Vector2.zero;
        }

        private void DestroyDragGhost()
        {
            if (dragGhost != null)
            {
                Destroy(dragGhost);
                dragGhost = null;
            }
        }
    }
}
