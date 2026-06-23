using DomiNox.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.Utilities
{
    public static class UiFactory
    {
        private static readonly Vector2 ReferenceResolution = new Vector2(1600f, 900f);

        public static void ConfigureCanvasScaler(CanvasScaler scaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        public static Text CreateText(Transform parent, string name, string value, int fontSize = 22, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = DomiNoxTheme.TextPrimary;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = DomiNoxTheme.BgCard;
            DomiNoxTheme.AddOutline(go, DomiNoxTheme.BorderNormal);
            var layout = go.GetComponent<LayoutElement>();
            layout.preferredWidth = 154f;
            layout.preferredHeight = 44f;
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
            colors.pressedColor = new Color(0f, 0f, 0f, 0.22f);
            button.colors = colors;
            var text = CreateText(go.transform, "Label", label, DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            text.color = DomiNoxTheme.TextPrimary;
            text.fontStyle = FontStyle.Bold;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(6f, 0f);
            text.rectTransform.offsetMax = new Vector2(-6f, 0f);
            return button;
        }

        public static void StyleButton(Button btn, Color bg, Color textColor, float preferredHeight = 44f)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = bg;
            var outline = btn.GetComponent<Outline>();
            if (outline == null) outline = btn.gameObject.AddComponent<Outline>();
            outline.effectColor = textColor;
            outline.effectDistance = new Vector2(2f, -2f);
            var lbl = btn.GetComponentInChildren<Text>();
            if (lbl != null) lbl.color = textColor;
            var le = btn.GetComponent<LayoutElement>();
            if (le != null) le.preferredHeight = preferredHeight;
        }

        public static GameObject CreatePanel(Transform parent, string name, Color color, RectOffset padding = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            DomiNoxTheme.AddOutline(go, DomiNoxTheme.BorderNormal);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = padding ?? new RectOffset(10, 10, 8, 8);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            return go;
        }

        public static GameObject CreateRow(Transform parent, string name, float spacing = 8f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return go;
        }

        public static GameObject CreateSeparator(Transform parent, float height = 1f, Color? color = null)
        {
            var go = new GameObject("Separator", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color ?? DomiNoxTheme.BorderNormal;
            go.GetComponent<LayoutElement>().preferredHeight = height;
            return go;
        }

        public static Text CreateIcon(Transform parent, string symbol, int size, Color color)
        {
            var text = CreateText(parent, "Icon", symbol, size, TextAnchor.MiddleCenter);
            text.color = color;
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            return text;
        }

        public static (GameObject root, Text label) CreatePill(Transform parent, string name, Color bg, Color textColor, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bg;
            DomiNoxTheme.AddOutline(go, textColor, 1.5f);
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = w;
            le.preferredHeight = h;
            var text = CreateText(go.transform, "Value", string.Empty, (int)(h * 0.54f), TextAnchor.MiddleCenter);
            text.color = textColor;
            text.fontStyle = FontStyle.Bold;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return (go, text);
        }
    }
}
