using System.Collections.Generic;

namespace DomiNox.Dominex
{
    public sealed class DomiNexScoringContext
    {
        public IReadOnlyList<DomiNexDefinition> ActiveDomiNex { get; }
        public int Credits { get; }
        public int DiscardsUsed { get; }
        public int MaxPlacedDominoes { get; }

        public DomiNexScoringContext(IReadOnlyList<DomiNexDefinition> activeDomiNex, int credits, int discardsUsed, int maxPlacedDominoes)
        {
            ActiveDomiNex = activeDomiNex;
            Credits = credits;
            DiscardsUsed = discardsUsed;
            MaxPlacedDominoes = maxPlacedDominoes;
        }
    }
}
