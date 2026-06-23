namespace DomiNox.Objectives
{
    // A challenge that, when completed, unlocks a DomiNex (RewardDomiNexId).
    public sealed class ObjectiveDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public string Hint { get; }
        public bool Hidden { get; }
        public string RewardDomiNexId { get; }
        public ObjectiveType Type { get; }
        public int Threshold { get; }

        public ObjectiveDefinition(string id, string name, string hint, bool hidden, string rewardDomiNexId, ObjectiveType type, int threshold)
        {
            Id = id;
            Name = name;
            Hint = hint;
            Hidden = hidden;
            RewardDomiNexId = rewardDomiNexId;
            Type = type;
            Threshold = threshold;
        }

        // What the collection codex shows for a still-locked objective.
        public string DisplayHint => Hidden || string.IsNullOrWhiteSpace(Hint) ? "???" : Hint;
    }
}
