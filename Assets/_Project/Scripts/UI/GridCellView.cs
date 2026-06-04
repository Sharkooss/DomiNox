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

        public void Initialize(int cellX, int cellY, Action<int, int> clicked)
        {
            x = cellX;
            y = cellY;
            background = gameObject.AddComponent<Image>();
            background.color = new Color(0.18f, 0.2f, 0.24f);
            var button = gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => clicked?.Invoke(x, y));
            label = UiFactory.CreateText(transform, "Label", string.Empty, 16, TextAnchor.MiddleCenter);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }

        public void SetContent(string value, bool occupied)
        {
            label.text = value;
            background.color = occupied ? new Color(0.82f, 0.82f, 0.78f) : new Color(0.18f, 0.2f, 0.24f);
            label.color = occupied ? Color.black : Color.white;
        }

        public void SetPreview(string value, bool valid)
        {
            label.text = value;
            background.color = valid ? new Color(0.25f, 0.72f, 0.36f, 0.9f) : new Color(0.82f, 0.18f, 0.18f, 0.9f);
            label.color = Color.white;
        }
    }
}
