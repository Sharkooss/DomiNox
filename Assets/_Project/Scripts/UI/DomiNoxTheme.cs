using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public static class DomiNoxTheme
    {
        // --- Backgrounds ---
        public static readonly Color BgDeep    = new Color(0.04f, 0.05f, 0.08f);
        public static readonly Color BgPanel   = new Color(0.07f, 0.09f, 0.13f);
        public static readonly Color BgCard    = new Color(0.10f, 0.13f, 0.18f);
        public static readonly Color BgOverlay = new Color(0.04f, 0.05f, 0.08f, 0.97f);

        // --- Accents ---
        public static readonly Color Gold          = new Color(1.00f, 0.82f, 0.28f);
        public static readonly Color GoldDim       = new Color(0.78f, 0.62f, 0.18f);
        public static readonly Color IvoryWhite    = new Color(0.98f, 0.95f, 0.88f);
        public static readonly Color IvoryDark     = new Color(0.16f, 0.13f, 0.08f);

        // --- Count (blue) & Mult (red) ---
        public static readonly Color CountBlue    = new Color(0.08f, 0.62f, 1.00f);
        public static readonly Color CountBlueDim = new Color(0.04f, 0.30f, 0.65f);
        public static readonly Color MultRed      = new Color(1.00f, 0.22f, 0.16f);
        public static readonly Color MultRedDim   = new Color(0.60f, 0.08f, 0.05f);

        // --- Text ---
        public static readonly Color TextPrimary   = new Color(0.96f, 0.94f, 0.90f);
        public static readonly Color TextSecondary = new Color(0.68f, 0.74f, 0.82f);
        public static readonly Color TextMuted     = new Color(0.40f, 0.46f, 0.54f);

        // --- Borders ---
        public static readonly Color BorderNormal = new Color(0.22f, 0.28f, 0.38f);
        public static readonly Color BorderActive = new Color(1.00f, 0.82f, 0.28f);
        public static readonly Color BorderCount  = new Color(0.08f, 0.62f, 1.00f);
        public static readonly Color BorderMult   = new Color(1.00f, 0.22f, 0.16f);

        // --- Jackpot ---
        public static readonly Color JackpotAmber  = new Color(1.00f, 0.65f, 0.10f);
        public static readonly Color JackpotViolet = new Color(0.72f, 0.20f, 1.00f);
        public static readonly Color JackpotCyan   = new Color(0.05f, 0.85f, 1.00f);
        public static readonly Color JackpotBg     = new Color(0.05f, 0.02f, 0.08f);

        // --- States ---
        public static readonly Color Success  = new Color(0.18f, 0.82f, 0.42f);
        public static readonly Color Danger   = new Color(1.00f, 0.28f, 0.16f);
        public static readonly Color Warning  = new Color(1.00f, 0.75f, 0.10f);
        public static readonly Color Disabled = new Color(0.35f, 0.40f, 0.46f);

        // --- Rarity colors ---
        public static readonly Color RarityCommon    = new Color(0.75f, 0.80f, 0.88f);
        public static readonly Color RarityRare      = new Color(0.22f, 0.58f, 1.00f);
        public static readonly Color RarityEpic      = new Color(0.78f, 0.32f, 1.00f);
        public static readonly Color RarityLegendary = new Color(1.00f, 0.72f, 0.12f);
        public static readonly Color RarityCursed    = new Color(0.95f, 0.10f, 0.18f);

        // --- Font sizes ---
        public const int FontXS  = 11;
        public const int FontSM  = 13;
        public const int FontMD  = 16;
        public const int FontLG  = 20;
        public const int FontXL  = 26;
        public const int FontXXL = 36;

        // --- Helpers ---
        public static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);

        public static Shadow AddShadow(GameObject go, Color? color = null, Vector2? distance = null)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = color ?? new Color(0f, 0f, 0f, 0.55f);
            shadow.effectDistance = distance ?? new Vector2(2f, -2f);
            return shadow;
        }

        public static Outline AddOutline(GameObject go, Color color, float size = 2f)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(size, -size);
            return outline;
        }

        public static DomiNox.Dominex.DomiNexRarity[] AllRarities =>
            (DomiNox.Dominex.DomiNexRarity[])System.Enum.GetValues(typeof(DomiNox.Dominex.DomiNexRarity));

        public static Color GetRarityColor(DomiNox.Dominex.DomiNexRarity rarity)
        {
            switch (rarity)
            {
                case DomiNox.Dominex.DomiNexRarity.Common:    return RarityCommon;
                case DomiNox.Dominex.DomiNexRarity.Rare:      return RarityRare;
                case DomiNox.Dominex.DomiNexRarity.Epic:      return RarityEpic;
                case DomiNox.Dominex.DomiNexRarity.Legendary: return RarityLegendary;
                case DomiNox.Dominex.DomiNexRarity.Cursed:    return RarityCursed;
                default: return Color.white;
            }
        }
    }
}
