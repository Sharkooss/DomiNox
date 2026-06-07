using DomiNox.Grid;

namespace DomiNox.Scoring
{
    public sealed class ScoringStep
    {
        public ScoringStepType Type { get; }
        public string SourceId { get; }
        public string SourceName { get; }
        public int CountDelta { get; }
        public int MultDelta { get; }
        public float MultiplierValue { get; }
        public string Description { get; }
        public int Order { get; }
        public PlacedDomino Domino { get; }
        public FloatingTextType FloatingTextType { get; }

        public ScoringStep(ScoringStepType type, string sourceId, string sourceName, int countDelta, int multDelta, float multiplierValue, string description, int order, PlacedDomino domino = null, FloatingTextType floatingTextType = FloatingTextType.Count)
        {
            Type = type;
            SourceId = sourceId;
            SourceName = sourceName;
            CountDelta = countDelta;
            MultDelta = multDelta;
            MultiplierValue = multiplierValue;
            Description = description;
            Order = order;
            Domino = domino;
            FloatingTextType = floatingTextType;
        }
    }
}
