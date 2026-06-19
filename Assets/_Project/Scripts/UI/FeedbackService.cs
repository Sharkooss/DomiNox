using System.Collections;
using DomiNox.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class FeedbackService : MonoBehaviour
    {
        private Canvas canvas;
        private Image screenFlash;
        private FloatingTextService floatingText;

        public static bool ReducedMotion { get; set; }

        public void Initialize(Canvas targetCanvas, FloatingTextService textService)
        {
            canvas = targetCanvas;
            floatingText = textService;
            var flash = new GameObject("ScreenFlash", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            flash.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)flash.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            screenFlash = flash.GetComponent<Image>();
            screenFlash.color = new Color(1f, 1f, 1f, 0f);
            flash.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        public void Floating(string text, RectTransform source, FloatingTextType type)
        {
            floatingText?.Spawn(text, source, type);
        }

        public void Pulse(RectTransform rect, float strength = 1.1f)
        {
            if (!ReducedMotion && rect != null)
            {
                StartCoroutine(PulseRoutine(rect, strength));
            }
        }

        public void Shake(RectTransform rect, float strength = 5f)
        {
            if (!ReducedMotion && rect != null)
            {
                StartCoroutine(ShakeRoutine(rect, strength));
            }
        }

        public void Flash(Color color, float alpha = 0.18f)
        {
            if (!ReducedMotion && screenFlash != null)
            {
                screenFlash.transform.SetAsLastSibling();
                StartCoroutine(FlashRoutine(color, alpha));
            }
        }

        public void CasinoBurst(RectTransform source, Color color, int count = 8)
        {
            if (ReducedMotion || canvas == null || source == null)
            {
                return;
            }

            for (var i = 0; i < count; i++)
            {
                SpawnChip(source, color, i, count);
            }
        }

        private void SpawnChip(RectTransform source, Color color, int index, int count)
        {
            var go = new GameObject("ChipBurst", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(10f, 10f);
            go.GetComponent<Image>().color = color;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, source.TransformPoint(source.rect.center));
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPoint, canvas.worldCamera, out var localPoint);
            rect.anchoredPosition = localPoint;
            var angle = (Mathf.PI * 2f * index) / Mathf.Max(1, count);
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            StartCoroutine(ChipRoutine(go.GetComponent<CanvasGroup>(), rect, direction));
        }

        private static IEnumerator PulseRoutine(RectTransform rect, float strength)
        {
            var start = rect.localScale;
            const float duration = 0.18f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                rect.localScale = start * Mathf.Lerp(1f, strength, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            rect.localScale = start;
        }

        private static IEnumerator ShakeRoutine(RectTransform rect, float strength)
        {
            var start = rect.anchoredPosition;
            const float duration = 0.16f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = 1f - Mathf.Clamp01(elapsed / duration);
                rect.anchoredPosition = start + new Vector2(Mathf.Sin(elapsed * 90f) * strength * t, 0f);
                yield return null;
            }

            rect.anchoredPosition = start;
        }

        private IEnumerator FlashRoutine(Color color, float alpha)
        {
            const float duration = 0.22f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                screenFlash.color = new Color(color.r, color.g, color.b, alpha * (1f - t));
                yield return null;
            }

            screenFlash.color = new Color(color.r, color.g, color.b, 0f);
        }

        private static IEnumerator ChipRoutine(CanvasGroup group, RectTransform rect, Vector2 direction)
        {
            const float duration = 0.55f;
            var start = rect.anchoredPosition;
            var distance = Random.Range(28f, 58f);
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                rect.anchoredPosition = start + direction * distance * Mathf.Sin(t * Mathf.PI * 0.5f);
                rect.localRotation = Quaternion.Euler(0f, 0f, t * 240f);
                group.alpha = 1f - t;
                yield return null;
            }

            Destroy(rect.gameObject);
        }
    }
}
