using System;
using DomiNox.Dominoes;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class DominoView : MonoBehaviour
    {
        private Image background;
        private Text label;
        private DominoInstance domino;
        private Action<DominoInstance> clicked;

        public void Initialize(DominoInstance dominoInstance, Action<DominoInstance> onClicked)
        {
            domino = dominoInstance;
            clicked = onClicked;
            background = gameObject.AddComponent<Image>();
            background.color = Color.white;
            var button = gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => clicked?.Invoke(domino));
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
    }
}
