namespace DomiNox.Bosses
{
    public sealed class BossDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string IntroMessage { get; }
        public float QuotaMultiplier { get; }
        public BossRuleType RuleType { get; }
        public int BannedValueCount { get; }
        public int DiscardsDelta { get; }
        public string[] DisabledPatternIds { get; }
        public int LockedDominoCount { get; }
        public int SevenDominoCountBonus { get; }
        public int SevenDominoMultBonus { get; }
        public float JackpotFinalScoreMultiplier { get; }

        public BossDefinition(
            string id,
            string name,
            string description,
            string introMessage,
            float quotaMultiplier,
            BossRuleType ruleType,
            int bannedValueCount = 0,
            int discardsDelta = 0,
            string[] disabledPatternIds = null,
            int lockedDominoCount = 0,
            int sevenDominoCountBonus = 0,
            int sevenDominoMultBonus = 0,
            float jackpotFinalScoreMultiplier = 1f)
        {
            Id = id;
            Name = name;
            Description = description;
            IntroMessage = introMessage;
            QuotaMultiplier = quotaMultiplier;
            RuleType = ruleType;
            BannedValueCount = bannedValueCount;
            DiscardsDelta = discardsDelta;
            DisabledPatternIds = disabledPatternIds ?? new string[0];
            LockedDominoCount = lockedDominoCount;
            SevenDominoCountBonus = sevenDominoCountBonus;
            SevenDominoMultBonus = sevenDominoMultBonus;
            JackpotFinalScoreMultiplier = jackpotFinalScoreMultiplier;
        }
    }
}
