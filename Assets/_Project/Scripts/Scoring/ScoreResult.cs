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

        public ScoreResult(int count, int mult, List<string> detectedPatterns, List<string> breakdownLines)
        {
            Count = count;
            Mult = mult;
            FinalScore = count * mult;
            DetectedPatterns = detectedPatterns;
            BreakdownLines = breakdownLines;
        }
    }
}
