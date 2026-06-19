using System.Collections.Generic;

namespace DomiNox.Jackpot
{
    public sealed class MajorJackpotState
    {
        public List<JackpotRewardDefinition> Options { get; }
        public HashSet<int> SelectedIndices { get; } = new HashSet<int>();

        public MajorJackpotState(List<JackpotRewardDefinition> options)
        {
            Options = options;
        }
    }
}
