namespace DomiNox.Jackpot
{
    public sealed class JackpotSpinResult
    {
        public JackpotSymbol Symbol1 { get; }
        public JackpotSymbol Symbol2 { get; }
        public JackpotSymbol Symbol3 { get; }
        public JackpotRewardTier Tier { get; }
        public JackpotRewardDefinition Reward { get; }

        public JackpotSpinResult(JackpotSymbol symbol1, JackpotSymbol symbol2, JackpotSymbol symbol3, JackpotRewardTier tier, JackpotRewardDefinition reward)
        {
            Symbol1 = symbol1;
            Symbol2 = symbol2;
            Symbol3 = symbol3;
            Tier = tier;
            Reward = reward;
        }
    }
}
