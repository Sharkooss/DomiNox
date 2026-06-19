namespace DomiNox.Dominoes
{
    public sealed class DominoModifierEffectResult
    {
        public string DominoInstanceId { get; }
        public string ModifierId { get; }
        public string ModifierName { get; }
        public int CountDelta { get; }
        public int MultDelta { get; }
        public float FinalScoreMultiplier { get; }
        public int JackpotMeterGain { get; }
        public int SpinTicketGain { get; }
        public bool GlassBroken { get; }
        public string Description { get; }

        public DominoModifierEffectResult(string dominoInstanceId, string modifierId, string modifierName, int countDelta = 0, int multDelta = 0, float finalScoreMultiplier = 1f, int jackpotMeterGain = 0, int spinTicketGain = 0, bool glassBroken = false, string description = null)
        {
            DominoInstanceId = dominoInstanceId;
            ModifierId = modifierId;
            ModifierName = modifierName;
            CountDelta = countDelta;
            MultDelta = multDelta;
            FinalScoreMultiplier = finalScoreMultiplier;
            JackpotMeterGain = jackpotMeterGain;
            SpinTicketGain = spinTicketGain;
            GlassBroken = glassBroken;
            Description = description ?? modifierName;
        }
    }
}
