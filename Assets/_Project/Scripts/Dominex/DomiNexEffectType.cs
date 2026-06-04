namespace DomiNox.Dominex
{
    public enum DomiNexEffectType
    {
        AddCount,
        AddMult,
        AddCountPerDominoSumAtLeast,
        AddCountPerDominoSumAtMost,
        AddMultPerDominoSumAtLeast,
        AddMultPerDominoSumAtMost,
        AddMultPerDominoSumExactly,
        AddMultPerDouble,
        AddMultPerCreditStep,
        AddMultIfNoDiscard,
        AddCountIfPlacedAtLeast,
        AddCountToLastDomino,
        AddMultToFirstDomino,
        AddMultIfExactPlacedCount,
        AddMultIfFullNox,
        AddMaxPlacedDominoes,
        AddCredits,
        FutureHook
    }
}
