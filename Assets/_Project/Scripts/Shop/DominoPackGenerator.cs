using System;
using System.Collections.Generic;
using DomiNox.Core;
using DomiNox.Dominoes;

namespace DomiNox.Shop
{
    public sealed class DominoPackGenerator
    {
        private readonly Random random;
        private readonly DominoModifierRollService modifierRollService;

        public DominoPackGenerator(Random random = null)
        {
            this.random = random ?? new Random();
            modifierRollService = new DominoModifierRollService(this.random);
        }

        public List<DominoInstance> GenerateChoices(int count)
        {
            var choices = new List<DominoInstance>();
            for (var i = 0; i < count; i++)
            {
                choices.Add(GenerateDomino($"pack_domino_{Guid.NewGuid():N}"));
            }

            return choices;
        }

        public DominoInstance GenerateDomino(string instanceId)
        {
            var left = random.Next(GameConstants.DominoMinValue, GameConstants.DominoMaxValue + 1);
            var right = random.Next(GameConstants.DominoMinValue, GameConstants.DominoMaxValue + 1);
            if (left > right)
            {
                (left, right) = (right, left);
            }

            var modifierId = ChanceUtils.RollChance(1, 2) ? modifierRollService.RollModifier() : null;
            var definition = new DominoDefinition($"domino_{left}_{right}", left, right);
            return new DominoInstance(instanceId, definition, modifierId);
        }
    }
}
