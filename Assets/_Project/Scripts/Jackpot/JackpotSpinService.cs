using System;
using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Jackpot
{
    public static class JackpotSpinService
    {
        public static JackpotSymbol RollSymbol(JackpotState state, Random random)
        {
            var weights = GetWeights(state);
            var total = weights.Values.Sum();
            var roll = random.Next(total);
            var cursor = 0;
            foreach (var pair in weights)
            {
                cursor += pair.Value;
                if (roll < cursor)
                {
                    return pair.Key;
                }
            }

            return JackpotSymbol.Blank;
        }

        public static JackpotSpinResult Spin(JackpotState state, Random random)
        {
            var symbols = new[] { RollSymbol(state, random), RollSymbol(state, random), RollSymbol(state, random) };
            var guaranteePair = state.MachineHeat >= 5;
            symbols = guaranteePair ? EnsureAtLeastPair(symbols) : symbols;

            var tier = GetTier(symbols);
            var reward = tier == JackpotRewardTier.Consolation
                ? JackpotRewardCatalog.ConsolationRewards[random.Next(JackpotRewardCatalog.ConsolationRewards.Count)]
                : tier == JackpotRewardTier.Pair
                    ? JackpotRewardCatalog.GetPairReward(symbols.GroupBy(symbol => symbol).First(group => group.Count() == 2).Key)
                    : JackpotRewardCatalog.GetTripleReward(symbols[0]);

            if (tier == JackpotRewardTier.MajorJackpot || tier == JackpotRewardTier.Triple)
            {
                state.JackpotLuck = 0;
            }
            else
            {
                state.JackpotLuck = Math.Min(5, state.JackpotLuck + 1);
            }

            if (guaranteePair || tier == JackpotRewardTier.MajorJackpot || tier == JackpotRewardTier.Triple)
            {
                state.MachineHeat = 0;
            }
            else
            {
                state.MachineHeat++;
            }

            return new JackpotSpinResult(symbols[0], symbols[1], symbols[2], tier, reward);
        }

        public static Dictionary<JackpotSymbol, int> GetWeights(JackpotState state)
        {
            var luck = state?.JackpotLuck ?? 0;
            return new Dictionary<JackpotSymbol, int>
            {
                { JackpotSymbol.Blank, Math.Max(1, 25 - luck) },
                { JackpotSymbol.Coin, 22 },
                { JackpotSymbol.Domino, 16 },
                { JackpotSymbol.Gem, 14 },
                { JackpotSymbol.DomiNex, 10 },
                { JackpotSymbol.Skull, 6 },
                { JackpotSymbol.Crown, 5 + luck },
                { JackpotSymbol.Seven, 2 + luck }
            };
        }

        public static JackpotSymbol[] EnsureAtLeastPair(JackpotSymbol[] symbols)
        {
            if (HasPair(symbols[0], symbols[1], symbols[2]))
            {
                return symbols;
            }

            symbols[2] = symbols[0];
            return symbols;
        }

        public static bool HasPair(JackpotSymbol a, JackpotSymbol b, JackpotSymbol c)
        {
            return a == b || a == c || b == c;
        }

        public static bool IsTriple(JackpotSymbol a, JackpotSymbol b, JackpotSymbol c)
        {
            return a == b && b == c;
        }

        public static bool IsMajorJackpot(JackpotSymbol a, JackpotSymbol b, JackpotSymbol c)
        {
            return a == JackpotSymbol.Seven && b == JackpotSymbol.Seven && c == JackpotSymbol.Seven;
        }

        public static JackpotRewardTier GetTier(IReadOnlyList<JackpotSymbol> symbols)
        {
            if (IsMajorJackpot(symbols[0], symbols[1], symbols[2]))
            {
                return JackpotRewardTier.MajorJackpot;
            }

            if (IsTriple(symbols[0], symbols[1], symbols[2]))
            {
                return JackpotRewardTier.Triple;
            }

            return HasPair(symbols[0], symbols[1], symbols[2]) ? JackpotRewardTier.Pair : JackpotRewardTier.Consolation;
        }
    }
}
