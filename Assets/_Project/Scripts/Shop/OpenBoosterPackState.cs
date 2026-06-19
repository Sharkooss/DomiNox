using System.Collections.Generic;
using DomiNox.Consumables;
using DomiNox.Dominex;
using DomiNox.Dominoes;

namespace DomiNox.Shop
{
    public sealed class OpenBoosterPackState
    {
        public BoosterPackDefinition Pack { get; }
        public List<ConsumableDefinition> Choices { get; }
        public List<DomiNexDefinition> DomiNexChoices { get; }
        public List<DominoInstance> DominoChoices { get; }
        public HashSet<int> SelectedIndices { get; } = new HashSet<int>();
        public int ActualPickCount { get; }
        public int MinPickCount { get; }
        public int MaxPickCount => ActualPickCount;
        public bool IsOptionalPick => MinPickCount == 0;
        public int OfferIndex { get; }

        public OpenBoosterPackState(BoosterPackDefinition pack, List<ConsumableDefinition> choices, int actualPickCount, int offerIndex, int minPickCount = 1)
        {
            Pack = pack;
            Choices = choices ?? new List<ConsumableDefinition>();
            DomiNexChoices = new List<DomiNexDefinition>();
            DominoChoices = new List<DominoInstance>();
            ActualPickCount = actualPickCount;
            MinPickCount = minPickCount;
            OfferIndex = offerIndex;
        }

        public OpenBoosterPackState(BoosterPackDefinition pack, List<DomiNexDefinition> dominexChoices, int actualPickCount, int offerIndex, int minPickCount = 1)
        {
            Pack = pack;
            Choices = new List<ConsumableDefinition>();
            DomiNexChoices = dominexChoices ?? new List<DomiNexDefinition>();
            DominoChoices = new List<DominoInstance>();
            ActualPickCount = actualPickCount;
            MinPickCount = minPickCount;
            OfferIndex = offerIndex;
        }

        public OpenBoosterPackState(BoosterPackDefinition pack, List<DominoInstance> dominoChoices, int actualPickCount, int offerIndex, int minPickCount = 1)
        {
            Pack = pack;
            Choices = new List<ConsumableDefinition>();
            DomiNexChoices = new List<DomiNexDefinition>();
            DominoChoices = dominoChoices ?? new List<DominoInstance>();
            ActualPickCount = actualPickCount;
            MinPickCount = minPickCount;
            OfferIndex = offerIndex;
        }

        public int ChoiceCount => Pack.ContentType == BoosterPackContentType.DomiNex ? DomiNexChoices.Count : Pack.ContentType == BoosterPackContentType.Domino ? DominoChoices.Count : Choices.Count;
    }
}
