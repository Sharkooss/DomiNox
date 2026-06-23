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
        private const float FadeDuration   = 0.75f;
        private const float TotalDuration  = RevealDuration + FadeDuration;

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
            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.78f);
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(120, 120, 160, 120);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var badge = new GameObject("BossBadge", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            badge.transform.SetParent(transform, false);
            badge.GetComponent<Image>().color = new Color(0.10f, 0.03f, 0.04f, 0.98f);
            DomiNoxTheme.AddOutline(badge, DomiNoxTheme.MultRed, 4f);
            DomiNoxTheme.AddShadow(badge, new Color(1f, 0.10f, 0.05f, 0.28f), new Vector2(0f, -4f));
            var badgeLayout = badge.GetComponent<VerticalLayoutGroup>();
            badgeLayout.padding = new RectOffset(38, 38, 32, 34);
            badgeLayout.spacing = 18f;
            badgeLayout.childAlignment = TextAnchor.MiddleCenter;
            badge.GetComponent<LayoutElement>().preferredWidth = 780f;
            badge.GetComponent<LayoutElement>().preferredHeight = 380f;

            var kicker = UiFactory.CreateText(badge.transform, "Kicker", "— BOSS —", DomiNoxTheme.FontLG, TextAnchor.MiddleCenter);
            kicker.color = DomiNoxTheme.Danger;
            kicker.fontStyle = FontStyle.Bold;
            kicker.GetComponent<LayoutElement>().preferredHeight = 36f;

            UiFactory.CreateSeparator(badge.transform, 1f, DomiNoxTheme.WithAlpha(DomiNoxTheme.MultRed, 0.45f));

            title = UiFactory.CreateText(badge.transform, "Title", string.Empty, DomiNoxTheme.FontXXL, TextAnchor.MiddleCenter);
            title.color = DomiNoxTheme.Gold;
            title.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(title.gameObject, new Color(1f, 0.5f, 0f, 0.38f), new Vector2(2f, -2f));
            title.GetComponent<LayoutElement>().preferredHeight = 58f;

            description = UiFactory.CreateText(badge.transform, "Description", string.Empty, DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            description.color = DomiNoxTheme.TextPrimary;
            description.GetComponent<LayoutElement>().preferredHeight = 90f;

            roulette = UiFactory.CreateText(badge.transform, "Roulette", string.Empty, 52, TextAnchor.MiddleCenter);
            roulette.color = DomiNoxTheme.MultRed;
            roulette.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(roulette.gameObject, new Color(1f, 0.1f, 0.05f, 0.45f), new Vector2(2f, -2f));
            roulette.GetComponent<LayoutElement>().preferredHeight = 78f;

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
                StartIntro(run.CurrentLevel.LevelIndex, boss);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void Update()
        {
            if (!gameObject.activeSelf || activeBoss == null) return;
            elapsed += Time.deltaTime;
            UpdateRoulette();
            canvasGroup.alpha = elapsed <= RevealDuration
                ? 1f
                : Mathf.Clamp01(1f - ((elapsed - RevealDuration) / FadeDuration));
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
            if (!roulette.gameObject.activeSelf || !activeBoss.BannedValue.HasValue) return;
            var value = elapsed < RevealDuration - 0.45f
                ? Mathf.FloorToInt(elapsed * 12f) % 7
                : activeBoss.BannedValue.Value;
            roulette.text = $"BANNED: {value}";
        }
    }
}
