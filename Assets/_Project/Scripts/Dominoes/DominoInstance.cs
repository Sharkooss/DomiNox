using System;

namespace DomiNox.Dominoes
{
    public sealed class DominoInstance
    {
        public string InstanceId { get; }
        public DominoDefinition Definition { get; }
        public string ModifierId { get; }

        public DominoInstance(string instanceId, DominoDefinition definition)
            : this(instanceId, definition, null)
        {
        }

        public DominoInstance(string instanceId, DominoDefinition definition, string modifierId)
        {
            InstanceId = string.IsNullOrWhiteSpace(instanceId) ? throw new ArgumentException("Instance id is required.", nameof(instanceId)) : instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            ModifierId = string.IsNullOrWhiteSpace(modifierId) ? null : modifierId;
        }

        public override string ToString() => Definition.ToString();
    }
}
