namespace DomiNox.Dominex
{
    public sealed class DomiNexEffectDefinition
    {
        public DomiNexTrigger Trigger { get; }
        public DomiNexEffectType Type { get; }
        public int Value { get; }
        public int Threshold { get; }
        public string Note { get; }

        public DomiNexEffectDefinition(DomiNexTrigger trigger, DomiNexEffectType type, int value = 0, int threshold = 0, string note = "")
        {
            Trigger = trigger;
            Type = type;
            Value = value;
            Threshold = threshold;
            Note = note;
        }
    }
}
