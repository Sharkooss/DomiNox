using System;
using DomiNox.Core;

namespace DomiNox.Dominoes
{
    public readonly struct DominoValue : IEquatable<DominoValue>
    {
        public int Value { get; }

        public DominoValue(int value)
        {
            if (value < GameConstants.DominoMinValue || value > GameConstants.DominoMaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "A domino value must be between 0 and 6.");
            }

            Value = value;
        }

        public bool Equals(DominoValue other) => Value == other.Value;
        public override bool Equals(object obj) => obj is DominoValue other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }
}
