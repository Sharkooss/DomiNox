namespace DomiNox.Bosses
{
    public static class BossRegistry
    {
        public static BossDefinition[] DemoBosses { get; } =
        {
            new BossDefinition(
                "one_armed_dealer",
                "Le Croupier Manchot",
                "Une valeur entre 0 et 6 est bannie. Les dominos contenant cette valeur ne peuvent pas etre poses.",
                "Une valeur disparait de la table.",
                1.25f,
                BossRuleType.BannedValue,
                bannedValueCount: 1),
            new BossDefinition(
                "red_table",
                "La Table Rouge",
                "Tu commences ce niveau avec 1 discard en moins.",
                "Cette table ne pardonne pas l'hesitation.",
                1.20f,
                BossRuleType.ModifyDiscards,
                discardsDelta: -1),
            new BossDefinition(
                "double_widow",
                "La Veuve des Doubles",
                "Les patterns Double, Double Pair et Triple Double ne donnent aucun bonus pendant ce niveau.",
                "Les doubles perdent leur pouvoir.",
                1.15f,
                BossRuleType.DisablePatterns,
                disabledPatternIds: new[] { "double_tile", "triple_double" }),
            new BossDefinition(
                "slot_machine_777",
                "La Machine 777",
                "Les dominos dont la somme vaut 7 donnent +7 Count et +1 Mult. Jackpot 7 donne x1.25 score final.",
                "Aligne les 7. La machine paie gros.",
                1.50f,
                BossRuleType.JackpotBoost,
                sevenDominoCountBonus: 7,
                sevenDominoMultBonus: 1,
                jackpotFinalScoreMultiplier: 1.25f),
            new BossDefinition(
                "locked_shoe",
                "Le Sabot Verrouille",
                "Deux dominos de ta main de depart sont verrouilles et ne peuvent pas etre defausses.",
                "Le sabot choisit ce que tu gardes.",
                1.20f,
                BossRuleType.LockHandDominoes,
                lockedDominoCount: 2),
            new BossDefinition(
                "the_tightrope",
                "Le Funambule",
                "Tu commences ce niveau avec 2 discards en moins.",
                "Aucun filet sous cette table.",
                1.30f,
                BossRuleType.ModifyDiscards,
                discardsDelta: -2),
            new BossDefinition(
                "line_breaker",
                "Le Brise-Ligne",
                "Les patterns Tile Line et Cross Tile ne donnent aucun bonus pendant ce niveau.",
                "Les lignes se brisent ici.",
                1.20f,
                BossRuleType.DisablePatterns,
                disabledPatternIds: new[] { "tile_line", "cross_tile" })
        };

        public static BossDefinition GetById(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? null : System.Array.Find(DemoBosses, boss => boss.Id == id);
        }

        public static BossDefinition GetForLevel(int levelIndex, System.Random random = null, string previousBossId = null)
        {
            if (levelIndex <= 0 || levelIndex % 5 != 0)
            {
                return null;
            }

            if (random == null)
            {
                return DemoBosses[((levelIndex / 5) - 1) % DemoBosses.Length];
            }

            return GetRandom(random, previousBossId);
        }

        public static BossDefinition GetRandom(System.Random random, string previousBossId = null)
        {
            var candidates = DemoBosses;
            if (!string.IsNullOrWhiteSpace(previousBossId) && DemoBosses.Length > 1)
            {
                candidates = System.Array.FindAll(DemoBosses, boss => boss.Id != previousBossId);
            }

            return candidates[(random ?? new System.Random()).Next(0, candidates.Length)];
        }
    }
}
