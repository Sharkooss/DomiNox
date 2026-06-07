using System.Collections;
using DomiNox.Scoring;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class FloatingTextService : MonoBehaviour
    {
        private Canvas canvas;

        public void Initialize(Canvas targetCanvas)
        {
            canvas = targetCanvas;
        }

        public void Spawn(string text, RectTransform source, FloatingTextType type)
        {
            if (canvas == null || source == null || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var go = new GameObject($"FloatingText_{type}", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(110f, 34f);
            var label = UiFactory.CreateText(go.transform, "Text", text, 18, TextAnchor.MiddleCenter);
            label.color = GetColor(type);
            label.fontStyle = FontStyle.Bold;
            label.raycastTarget = false;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;

            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, source.TransformPoint(source.rect.center));
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPoint, canvas.worldCamera, out var localPoint);
            rect.anchoredPosition = localPoint + new Vector2(0f, 18f);
            StartCoroutine(Animate(go.GetComponent<CanvasGroup>(), rect));
        }

        private static Color GetColor(FloatingTextType type)
        {
            switch (type)
            {
                case FloatingTextType.Count:
                    return new Color(0.2f, 0.65f, 1f);
                case FloatingTextType.Mult:
                    return new Color(1f, 0.28f, 0.24f);
                case FloatingTextType.Multiplier:
                    return new Color(0.78f, 0.38f, 1f);
                case FloatingTextType.Credits:
                    return new Color(1f, 0.84f, 0.25f);
                case FloatingTextType.Warning:
                    return new Color(0.72f, 0.05f, 0.05f);
                default:
                    return Color.white;
            }
        }

        private static IEnumerator Animate(CanvasGroup group, RectTransform rect)
        {
            const float duration = 0.75f;
            var start = rect.anchoredPosition;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                rect.anchoredPosition = start + new Vector2(0f, 44f * t);
                rect.localScale = Vector3.one * Mathf.Lerp(0.82f, 1.08f, Mathf.Sin(t * Mathf.PI));
                group.alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }

            Destroy(rect.gameObject);
        }
    }
}
