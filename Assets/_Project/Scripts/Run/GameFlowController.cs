using System;
using DomiNox.Core;
using DomiNox.Dominoes;
using DomiNox.Grid;
using DomiNox.Patterns;
using DomiNox.Scoring;
using UnityEngine;

namespace DomiNox.Run
{
    public sealed class GameFlowController : MonoBehaviour
    {
        private readonly PatternDetector patternDetector = new PatternDetector();
        private ScoreCalculator scoreCalculator;
        private ScoreResult lastScoreResult;
        private DominoInstance selectedDomino;

        public event Action<RunState, ScoreResult, string> StateChanged;

        public RunState Run { get; private set; }
        public DominoOrientation CurrentOrientation { get; private set; } = DominoOrientation.Horizontal;
        public DominoInstance SelectedDomino => selectedDomino;

        private void Awake()
        {
            scoreCalculator = new ScoreCalculator(patternDetector);
            InitializeRun();
        }

        public void InitializeRun()
        {
            Run = new RunState { CurrentLevel = new LevelState() };
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
                selectedDomino = domino;
                Notify($"Domino selectionne: {domino}");
            }
        }

        public void BeginDragDomino(DominoInstance domino)
        {
            if (Run.CurrentLevel.Hand.Contains(domino))
            {
                selectedDomino = domino;
            }
        }

        public void ToggleOrientation()
        {
            CurrentOrientation = CurrentOrientation == DominoOrientation.Horizontal ? DominoOrientation.Vertical : DominoOrientation.Horizontal;
            Notify($"Orientation: {CurrentOrientation}");
        }

        public void RotateSelectedRight()
        {
            CurrentOrientation = CurrentOrientation == DominoOrientation.Horizontal ? DominoOrientation.Vertical : DominoOrientation.Horizontal;
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
            level.ActionPoints = GameConstants.PhaseOneActionPoints;
            level.CurrentScore = 0;
            level.IsWon = false;
            level.IsLost = false;
            lastScoreResult = new ScoreResult(0, 1, new System.Collections.Generic.List<string>(), new System.Collections.Generic.List<string> { "Placements reinitialises." });
            Notify("Placements reinitialises.");
        }

        public void ValidateScore()
        {
            var level = Run.CurrentLevel;
            lastScoreResult = scoreCalculator.Calculate(level.Grid.GetPlacedDominoes(), level.MaxPlacedDominoes);
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
