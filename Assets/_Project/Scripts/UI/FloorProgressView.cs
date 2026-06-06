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
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.045f, 0.055f, 0.075f, 0.98f);

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 20, 18);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            title = UiFactory.CreateText(transform, "Title", string.Empty, 30, TextAnchor.MiddleCenter);
            title.color = new Color(0.98f, 0.84f, 0.34f);
            title.GetComponent<LayoutElement>().preferredHeight = 42f;

            subtitle = UiFactory.CreateText(transform, "Subtitle", "Choose your next table", 17, TextAnchor.MiddleCenter);
            subtitle.color = new Color(0.72f, 0.78f, 0.86f);
            subtitle.GetComponent<LayoutElement>().preferredHeight = 28f;

            var cards = new GameObject("Cards", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            cards.transform.SetParent(transform, false);
            cardsRoot = cards.transform;
            cards.GetComponent<LayoutElement>().preferredHeight = 372f;
            var cardsLayout = cards.GetComponent<HorizontalLayoutGroup>();
            cardsLayout.spacing = 10f;
            cardsLayout.childAlignment = TextAnchor.MiddleCenter;
            cardsLayout.childForceExpandWidth = false;
            cardsLayout.childForceExpandHeight = true;

            var footer = UiFactory.CreateText(transform, "Footer", "Le boss est visible a l'avance pour preparer tes achats au shop.", 14, TextAnchor.MiddleCenter);
            footer.color = new Color(0.64f, 0.7f, 0.78f);
            footer.GetComponent<LayoutElement>().preferredHeight = 28f;
        }

        public void Render(RunState run)
        {
            var currentLevel = run.CurrentLevel.LevelIndex;
            var floor = run.CurrentLevel.FloorIndex;
            title.text = $"Floor {floor}";
            subtitle.text = "Choose your next table";

            foreach (Transform child in cardsRoot)
            {
                Destroy(child.gameObject);
            }

            for (var levelInFloor = 1; levelInFloor <= GameConstants.LevelsPerFloor; levelInFloor++)
            {
                var globalLevel = ((floor - 1) * GameConstants.LevelsPerFloor) + levelInFloor;
                CreateLevelCard(run, globalLevel, levelInFloor, currentLevel);
            }
        }

        private void CreateLevelCard(RunState run, int globalLevel, int levelInFloor, int currentLevel)
        {
            var isBoss = levelInFloor == GameConstants.LevelsPerFloor;
            var isCompleted = globalLevel < currentLevel;
            var isCurrent = globalLevel == currentLevel;
            var boss = run.CurrentFloorBoss;
            var cardColor = GetCardColor(isBoss, isCompleted, isCurrent);

            var card = new GameObject($"LevelCard_{levelInFloor}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            card.transform.SetParent(cardsRoot, false);
            card.GetComponent<Image>().color = cardColor;
            card.GetComponent<LayoutElement>().preferredWidth = 158f;
            card.GetComponent<LayoutElement>().preferredHeight = isBoss ? 366f : 336f;
            var outline = card.GetComponent<Outline>();
            outline.effectColor = isCurrent ? new Color(1f, 0.83f, 0.32f) : isBoss ? new Color(0.92f, 0.28f, 0.18f) : new Color(0.18f, 0.24f, 0.32f);
            outline.effectDistance = isCurrent ? new Vector2(4f, -4f) : new Vector2(2f, -2f);

            var layout = card.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 14, 14);
            layout.spacing = 9f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddText(card.transform, isBoss ? "BOSS" : $"Table {levelInFloor}", 21, TextAnchor.MiddleCenter, isBoss ? new Color(1f, 0.46f, 0.3f) : new Color(0.96f, 0.86f, 0.48f), 32f);
            AddText(card.transform, GetTableName(isBoss, levelInFloor, boss), isBoss ? 15 : 14, TextAnchor.MiddleCenter, Color.white, 46f);
            AddText(card.transform, $"Score at least\n{controller.GetQuotaForLevel(globalLevel)}", 14, TextAnchor.MiddleCenter, new Color(0.82f, 0.88f, 0.96f), 48f);

            if (isBoss && boss != null)
            {
                AddText(card.transform, $"Rule:\n{GetBossRuleShort(boss)}", 12, TextAnchor.MiddleCenter, new Color(1f, 0.77f, 0.62f), 72f);
                AddText(card.transform, "Reward:\nBoss reward", 13, TextAnchor.MiddleCenter, new Color(0.74f, 0.94f, 0.72f), 42f);
            }
            else
            {
                AddText(card.transform, levelInFloor == GameConstants.ClassicLevelsPerFloor ? "High Stakes" : "Classic", 13, TextAnchor.MiddleCenter, new Color(0.7f, 0.76f, 0.84f), 28f);
                AddText(card.transform, levelInFloor == GameConstants.ClassicLevelsPerFloor ? "Reward:\n$6+" : "Reward:\n$5+", 13, TextAnchor.MiddleCenter, new Color(0.74f, 0.94f, 0.72f), 42f);
            }

            AddStatusOrButton(card.transform, isBoss, isCompleted, isCurrent);
        }

        private void AddStatusOrButton(Transform parent, bool isBoss, bool isCompleted, bool isCurrent)
        {
            if (isCompleted)
            {
                AddText(parent, "OK\nCleared", 18, TextAnchor.MiddleCenter, new Color(0.62f, 0.95f, 0.62f), 58f);
                return;
            }

            if (isCurrent)
            {
                var button = UiFactory.CreateButton(parent, "PlayButton", isBoss ? "Play Boss" : "Play");
                button.GetComponent<LayoutElement>().preferredWidth = 132f;
                button.GetComponent<LayoutElement>().preferredHeight = 46f;
                button.GetComponent<Image>().color = isBoss ? new Color(0.58f, 0.16f, 0.12f) : new Color(0.24f, 0.34f, 0.5f);
                button.onClick.AddListener(controller.StartCurrentLevel);
                return;
            }

            AddText(parent, isBoss ? "Defeat all 4\ntables first" : "Upcoming", 13, TextAnchor.MiddleCenter, new Color(0.52f, 0.58f, 0.66f), 58f);
        }

        private Text AddText(Transform parent, string value, int size, TextAnchor anchor, Color color, float height)
        {
            var text = UiFactory.CreateText(parent, "Text", value, size, anchor);
            text.color = color;
            text.GetComponent<LayoutElement>().preferredHeight = height;
            return text;
        }

        private static string GetTableName(bool isBoss, int levelInFloor, BossDefinition boss)
        {
            if (isBoss)
            {
                return boss?.Name ?? "Boss Table";
            }

            return levelInFloor == GameConstants.ClassicLevelsPerFloor ? "High Stakes Table" : "Classic Table";
        }

        private static string GetBossRuleShort(BossDefinition boss)
        {
            switch (boss.RuleType)
            {
                case BossRuleType.BannedValue:
                    return "One value is banned";
                case BossRuleType.ModifyDiscards:
                    return "-1 discard";
                case BossRuleType.DisablePatterns:
                    return "Doubles disabled";
                case BossRuleType.JackpotBoost:
                    return "7s score more";
                case BossRuleType.LockHandDominoes:
                    return "Hand locks";
                default:
                    return boss.Description;
            }
        }

        private static Color GetCardColor(bool isBoss, bool completed, bool current)
        {
            if (completed)
            {
                return new Color(0.07f, 0.085f, 0.1f, 0.92f);
            }

            if (current)
            {
                return isBoss ? new Color(0.22f, 0.07f, 0.06f, 0.98f) : new Color(0.12f, 0.16f, 0.23f, 0.98f);
            }

            return isBoss ? new Color(0.12f, 0.055f, 0.055f, 0.82f) : new Color(0.075f, 0.09f, 0.12f, 0.82f);
        }
    }
}
