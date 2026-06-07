using System.Collections.Generic;

namespace DomiNox.Scoring
{
    public sealed class ScoreResult
    {
        public int Count { get; }
        public int Mult { get; }
        public int FinalScore { get; }
        public List<string> DetectedPatterns { get; }
        public List<string> BreakdownLines { get; }
        public List<ScoringStep> Steps { get; }
        public List<PostScoringEffectResult> PostScoringEffects { get; }
        public string ValuePatternId { get; }
        public string ValuePatternName { get; }
        public int ValuePatternLevel { get; }
        public string DesignPatternId { get; }
        public string DesignPatternName { get; }
        public int DesignPatternLevel { get; }
        public PatternComboResult PatternCombo { get; }
        public bool HasPatternCombo => PatternCombo != null;
        public string ComboName => PatternCombo?.Name;
        public int ComboCountBonus => PatternCombo?.ComboCountBonus ?? 0;
        public int ComboMultBonus => PatternCombo?.ComboMultBonus ?? 0;

        public ScoreResult(int count, int mult, List<string> detectedPatterns, List<string> breakdownLines)
            : this(count, mult, count * mult, detectedPatterns, breakdownLines)
        {
        }

        public ScoreResult(int count, int mult, int finalScore, List<string> detectedPatterns, List<string> breakdownLines, string valuePatternId = null, string valuePatternName = null, int valuePatternLevel = 1, string designPatternId = null, string designPatternName = null, int designPatternLevel = 1, List<ScoringStep> steps = null, List<PostScoringEffectResult> postScoringEffects = null, PatternComboResult patternCombo = null)
        {
            Count = count;
            Mult = mult;
            FinalScore = finalScore;
            DetectedPatterns = detectedPatterns;
            BreakdownLines = breakdownLines;
            ValuePatternId = valuePatternId;
            ValuePatternName = valuePatternName;
            ValuePatternLevel = valuePatternLevel;
            DesignPatternId = designPatternId;
            DesignPatternName = designPatternName;
            DesignPatternLevel = designPatternLevel;
            Steps = steps ?? new List<ScoringStep>();
            PostScoringEffects = postScoringEffects ?? new List<PostScoringEffectResult>();
            PatternCombo = patternCombo;
        }

        public ScoreResult WithPostScoringEffects(List<PostScoringEffectResult> postScoringEffects)
        {
            return new ScoreResult(Count, Mult, FinalScore, DetectedPatterns, BreakdownLines, ValuePatternId, ValuePatternName, ValuePatternLevel, DesignPatternId, DesignPatternName, DesignPatternLevel, Steps, postScoringEffects, PatternCombo);
        }
    }
}
