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
        private Image rarityBadge;
        private Outline outline;

        public void Initialize(string emptyText) => Initialize(emptyText, false);

        public void Initialize(string emptyText, bool compact)
        {
            var bg = gameObject.AddComponent<Image>();
            bg.color = DomiNoxTheme.BgCard;
            outline = DomiNoxTheme.AddOutline(gameObject, DomiNoxTheme.BorderNormal, compact ? 2f : 3.5f);

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = compact ? new RectOffset(8, 8, 7, 7) : new RectOffset(14, 14, 12, 12);
            layout.spacing = compact ? 4f : 8f;
            layout.childAlignment = TextAnchor.UpperCenter;

            nameText = UiFactory.CreateText(transform, "Name", emptyText, compact ? DomiNoxTheme.FontMD : DomiNoxTheme.FontLG, TextAnchor.MiddleCenter);
            nameText.color = DomiNoxTheme.TextPrimary;
            nameText.fontStyle = FontStyle.Bold;

            rarityBadge = new GameObject("RarityBadge", typeof(RectTransform), typeof(Image), typeof(LayoutElement)).GetComponent<Image>();
            rarityBadge.transform.SetParent(transform, false);
            rarityBadge.color = DomiNoxTheme.BorderNormal;
            rarityBadge.GetComponent<LayoutElement>().preferredHeight = compact ? 18f : 26f;
            rarityText = UiFactory.CreateText(rarityBadge.transform, "Rarity", string.Empty, compact ? DomiNoxTheme.FontXS : DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            rarityText.color = DomiNoxTheme.BgDeep;
            rarityText.fontStyle = FontStyle.Bold;
            Stretch(rarityText.rectTransform);

            descriptionText = UiFactory.CreateText(transform, "Description", string.Empty, compact ? DomiNoxTheme.FontXS : DomiNoxTheme.FontSM, TextAnchor.UpperLeft);
            descriptionText.color = DomiNoxTheme.TextSecondary;
            descriptionText.GetComponent<LayoutElement>().flexibleHeight = 1f;

            tagsText = UiFactory.CreateText(transform, "Tags", string.Empty, compact ? DomiNoxTheme.FontXS : DomiNoxTheme.FontSM, TextAnchor.LowerCenter);
            tagsText.color = DomiNoxTheme.TextMuted;

            Render(null, emptyText);
        }

        public void Render(DomiNexDefinition definition, string emptyText = "Hover a DomiNex")
            => Render(definition, null, emptyText);

        public void Render(DomiNexDefinition definition, RunState run, string emptyText = "Hover a DomiNex")
        {
            if (definition == null)
            {
                nameText.text = emptyText;
                rarityText.text = string.Empty;
                rarityBadge.color = DomiNoxTheme.BorderNormal;
                descriptionText.text = string.Empty;
                tagsText.text = string.Empty;
                outline.effectColor = DomiNoxTheme.BorderNormal;
                return;
            }

            var rarityColor = DomiNoxTheme.GetRarityColor(definition.Rarity);
            nameText.text = definition.Name;
            nameText.color = DomiNoxTheme.TextPrimary;
            rarityText.text = definition.Rarity.ToString().ToUpperInvariant();
            rarityText.color = DomiNoxTheme.BgDeep;
            rarityBadge.color = rarityColor;
            var dynamicText = definition.GetCurrentEffectText(run);
            descriptionText.text = string.IsNullOrWhiteSpace(dynamicText)
                ? definition.Description
                : $"{definition.Description}\n{dynamicText}";
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
