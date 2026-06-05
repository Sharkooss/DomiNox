using System.Collections.Generic;

namespace DomiNox.Patterns
{
    public static class PatternCatalog
    {
        public static IReadOnlyList<PatternInfo> ValuePatterns { get; } = new[]
        {
            new PatternInfo(PatternNames.TripleDouble, PatternCategory.Value, "Jouer 3 doubles ou plus.", "+80 Chips, +4 Mult.", 80, 4, 90, "D..", ".D.", "..D"),
            new PatternInfo(PatternNames.BigStraight, PatternCategory.Value, "Jouer une suite connectee de 5 valeurs consecutives.", "+80 Chips, +5 Mult.", 80, 5, 80, "123", ".45", "..6"),
            new PatternInfo(PatternNames.JackpotSeven, PatternCategory.Value, "Jouer au moins 3 dominos dont la somme vaut 7.", "+70 Chips, +4 Mult.", 70, 4, 70, "7.7", ".7.", "..."),
            new PatternInfo(PatternNames.DoublePair, PatternCategory.Value, "Jouer 2 doubles dans la meme validation.", "+45 Chips, +2 Mult.", 45, 2, 60, "D.D", "...", "..."),
            new PatternInfo(PatternNames.SameValue, PatternCategory.Value, "Jouer au moins 3 dominos qui contiennent la meme valeur.", "+40 Chips, +3 Mult.", 40, 3, 50, "4.4", ".4.", "..."),
            new PatternInfo(PatternNames.SmallStraight, PatternCategory.Value, "Jouer une suite connectee de 3 valeurs consecutives.", "+35 Chips, +2 Mult.", 35, 2, 40, "123", "...", "..."),
            new PatternInfo(PatternNames.DoublePlayed, PatternCategory.Value, "Jouer au moins 1 double.", "+25 Chips, +1 Mult.", 25, 1, 30, ".D.", "...", "..."),
            new PatternInfo(PatternNames.PairLink, PatternCategory.Value, "Jouer au moins 2 dominos qui partagent une valeur.", "+20 Chips, +1 Mult.", 20, 1, 20, "2-2", "...", "..."),
            new PatternInfo(PatternNames.LowRoll, PatternCategory.Value, "Tous les dominos joues ont une somme de 5 ou moins.", "+20 Chips, +4 Mult.", 20, 4, 15, "lo.", ".lo", "..."),
            new PatternInfo(PatternNames.HighTile, PatternCategory.Value, "Pattern par defaut si aucun autre pattern de valeur n'est retenu.", "+10 Chips.", 10, 0, 0, ".H.", "...", "...")
        };

        public static IReadOnlyList<PatternInfo> DesignPatterns { get; } = new[]
        {
            new PatternInfo(PatternNames.Loop, PatternCategory.Design, "Les cases jouees forment une boucle fermee.", "+80 Chips, +5 Mult.", 80, 5, 80, "XXX", "X.X", "XXX"),
            new PatternInfo(PatternNames.Snake, PatternCategory.Design, "La forme change de direction au moins 2 fois sans se couper.", "+25 Chips, +2 Mult.", 25, 2, 50, "XX.", ".XX", "XX."),
            new PatternInfo(PatternNames.Corner, PatternCategory.Design, "La forme fait exactement un angle a 90 degres.", "+2 Mult.", 0, 2, 30, "X..", "X..", "XXX"),
            new PatternInfo(PatternNames.Line, PatternCategory.Design, "Toutes les cases jouees forment une ligne horizontale ou verticale.", "+20 Chips.", 20, 0, 20, "XXX", "...", "...")
        };

        public static IReadOnlyList<PatternInfo> BonusPatterns { get; } = new[]
        {
            new PatternInfo(PatternNames.NoxHand, PatternCategory.Bonus, "Utiliser exactement la limite de pose du niveau.", "+50 Chips, +3 Mult.", 50, 3, 10, "MAX", "...", "...")
        };

        public static IReadOnlyList<PatternInfo> All { get; } = BuildAll();

        public static PatternInfo GetByName(string name)
        {
            foreach (var pattern in All)
            {
                if (pattern.Name == name)
                {
                    return pattern;
                }
            }

            return null;
        }

        private static IReadOnlyList<PatternInfo> BuildAll()
        {
            var all = new List<PatternInfo>();
            all.AddRange(ValuePatterns);
            all.AddRange(DesignPatterns);
            all.AddRange(BonusPatterns);
            return all;
        }
    }
}
