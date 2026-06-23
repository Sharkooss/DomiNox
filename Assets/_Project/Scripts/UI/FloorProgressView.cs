using DomiNox.Bosses;
using DomiNox.Core;
using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class FloorProgressView : MonoBehaviour
    {
        private GameFlowController controller;
        private Text title;
        private Text subtitle;
        private Transform cardsRoot;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            var bg = gameObject.AddComponent<Image>();
            bg.color = DomiNoxTheme.BgPanel;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 20, 18);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            title = UiFactory.CreateText(transform, "Title", string.Empty, DomiNoxTheme.FontXL, TextAnchor.MiddleCenter);
            title.color = DomiNoxTheme.Gold;
            title.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.4f), new Vector2(1f, -1f));
            title.GetComponent<LayoutElement>().preferredHeight = 44f;

            subtitle = UiFactory.CreateText(transform, "Subtitle", "Choose your next table", DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            subtitle.color = DomiNoxTheme.TextSecondary;
            subtitle.GetComponent<LayoutElement>().preferredHeight = 26f;

            var cards = new GameObject("Cards", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            cards.transform.SetParent(transform, false);
            cardsRoot = cards.transform;
            cards.GetComponent<LayoutElement>().preferredHeight = 372f;
            var cardsLayout = cards.GetComponent<HorizontalLayoutGroup>();
            cardsLayout.spacing = 10f;
            cardsLayout.childAlignment = TextAnchor.MiddleCenter;
            cardsLayout.childForceExpandWidth = false;
            cardsLayout.childForceExpandHeight = true;

            var footer = UiFactory.CreateText(transform, "Footer", "The upcoming boss is revealed so you can plan your shop purchases.", DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            footer.color = DomiNoxTheme.TextMuted;
            footer.GetComponent<LayoutElement>().preferredHeight = 26f;
        }

        public void Render(RunState run)
        {
            var currentLevel = run.CurrentLevel.LevelIndex;
            var floor = run.CurrentLevel.FloorIndex;
            title.text = $"Floor {floor}";
            foreach (Transform child in cardsRoot) Destroy(child.gameObject);
            for (var levelInFloor = 1; levelInFloor <= GameConstants.LevelsPerFloor; levelInFloor++)
            {
                var globalLevel = ((floor - 1) * GameConstants.LevelsPerFloor) + levelInFloor;
                CreateLevelCard(run, globalLevel, levelInFloor, currentLevel);
            }
        }

        private void CreateLevelCard(RunState run, int globalLevel, int levelInFloor, int currentLevel)
        {
            var isBoss      = levelInFloor == GameConstants.LevelsPerFloor;
            var isCompleted = globalLevel < currentLevel;
            var isCurrent   = globalLevel == currentLevel;
            var boss        = run.CurrentFloorBoss;
            var cardBg      = GetCardColor(isBoss, isCompleted, isCurrent);

            var card = new GameObject($"LevelCard_{levelInFloor}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            card.transform.SetParent(cardsRoot, false);
            card.GetComponent<Image>().color = cardBg;
            card.GetComponent<LayoutElement>().preferredWidth  = 162f;
            card.GetComponent<LayoutElement>().preferredHeight = isBoss ? 368f : 338f;

            var borderColor = isCurrent ? DomiNoxTheme.Gold : isBoss ? DomiNoxTheme.MultRed : DomiNoxTheme.BorderNormal;
            var borderSize  = isCurrent ? 4f : 2f;
            DomiNoxTheme.AddOutline(card, borderColor, borderSize);
            if (isCurrent) DomiNoxTheme.AddShadow(card, DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.22f), new Vector2(0f, -3f));

            var layout = card.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 14, 14);
            layout.spacing = 9f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var headerColor = isBoss ? DomiNoxTheme.Danger : isCurrent ? DomiNoxTheme.Gold : DomiNoxTheme.TextSecondary;
            var headerText  = isBoss ? "BOSS" : $"Table {levelInFloor}";
            AddText(card.transform, headerText, DomiNoxTheme.FontLG, TextAnchor.MiddleCenter, headerColor, 30f, FontStyle.Bold);
            AddText(card.transform, GetTableName(isBoss, levelInFloor, boss), DomiNoxTheme.FontSM, TextAnchor.MiddleCenter, DomiNoxTheme.TextPrimary, 44f);
            AddText(card.transform, $"Score at least\n{controller.GetQuotaForLevel(globalLevel)}", DomiNoxTheme.FontSM, TextAnchor.MiddleCenter, DomiNoxTheme.TextSecondary, 46f);

            if (isBoss && boss != null)
            {
                UiFactory.CreateSeparator(card.transform, 1f, DomiNoxTheme.WithAlpha(DomiNoxTheme.Danger, 0.35f));
                AddText(card.transform, $"Rule:\n{GetBossRuleShort(boss)}", DomiNoxTheme.FontXS, TextAnchor.MiddleCenter, new Color(1f, 0.72f, 0.55f), 70f);
            }
            else
            {
                var tableType = levelInFloor == GameConstants.ClassicLevelsPerFloor ? "High Stakes" : "Classic";
                AddText(card.transform, tableType, DomiNoxTheme.FontXS, TextAnchor.MiddleCenter, DomiNoxTheme.TextMuted, 26f);
            }

            AddStatusOrButton(card.transform, isBoss, isCompleted, isCurrent);
        }

        private void AddStatusOrButton(Transform parent, bool isBoss, bool isCompleted, bool isCurrent)
        {
            if (isCompleted)
            {
                AddText(parent, "Cleared", DomiNoxTheme.FontMD, TextAnchor.MiddleCenter, DomiNoxTheme.Success, 54f, FontStyle.Bold);
                return;
            }

            if (isCurrent)
            {
                var button = UiFactory.CreateButton(parent, "PlayButton", isBoss ? "Play Boss" : "Play");
                button.GetComponent<LayoutElement>().preferredWidth  = 136f;
                button.GetComponent<LayoutElement>().preferredHeight = 48f;
                var bg  = isBoss ? new Color(0.40f, 0.08f, 0.06f) : new Color(0.08f, 0.22f, 0.12f);
                var col = isBoss ? DomiNoxTheme.Danger : DomiNoxTheme.Success;
                UiFactory.StyleButton(button, bg, col, 48f);
                button.onClick.AddListener(controller.StartCurrentLevel);
                return;
            }

            AddText(parent, isBoss ? "Win 4 tables\nfirst" : "Upcoming", DomiNoxTheme.FontXS, TextAnchor.MiddleCenter, DomiNoxTheme.TextMuted, 54f);
        }

        private Text AddText(Transform parent, string value, int size, TextAnchor anchor, Color color, float height, FontStyle style = FontStyle.Normal)
        {
            var text = UiFactory.CreateText(parent, "Text", value, size, anchor);
            text.color = color;
            text.fontStyle = style;
            text.GetComponent<LayoutElement>().preferredHeight = height;
            return text;
        }

        private static string GetTableName(bool isBoss, int levelInFloor, BossDefinition boss)
        {
            if (isBoss) return boss?.Name ?? "Boss Table";
            return levelInFloor == GameConstants.ClassicLevelsPerFloor ? "High Stakes Table" : "Classic Table";
        }

        private static string GetBossRuleShort(BossDefinition boss)
        {
            switch (boss.RuleType)
            {
                case BossRuleType.BannedValue:    return "One value is banned";
                case BossRuleType.ModifyDiscards: return "-1 discard";
                case BossRuleType.DisablePatterns:return "Doubles disabled";
                case BossRuleType.JackpotBoost:   return "7s score more";
                case BossRuleType.LockHandDominoes:return "Hand locks";
                default:                           return boss.Description;
            }
        }

        private static Color GetCardColor(bool isBoss, bool completed, bool current)
        {
            if (completed) return new Color(0.07f, 0.08f, 0.10f, 0.90f);
            if (current)   return isBoss ? new Color(0.16f, 0.05f, 0.04f, 0.98f) : new Color(0.10f, 0.14f, 0.20f, 0.98f);
            return isBoss ? new Color(0.11f, 0.05f, 0.05f, 0.80f) : DomiNoxTheme.BgPanel;
        }
    }
}
