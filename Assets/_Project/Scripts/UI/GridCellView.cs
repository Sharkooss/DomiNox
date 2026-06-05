using System;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class GridCellView : MonoBehaviour
    {
        private int x;
        private int y;
        private Image background;
        private Text label;
        private Outline outline;
        private Image topBorder;
        private Image rightBorder;
        private Image bottomBorder;
        private Image leftBorder;
        private Image verticalSeparator;
        private Image horizontalSeparator;

        public void Initialize(int cellX, int cellY, Action<int, int> clicked)
        {
            x = cellX;
            y = cellY;
            background = gameObject.AddComponent<Image>();
            background.color = new Color(0.18f, 0.2f, 0.24f);
            outline = gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(3f, -3f);
            outline.enabled = false;
            var button = gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => clicked?.Invoke(x, y));
            label = UiFactory.CreateText(transform, "Label", string.Empty, 16, TextAnchor.MiddleCenter);
            label.raycastTarget = false;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            topBorder = CreateLine("TopBorder", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 4f));
            rightBorder = CreateLine("RightBorder", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(4f, 0f));
            bottomBorder = CreateLine("BottomBorder", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f));
            leftBorder = CreateLine("LeftBorder", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(4f, 0f));
            verticalSeparator = CreateLine("VerticalSeparator", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(4f, 34f));
            horizontalSeparator = CreateLine("HorizontalSeparator", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(34f, 4f));
            SetFrame(false, false, false, false, false, false);
        }

        public void SetContent(string value, bool occupied, DominoCellFrame frame)
        {
            label.text = value;
            background.color = occupied ? new Color(0.82f, 0.82f, 0.78f) : new Color(0.18f, 0.2f, 0.24f);
            label.color = occupied ? Color.black : Color.white;
            outline.enabled = false;
            SetFrame(frame.Top, frame.Right, frame.Bottom, frame.Left, frame.VerticalSeparator, frame.HorizontalSeparator);
        }

        public void SetPreview(string value, bool valid)
        {
            label.text = value;
            background.color = valid ? new Color(0.25f, 0.72f, 0.36f, 0.9f) : new Color(0.82f, 0.18f, 0.18f, 0.9f);
            label.color = Color.white;
            outline.enabled = true;
            outline.effectColor = valid ? new Color(0.8f, 1f, 0.84f) : new Color(1f, 0.72f, 0.72f);
            SetFrame(false, false, false, false, false, false);
        }

        private Image CreateLine(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            return image;
        }

        private void SetFrame(bool top, bool right, bool bottom, bool left, bool separatorVertical, bool separatorHorizontal)
        {
            topBorder.gameObject.SetActive(top);
            rightBorder.gameObject.SetActive(right);
            bottomBorder.gameObject.SetActive(bottom);
            leftBorder.gameObject.SetActive(left);
            verticalSeparator.gameObject.SetActive(separatorVertical);
            horizontalSeparator.gameObject.SetActive(separatorHorizontal);
        }
    }

    public readonly struct DominoCellFrame
    {
        public static DominoCellFrame Empty { get; } = new DominoCellFrame(false, false, false, false, false, false);

        public bool Top { get; }
        public bool Right { get; }
        public bool Bottom { get; }
        public bool Left { get; }
        public bool VerticalSeparator { get; }
        public bool HorizontalSeparator { get; }

        public DominoCellFrame(bool top, bool right, bool bottom, bool left, bool verticalSeparator, bool horizontalSeparator)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
            VerticalSeparator = verticalSeparator;
            HorizontalSeparator = horizontalSeparator;
        }
    }
}
