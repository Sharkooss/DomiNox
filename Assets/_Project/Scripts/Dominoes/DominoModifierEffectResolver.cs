using System.Collections.Generic;
using DomiNox.Core;
using DomiNox.Grid;

namespace DomiNox.Dominoes
{
    public static class DominoModifierEffectResolver
    {
        public static List<DominoModifierEffectResult> Resolve(PlacedDomino placed)
        {
            var results = new List<DominoModifierEffectResult>();
            var domino = placed?.Domino;
            var modifier = DominoModifierRegistry.GetById(domino?.ModifierId);
            if (modifier == null)
            {
                return results;
            }

            switch (modifier.Id)
            {
                case DominoModifierRegistry.BlueId:
                    results.Add(new DominoModifierEffectResult(domino.InstanceId, modifier.Id, modifier.Name, countDelta: 15, description: "+15 Tile"));
                    break;
                case DominoModifierRegistry.RedId:
                    results.Add(new DominoModifierEffectResult(domino.InstanceId, modifier.Id, modifier.Name, multDelta: 3, description: "+3 Mult"));
                    break;
                case DominoModifierRegistry.GlassId:
                    results.Add(new DominoModifierEffectResult(domino.InstanceId, modifier.Id, modifier.Name, finalScoreMultiplier: 2f, glassBroken: ChanceUtils.RollChance(1, 4), description: "x2 hand score"));
                    break;
                case DominoModifierRegistry.LuckyId:
                    if (ChanceUtils.RollChance(1, 30))
                    {
                        results.Add(new DominoModifierEffectResult(domino.InstanceId, modifier.Id, modifier.Name, countDelta: 100, multDelta: 10, spinTicketGain: 1, description: "LUCKY HIT! +1 Spin +100 Tile +10 Mult"));
                    }

                    if (ChanceUtils.RollChance(1, 10))
                    {
                        results.Add(new DominoModifierEffectResult(domino.InstanceId, modifier.Id, modifier.Name, multDelta: 30, description: "Lucky +30 Mult"));
                    }

                    if (ChanceUtils.RollChance(1, 3))
                    {
                        results.Add(new DominoModifierEffectResult(domino.InstanceId, modifier.Id, modifier.Name, multDelta: 5, description: "Lucky +5 Mult"));
                    }
                    break;
                case DominoModifierRegistry.JackpotId:
                    results.Add(new DominoModifierEffectResult(domino.InstanceId, modifier.Id, modifier.Name, jackpotMeterGain: 10, description: "+10 Jackpot"));
                    break;
                case DominoModifierRegistry.LightId:
                    results.Add(new DominoModifierEffectResult(domino.InstanceId, modifier.Id, modifier.Name, multDelta: -2, description: "-2 Mult"));
                    break;
            }

            return results;
        }
    }
}
