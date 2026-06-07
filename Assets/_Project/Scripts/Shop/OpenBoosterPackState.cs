using System.Collections.Generic;
using DomiNox.Consumables;

namespace DomiNox.Shop
{
    public sealed class OpenBoosterPackState
    {
        public BoosterPackDefinition Pack { get; }
        public List<ConsumableDefinition> Choices { get; }
        public HashSet<int> SelectedIndices { get; } = new HashSet<int>();
        public int ActualPickCount { get; }
        public int OfferIndex { get; }

        public OpenBoosterPackState(BoosterPackDefinition pack, List<ConsumableDefinition> choices, int actualPickCount, int offerIndex)
        {
            Pack = pack;
            Choices = choices;
            ActualPickCount = actualPickCount;
            OfferIndex = offerIndex;
        }
    }
}
