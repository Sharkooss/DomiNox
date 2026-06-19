using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DomiNox.Dominoes
{
    public static class DominoModifierRegistry
    {
        public const string BlueId = "blue";
        public const string RedId = "red";
        public const string GoldId = "gold";
        public const string GlassId = "glass";
        public const string LuckyId = "lucky";
        public const string JackpotId = "jackpot";
        public const string LightId = "light";

        public static IReadOnlyList<DominoModifierDefinition> All { get; } = new[]
        {
            Define(BlueId, "Blue Domino", "When scored, gives +15 Tile.", new Color(0.18f, 0.55f, 1f), "B"),
            Define(RedId, "Red Domino", "When scored, gives +3 Mult.", new Color(1f, 0.25f, 0.22f), "R"),
            Define(GoldId, "Gold Domino", "If played during a cleared level, gives +2 credits.", new Color(1f, 0.78f, 0.16f), "G"),
            Define(GlassId, "Glass Domino", "When scored, x2 hand score. 1 in 4 chance to break after scoring.", new Color(0.68f, 0.95f, 1f, 0.72f), "GL"),
            Define(LuckyId, "Lucky Domino", "When scored: 1 in 3 for +5 Mult, 1 in 10 for +30 Mult, 1 in 30 for +1 Spin, +100 Tile and +10 Mult.", new Color(0.55f, 1f, 0.32f), "L"),
            Define(JackpotId, "Jackpot Domino", "When scored, fills the Jackpot Meter by +10.", new Color(1f, 0.18f, 0.08f), "777"),
            Define(LightId, "Light Domino", "Does not count against the placed domino limit, but gives -2 Mult when scored.", new Color(0.85f, 1f, 1f), "LT")
        };

        public static DominoModifierDefinition GetById(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? null : All.FirstOrDefault(modifier => modifier.Id == id);
        }

        public static bool IsLight(DominoInstance domino)
        {
            return domino?.ModifierId == LightId;
        }

        private static DominoModifierDefinition Define(string id, string name, string description, Color color, string badge)
        {
            return new DominoModifierDefinition(id, name, description, color, badge);
        }
    }
}
