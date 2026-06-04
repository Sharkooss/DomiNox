using System;

namespace DomiNox.Dominoes
{
    public sealed class DominoInstance
    {
        public string InstanceId { get; }
        public DominoDefinition Definition { get; }

        public DominoInstance(string instanceId, DominoDefinition definition)
        {
            InstanceId = string.IsNullOrWhiteSpace(instanceId) ? throw new ArgumentException("Instance id is required.", nameof(instanceId)) : instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public override string ToString() => Definition.ToString();
    }
}
