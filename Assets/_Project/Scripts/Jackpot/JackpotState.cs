using System.Collections.Generic;

namespace DomiNox.Jackpot
{
    public sealed class JackpotState
    {
        public int Meter { get; set; }
        public int MaxMeter { get; set; } = 100;
        public int SpinTickets { get; set; }
        public int JackpotLuck { get; set; }
        public int MachineHeat { get; set; }
        public int JackpotMeterMultiplierLevelsRemaining { get; set; }
        public float JackpotMeterMultiplier { get; set; } = 1f;
        public List<JackpotGainLine> LastGainBreakdown { get; } = new List<JackpotGainLine>();
    }

    public sealed class JackpotGainLine
    {
        public JackpotGainSource Source { get; }
        public int BaseAmount { get; }
        public int AppliedAmount { get; }

        public JackpotGainLine(JackpotGainSource source, int baseAmount, int appliedAmount)
        {
            Source = source;
            BaseAmount = baseAmount;
            AppliedAmount = appliedAmount;
        }
    }
}
