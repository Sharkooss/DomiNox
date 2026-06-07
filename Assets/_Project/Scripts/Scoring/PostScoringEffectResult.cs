namespace DomiNox.Scoring
{
    public sealed class PostScoringEffectResult
    {
        public string SourceId { get; }
        public string SourceName { get; }
        public string Description { get; }
        public bool Triggered { get; }
        public string PatternId { get; }

        public PostScoringEffectResult(string sourceId, string sourceName, string description, bool triggered, string patternId = null)
        {
            SourceId = sourceId;
            SourceName = sourceName;
            Description = description;
            Triggered = triggered;
            PatternId = patternId;
        }
    }
}
