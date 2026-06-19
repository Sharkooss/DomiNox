using UnityEngine;

namespace DomiNox.Jackpot
{
    public sealed class JackpotSymbolViewData
    {
        public string Label { get; }
        public string DisplayName { get; }
        public Color Color { get; }

        public JackpotSymbolViewData(string label, string displayName, Color color)
        {
            Label = label;
            DisplayName = displayName;
            Color = color;
        }
    }

    public static class JackpotSymbolViewDataCatalog
    {
        public static JackpotSymbolViewData Get(JackpotSymbol symbol)
        {
            return symbol switch
            {
                JackpotSymbol.Seven => new JackpotSymbolViewData("7", "Seven", new Color(1f, 0.16f, 0.08f)),
                JackpotSymbol.Coin => new JackpotSymbolViewData("CO", "Coin", new Color(1f, 0.84f, 0.25f)),
                JackpotSymbol.Domino => new JackpotSymbolViewData("DM", "Domino", new Color(0.92f, 0.88f, 0.78f)),
                JackpotSymbol.Gem => new JackpotSymbolViewData("GM", "Gem", new Color(0.62f, 0.35f, 1f)),
                JackpotSymbol.DomiNex => new JackpotSymbolViewData("DX", "DomiNex", new Color(0.1f, 0.88f, 1f)),
                JackpotSymbol.Crown => new JackpotSymbolViewData("CR", "Crown", new Color(1f, 0.68f, 0.12f)),
                JackpotSymbol.Skull => new JackpotSymbolViewData("SK", "Skull", new Color(0.42f, 0.12f, 0.56f)),
                _ => new JackpotSymbolViewData("--", "Blank", new Color(0.52f, 0.56f, 0.62f))
            };
        }
    }
}
