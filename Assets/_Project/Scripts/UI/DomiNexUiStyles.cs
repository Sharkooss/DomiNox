using DomiNox.Dominex;
using UnityEngine;

namespace DomiNox.UI
{
    public static class DomiNexUiStyles
    {
        public static Color GetRarityColor(DomiNexRarity rarity)
        {
            return rarity switch
            {
                DomiNexRarity.Common => new Color(0.78f, 0.82f, 0.88f),
                DomiNexRarity.Rare => new Color(0.25f, 0.55f, 1f),
                DomiNexRarity.Epic => new Color(0.72f, 0.35f, 1f),
                DomiNexRarity.Legendary => new Color(1f, 0.72f, 0.18f),
                DomiNexRarity.Cursed => new Color(0.9f, 0.12f, 0.16f),
                _ => Color.white
            };
        }
    }
}
