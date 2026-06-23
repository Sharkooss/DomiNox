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
            if (canvas == null || source == null || string.IsNullOrWhiteSpace(text)) return;

            var go = new GameObject($"FloatingText_{type}", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(130f, 38f);

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            shadow.effectDistance = new Vector2(1f, -1f);

            var label = UiFactory.CreateText(go.transform, "Text", text, DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            label.color = GetColor(type);
            label.fontStyle = FontStyle.Bold;
            label.raycastTarget = false;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;

            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, source.TransformPoint(source.rect.center));
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPoint, canvas.worldCamera, out var localPoint);
            rect.anchoredPosition = localPoint + new Vector2(0f, 22f);
            go.transform.SetAsLastSibling();
            StartCoroutine(Animate(go.GetComponent<CanvasGroup>(), rect, type));
        }

        private static Color GetColor(FloatingTextType type)
        {
            switch (type)
            {
                case FloatingTextType.Count:      return DomiNoxTheme.CountBlue;
                case FloatingTextType.Mult:       return DomiNoxTheme.MultRed;
                case FloatingTextType.Multiplier: return DomiNoxTheme.JackpotViolet;
                case FloatingTextType.Credits:    return DomiNoxTheme.Gold;
                case FloatingTextType.Warning:    return DomiNoxTheme.Danger;
                default:                          return DomiNoxTheme.TextPrimary;
            }
        }

        private static IEnumerator Animate(CanvasGroup group, RectTransform rect, FloatingTextType type)
        {
            var duration = type == FloatingTextType.Multiplier ? 1.10f : 0.82f;
            var rise     = type == FloatingTextType.Multiplier ? 66f    : 48f;
            var start    = rect.anchoredPosition;
            var elapsed  = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var ease = 1f - Mathf.Pow(1f - t, 2f);
                rect.anchoredPosition = start + new Vector2(0f, rise * ease);
                var bounceScale = type == FloatingTextType.Multiplier
                    ? Mathf.Lerp(0.75f, 1.18f, Mathf.Sin(t * Mathf.PI * 0.9f))
                    : Mathf.Lerp(0.80f, 1.05f, Mathf.Sin(t * Mathf.PI));
                rect.localScale = Vector3.one * bounceScale;
                group.alpha = t < 0.65f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (t - 0.65f) / 0.35f);
                yield return null;
            }

            Destroy(rect.gameObject);
        }
    }
}
