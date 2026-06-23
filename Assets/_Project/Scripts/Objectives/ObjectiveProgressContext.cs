namespace DomiNox.Objectives
{
    // Carries the run facts an objective might check at a given evaluation moment.
    // Unfilled fields stay at their defaults, so objectives that don't apply simply won't trigger.
    public sealed class ObjectiveProgressContext
    {
        public int FloorReached { get; set; }
        public int HandScore { get; set; }
        public bool WonBossWithoutDiscard { get; set; }
        public int DesignPatternsThisHand { get; set; }
        public int OwnedDomiNexCount { get; set; }
    }
}
