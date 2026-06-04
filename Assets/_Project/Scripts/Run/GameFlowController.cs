using System;
using System.Collections.Generic;
using System.Linq;
using DomiNox.Core;
using DomiNox.Dominoes;
using DomiNox.Dominex;
using DomiNox.Grid;
using DomiNox.Patterns;
using DomiNox.Scoring;
using UnityEngine;

namespace DomiNox.Run
{
    public sealed class GameFlowController : MonoBehaviour
    {
        private readonly PatternDetector patternDetector = new PatternDetector();
        private readonly DomiNexEffectEngine dominexEffectEngine = new DomiNexEffectEngine();
        private ScoreCalculator scoreCalculator;
        private ScoreResult lastScoreResult;
        private DominoInstance selectedDomino;
        private readonly HashSet<DominoInstance> selectedForDiscard = new HashSet<DominoInstance>();

        public event Action<RunState, ScoreResult, string> StateChanged;

        public RunState Run { get; private set; }
        public DominoOrientation CurrentOrientation { get; private set; } = DominoOrientation.HorizontalRight;
        public DominoInstance SelectedDomino => selectedDomino;
        public IReadOnlyCollection<DominoInstance> SelectedForDiscard => selectedForDiscard;

        private void Awake()
        {
            scoreCalculator = new ScoreCalculator(patternDetector, dominexEffectEngine);
            InitializeRun();
        }

        public void InitializeRun()
        {
            Run = new RunState { CurrentLevel = new LevelState() };
            Run.DomiNexInventory.SetActive(DomiNexRegistry.GetPrototypeActiveSet());
            dominexEffectEngine.ApplyRunStart(Run.DomiNexInventory, Run, null);
            dominexEffectEngine.ApplyLevelStart(Run.DomiNexInventory, Run.CurrentLevel, null);
            Run.Bag.Initialize(DominoFactory.CreateDoubleSixSet());

            foreach (var domino in Run.Bag.Draw(GameConstants.StartingHandSize))
            {
                Run.CurrentLevel.Hand.AddDomino(domino);
            }

            lastScoreResult = new ScoreResult(0, 1, new System.Collections.Generic.List<string>(), new System.Collections.Generic.List<string> { "Place des dominos puis valide." });
            Notify("Selectionne un domino.");
        }

        public void SelectDomino(DominoInstance domino)
        {
            if (Run.CurrentLevel.Hand.Contains(domino))
            {
                if (!selectedForDiscard.Add(domino))
                {
                    selectedForDiscard.Remove(domino);
                }

                Notify($"Selection discard: {selectedForDiscard.Count} domino(s).");
            }
        }

        public void BeginDragDomino(DominoInstance domino)
        {
            if (Run.CurrentLevel.Hand.Contains(domino))
            {
                selectedDomino = domino;
                selectedForDiscard.Remove(domino);
            }
        }

        public void DiscardSelectedDominoes()
        {
            var level = Run.CurrentLevel;
            if (level.DiscardsRemaining <= 0)
            {
                Notify("Plus aucun discard disponible.");
                return;
            }

            if (selectedForDiscard.Count == 0)
            {
                Notify("Selectionne au moins un domino a discard.");
                return;
            }

            var discarded = selectedForDiscard.Where(level.Hand.Contains).ToList();
            foreach (var domino in discarded)
            {
                level.Hand.RemoveDomino(domino);
                Run.Bag.Discard(domino);
            }

            foreach (var domino in Run.Bag.Draw(discarded.Count))
            {
                level.Hand.AddDomino(domino);
            }

            if (!(level.DiscardsUsed == 0 && dominexEffectEngine.HasActive(Run.DomiNexInventory, "main_stable")))
            {
                level.DiscardsRemaining--;
            }

            level.DiscardsUsed++;
            selectedForDiscard.Clear();
            selectedDomino = null;
            Notify($"{discarded.Count} domino(s) discard. {level.DiscardsRemaining} discard(s) restant(s).");
        }

        public void ToggleOrientation()
        {
            RotateRight();
            Notify($"Orientation: {CurrentOrientation}");
        }

        public void RotateRightWithoutNotify()
        {
            RotateRight();
        }

        public void RotateLeftWithoutNotify()
        {
            CurrentOrientation = CurrentOrientation == DominoOrientation.HorizontalRight
                ? DominoOrientation.VerticalUp
                : (DominoOrientation)((int)CurrentOrientation - 1);
        }

        private void RotateRight()
        {
            CurrentOrientation = CurrentOrientation == DominoOrientation.VerticalUp
                ? DominoOrientation.HorizontalRight
                : (DominoOrientation)((int)CurrentOrientation + 1);
        }

        public void TryPlaceSelected(int x, int y)
        {
            var level = Run.CurrentLevel;
            if (selectedDomino == null)
            {
                Notify("Aucun domino selectionne.");
                return;
            }

            if (level.Grid.GetPlacedDominoes().Count >= level.MaxPlacedDominoes || level.ActionPoints <= 0)
            {
                Notify("Limite de placement atteinte.");
                return;
            }

            var position = new GridPosition(x, y);
            if (!level.Grid.PlaceDomino(selectedDomino, position, CurrentOrientation))
            {
                Notify("Placement impossible: connecte une valeur identique.");
                return;
            }

            level.Hand.RemoveDomino(selectedDomino);
            level.ActionPoints--;
            selectedDomino = null;
            Notify("Domino place.");
        }

        public bool CanPlaceSelected(int x, int y)
        {
            var level = Run.CurrentLevel;
            if (selectedDomino == null || level.Grid.GetPlacedDominoes().Count >= level.MaxPlacedDominoes || level.ActionPoints <= 0)
            {
                return false;
            }

            return level.Grid.CanPlaceDomino(selectedDomino, new GridPosition(x, y), CurrentOrientation);
        }

        public void ResetPlacements()
        {
            var level = Run.CurrentLevel;
            var placed = level.Grid.GetPlacedDominoes();
            level.Grid.Clear();

            foreach (var item in placed)
            {
                level.Hand.AddDomino(item.Domino);
            }

            selectedDomino = null;
            selectedForDiscard.Clear();
            level.ActionPoints = GameConstants.PhaseOneActionPoints;
            level.DiscardsUsed = 0;
            level.CurrentScore = 0;
            level.IsWon = false;
            level.IsLost = false;
            lastScoreResult = new ScoreResult(0, 1, new System.Collections.Generic.List<string>(), new System.Collections.Generic.List<string> { "Placements reinitialises." });
            Notify("Placements reinitialises.");
        }

        public void ValidateScore()
        {
            var level = Run.CurrentLevel;
            var dominexContext = new DomiNexScoringContext(Run.DomiNexInventory.Active, Run.Credits, level.DiscardsUsed, level.MaxPlacedDominoes);
            lastScoreResult = scoreCalculator.Calculate(level.Grid.GetPlacedDominoes(), level.MaxPlacedDominoes, dominexContext);
            level.CurrentScore = lastScoreResult.FinalScore;
            level.IsWon = level.CurrentScore >= level.Quota;
            level.IsLost = !level.IsWon;
            Notify(level.IsWon ? "Niveau reussi." : "Score insuffisant.");
        }

        private void Notify(string message)
        {
            StateChanged?.Invoke(Run, lastScoreResult, message);
        }
    }
}
