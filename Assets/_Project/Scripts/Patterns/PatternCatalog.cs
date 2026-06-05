using System.Collections.Generic;

namespace DomiNox.Patterns
{
    public static class PatternCatalog
    {
        public static IReadOnlyList<PatternInfo> All { get; } = new[]
        {
            new PatternInfo(PatternNames.DoublePlayed, "Jouer au moins 1 double.", "+2 Mult par double joue."),
            new PatternInfo(PatternNames.TripleDouble, "Jouer au moins 3 doubles.", "+8 Mult."),
            new PatternInfo(PatternNames.JackpotSeven, "Jouer au moins 3 dominos dont la somme vaut 7.", "+10 Mult."),
            new PatternInfo(PatternNames.FullNox, "Utiliser toute la limite de pose du niveau.", "+5 Mult."),
            new PatternInfo(PatternNames.HighStake, "Tous les dominos poses ont une somme de 8 ou plus.", "+30 Count.")
        };
    }
}
