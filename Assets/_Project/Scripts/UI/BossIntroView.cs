using DomiNox.Bosses;
using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class BossIntroView : MonoBehaviour
    {
        private const float RevealDuration = 2.2f;
        private const float FadeDuration = 0.75f;
        private const float TotalDuration = RevealDuration + FadeDuration;

        private GameFlowController controller;
        private CanvasGroup canvasGroup;
        private Text title;
        private Text description;
        private Text roulette;
        private BossLevelState activeBoss;
        private int activeLevelIndex = -1;
        private float elapsed;
        private bool completionSent;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(120, 120, 170, 120);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var badge = new GameObject("BossBadge", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            badge.transform.SetParent(transform, false);
            badge.GetComponent<Image>().color = new Color(0.12f, 0.04f, 0.06f, 0.96f);
            var outline = badge.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 0.2f, 0.16f);
            outline.effectDistance = new Vector2(4f, -4f);
            var badgeLayout = badge.GetComponent<VerticalLayoutGroup>();
            badgeLayout.padding = new RectOffset(34, 34, 28, 30);
            badgeLayout.spacing = 16f;
            badgeLayout.childAlignment = TextAnchor.MiddleCenter;
            badge.GetComponent<LayoutElement>().preferredWidth = 760f;
            badge.GetComponent<LayoutElement>().preferredHeight = 360f;

            var kicker = UiFactory.CreateText(badge.transform, "Kicker", "BOSS", 22, TextAnchor.MiddleCenter);
            kicker.color = new Color(1f, 0.42f, 0.3f);
            kicker.GetComponent<LayoutElement>().preferredHeight = 34f;

            title = UiFactory.CreateText(badge.transform, "Title", string.Empty, 36, TextAnchor.MiddleCenter);
            title.color = new Color(1f, 0.82f, 0.28f);
            title.GetComponent<LayoutElement>().preferredHeight = 56f;

            description = UiFactory.CreateText(badge.transform, "Description", string.Empty, 18, TextAnchor.MiddleCenter);
            description.color = new Color(0.92f, 0.94f, 0.98f);
            description.GetComponent<LayoutElement>().preferredHeight = 88f;

            roulette = UiFactory.CreateText(badge.transform, "Roulette", string.Empty, 48, TextAnchor.MiddleCenter);
            roulette.color = new Color(1f, 0.25f, 0.18f);
            roulette.GetComponent<LayoutElement>().preferredHeight = 74f;

            gameObject.SetActive(false);
        }

        public void Render(RunState run)
        {
            var boss = run.CurrentLevel.Boss;
            if (boss == null || !controller.BossIntroActive)
            {
                gameObject.SetActive(false);
                return;
            }

            if (activeBoss != boss || activeLevelIndex != run.CurrentLevel.LevelIndex)
            {
                StartIntro(run.CurrentLevel.LevelIndex, boss);
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void Update()
        {
            if (!gameObject.activeSelf || activeBoss == null)
            {
                return;
            }

            elapsed += Time.deltaTime;
            UpdateRoulette();
            canvasGroup.alpha = elapsed <= RevealDuration ? 1f : Mathf.Clamp01(1f - ((elapsed - RevealDuration) / FadeDuration));

            if (!completionSent && elapsed >= TotalDuration)
            {
                completionSent = true;
                gameObject.SetActive(false);
                controller.CompleteBossIntro();
            }
        }

        private void StartIntro(int levelIndex, BossLevelState boss)
        {
            activeBoss = boss;
            activeLevelIndex = levelIndex;
            elapsed = 0f;
            completionSent = false;
            canvasGroup.alpha = 1f;
            title.text = boss.Definition.Name;
            description.text = boss.GetEffectSummary();
            roulette.gameObject.SetActive(boss.Definition.RuleType == BossRuleType.BannedValue && boss.BannedValue.HasValue);
            UpdateRoulette();
        }

        private void UpdateRoulette()
        {
            if (!roulette.gameObject.activeSelf || !activeBoss.BannedValue.HasValue)
            {
                return;
            }

            var value = elapsed < RevealDuration - 0.45f
                ? Mathf.FloorToInt(elapsed * 12f) % 7
                : activeBoss.BannedValue.Value;
            roulette.text = $"Roulette: {value}";
        }
    }
}
