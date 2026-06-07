using DomiNox.Dominex;
using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class DomiNexDetailCardView : MonoBehaviour
    {
        private Text nameText;
        private Text rarityText;
        private Text descriptionText;
        private Text tagsText;
        private Image rarityBackground;
        private Outline outline;

        public void Initialize(string emptyText)
        {
            Initialize(emptyText, false);
        }

        public void Initialize(string emptyText, bool compact)
        {
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.1f, 0.13f, 0.98f);
            outline = gameObject.AddComponent<Outline>();
            outline.effectDistance = compact ? new Vector2(2f, -2f) : new Vector2(4f, -4f);
            outline.effectColor = new Color(0.35f, 0.38f, 0.44f);

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = compact ? new RectOffset(8, 8, 7, 7) : new RectOffset(12, 12, 12, 12);
            layout.spacing = compact ? 4f : 8f;
            layout.childAlignment = TextAnchor.UpperCenter;

            nameText = UiFactory.CreateText(transform, "Name", emptyText, compact ? 15 : 20, TextAnchor.MiddleCenter);
            rarityBackground = new GameObject("RarityBadge", typeof(RectTransform), typeof(Image), typeof(LayoutElement)).GetComponent<Image>();
            rarityBackground.transform.SetParent(transform, false);
            rarityBackground.GetComponent<LayoutElement>().preferredHeight = compact ? 18f : 28f;
            rarityText = UiFactory.CreateText(rarityBackground.transform, "Rarity", string.Empty, compact ? 10 : 14, TextAnchor.MiddleCenter);
            rarityText.color = Color.black;
            Stretch(rarityText.rectTransform);

            descriptionText = UiFactory.CreateText(transform, "Description", string.Empty, compact ? 11 : 14, TextAnchor.UpperLeft);
            descriptionText.GetComponent<LayoutElement>().flexibleHeight = 1f;
            tagsText = UiFactory.CreateText(transform, "Tags", string.Empty, compact ? 10 : 12, TextAnchor.LowerCenter);
            tagsText.color = new Color(0.65f, 0.72f, 0.8f);

            Render(null, emptyText);
        }

        public void Render(DomiNexDefinition definition, string emptyText = "Survole un DomiNex")
        {
            Render(definition, null, emptyText);
        }

        public void Render(DomiNexDefinition definition, RunState run, string emptyText = "Survole un DomiNex")
        {
            if (definition == null)
            {
                nameText.text = emptyText;
                rarityText.text = string.Empty;
                rarityBackground.color = new Color(0.25f, 0.27f, 0.32f);
                descriptionText.text = "";
                tagsText.text = "";
                outline.effectColor = new Color(0.35f, 0.38f, 0.44f);
                return;
            }

            var rarityColor = DomiNexUiStyles.GetRarityColor(definition.Rarity);
            nameText.text = definition.Name;
            rarityText.text = definition.Rarity.ToString().ToUpperInvariant();
            rarityBackground.color = rarityColor;
            var dynamicText = definition.GetCurrentEffectText(run);
            descriptionText.text = string.IsNullOrWhiteSpace(dynamicText) ? definition.Description : $"{definition.Description}\n{dynamicText}";
            tagsText.text = definition.Tags.Count == 0 ? string.Empty : string.Join("  •  ", definition.Tags);
            outline.effectColor = rarityColor;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
