using System.Collections.Generic;
using DomiNox.Core;

namespace DomiNox.Dominoes
{
    public static class DominoFactory
    {
        public static List<DominoInstance> CreateDoubleSixSet()
        {
            var dominoes = new List<DominoInstance>();
            var index = 0;

            for (var left = GameConstants.DominoMinValue; left <= GameConstants.DominoMaxValue; left++)
            {
                for (var right = left; right <= GameConstants.DominoMaxValue; right++)
                {
                    var definition = new DominoDefinition($"domino_{left}_{right}", left, right);
                    dominoes.Add(new DominoInstance($"run_domino_{index++}", definition));
                }
            }

            return dominoes;
        }
    }
}
