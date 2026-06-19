using System;
using System.Collections.Generic;
using System.Linq;
using DomiNox.Dominoes;
using DomiNox.Grid;
using DomiNox.Patterns;
using DomiNox.Run;
using DomiNox.Scoring;

namespace DomiNox.Jackpot
{
    public static class JackpotMeterService
    {
        public static int AddJackpotMeter(RunState run, int baseAmount, JackpotGainSource source)
        {
            if (run == null || baseAmount <= 0)
            {
                return 0;
            }

            var applied = Math.Max(0, (int)Math.Round(baseAmount * run.Jackpot.JackpotMeterMultiplier));
            run.Jackpot.Meter += applied;
            while (run.Jackpot.Meter >= run.Jackpot.MaxMeter)
            {
                run.Jackpot.Meter -= run.Jackpot.MaxMeter;
                run.Jackpot.SpinTickets++;
            }

            run.Jackpot.LastGainBreakdown.Add(new JackpotGainLine(source, baseAmount, applied));
            return applied;
        }

        public static int ApplyScoringGains(RunState run, ScoreResult score, IReadOnlyList<PlacedDomino> placedDominoes, bool secretUnlocked, Random random)
        {
            return ApplyScoringGains(run, score, placedDominoes, secretUnlocked, random, 0, score?.FinalScore ?? 0);
        }

        public static int ApplyScoringGains(RunState run, ScoreResult score, IReadOnlyList<PlacedDomino> placedDominoes, bool secretUnlocked, Random random, int levelScoreBeforeHand, int levelScoreAfterHand)
        {
            run.Jackpot.LastGainBreakdown.Clear();
            var total = 0;
            var isBoss = run.CurrentLevel.Boss != null;
            total += AddJackpotMeter(run, random.Next(isBoss ? 25 : 8, isBoss ? 36 : 13), isBoss ? JackpotGainSource.BossClear : JackpotGainSource.ClassicLevelClear);

            var sumSevenCount = placedDominoes.Count(placed => placed.Domino.Definition.Sum == 7);
            total += AddJackpotMeter(run, sumSevenCount * 3, JackpotGainSource.SumSevenDomino);

            var doubleCount = placedDominoes.Count(placed => placed.Domino.Definition.IsDouble);
            total += AddJackpotMeter(run, doubleCount * 2, JackpotGainSource.DoubleDomino);

            if (score.ValuePatternId == PatternNames.JackpotSevenId)
            {
                total += AddJackpotMeter(run, 20, JackpotGainSource.JackpotSevenPattern);
            }

            if (score.ValuePatternId == PatternNames.DoubleTileId)
            {
                total += AddJackpotMeter(run, 5, JackpotGainSource.DoubleTilePattern);
            }

            if (score.ValuePatternId == PatternNames.TripleDoubleId)
            {
                total += AddJackpotMeter(run, 15, JackpotGainSource.TripleDoublePattern);
            }

            var level = run.CurrentLevel;
            if (level.Quota > 0 && levelScoreAfterHand >= level.Quota * 2 && !level.AwardedQuotaOver100JackpotGain)
            {
                total += AddJackpotMeter(run, 20, JackpotGainSource.QuotaOver100);
                level.AwardedQuotaOver100JackpotGain = true;
                level.AwardedQuotaOver50JackpotGain = true;
            }
            else if (level.Quota > 0 && levelScoreAfterHand >= (int)Math.Ceiling(level.Quota * 1.5f) && !level.AwardedQuotaOver50JackpotGain)
            {
                total += AddJackpotMeter(run, 15, JackpotGainSource.QuotaOver50);
                level.AwardedQuotaOver50JackpotGain = true;
            }

            if (secretUnlocked)
            {
                total += AddJackpotMeter(run, 50, JackpotGainSource.SecretPatternUnlocked);
            }

            return total;
        }
    }
}
