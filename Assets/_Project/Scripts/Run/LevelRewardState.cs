namespace DomiNox.Run
{
    public sealed class LevelRewardState
    {
        public int BaseCredits { get; }
        public int DiscardCredits { get; }
        public int InterestCredits { get; }
        public int TotalCredits => BaseCredits + DiscardCredits + InterestCredits;

        public LevelRewardState(int baseCredits, int discardCredits, int interestCredits)
        {
            BaseCredits = baseCredits;
            DiscardCredits = discardCredits;
            InterestCredits = interestCredits;
        }
    }
}
