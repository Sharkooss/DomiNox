using System.Collections.Generic;
using System.Linq;
using DomiNox.Core;
using DomiNox.Grid;

namespace DomiNox.Patterns
{
    public sealed class PatternDetector
    {
        private static readonly GridPosition[] AdjacentOffsets =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1)
        };

        public List<string> DetectPatterns(List<PlacedDomino> placedDominoes)
        {
            return DetectPatterns(placedDominoes, GameConstants.PhaseOneMaxPlacedDominoes);
        }

        public List<string> DetectPatterns(List<PlacedDomino> placedDominoes, int maxPlacedDominoes)
        {
            return DetectPatternInfos(placedDominoes, maxPlacedDominoes).Select(pattern => pattern.Name).ToList();
        }

        public List<PatternInfo> DetectPatternInfos(List<PlacedDomino> placedDominoes, int maxPlacedDominoes)
        {
            return DetectPatternInfos(placedDominoes, maxPlacedDominoes, null);
        }

        public List<PatternInfo> DetectPatternInfos(List<PlacedDomino> placedDominoes, int maxPlacedDominoes, IReadOnlyCollection<string> disabledPatternIds)
        {
            placedDominoes = placedDominoes ?? new List<PlacedDomino>();
            var disabled = new HashSet<string>(disabledPatternIds ?? new string[0]);

            var patterns = new List<PatternInfo>();
            if (placedDominoes.Count == 0)
            {
                return patterns;
            }

            patterns.Add(DetectBestValuePattern(placedDominoes, disabled));

            var design = DetectBestDesignPattern(placedDominoes);
            if (design != null)
            {
                patterns.Add(design);
            }

            return patterns.Where(pattern => pattern != null && !disabled.Contains(ToPatternId(pattern.Name))).ToList();
        }

        private static PatternInfo DetectBestValuePattern(IReadOnlyCollection<PlacedDomino> placedDominoes, HashSet<string> disabled)
        {
            if (placedDominoes.Count == 0)
            {
                return PatternCatalog.GetByName(PatternNames.HighTile);
            }

            var detected = new List<PatternInfo>();
            var doubleCount = placedDominoes.Count(placed => placed.Domino.Definition.IsDouble);
            var jackpotSevenCount = placedDominoes.Count(placed => placed.Domino.Definition.Sum == 7);
            var allLowRoll = placedDominoes.All(placed => placed.Domino.Definition.Sum <= 5);
            var valuesPerDomino = placedDominoes
                .SelectMany(placed => new[] { placed.Domino.Definition.Left, placed.Domino.Definition.Right }.Distinct())
                .GroupBy(value => value)
                .ToDictionary(group => group.Key, group => group.Count());

            if (doubleCount >= 3)
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.TripleDouble));
            }

            if (HasStraight(placedDominoes, 5))
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.BigStraight));
            }

            if (jackpotSevenCount >= 3)
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.JackpotSeven));
            }

            if (doubleCount >= 2)
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.DoublePair));
            }

            if (valuesPerDomino.Values.Any(count => count >= 3))
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.SameValue));
            }

            if (HasStraight(placedDominoes, 3))
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.SmallStraight));
            }

            if (doubleCount >= 1)
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.DoublePlayed));
            }

            if (valuesPerDomino.Values.Any(count => count >= 2))
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.PairLink));
            }

            if (allLowRoll)
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.LowRoll));
            }

            return detected.Where(pattern => pattern != null && !disabled.Contains(ToPatternId(pattern.Name))).OrderByDescending(pattern => pattern.Priority).FirstOrDefault()
                ?? PatternCatalog.GetByName(PatternNames.HighTile);
        }

        private static bool HasStraight(IReadOnlyCollection<PlacedDomino> placedDominoes, int requiredLength)
        {
            var values = placedDominoes
                .SelectMany(placed => new[] { placed.Domino.Definition.Left, placed.Domino.Definition.Right })
                .Distinct()
                .OrderBy(value => value)
                .ToList();

            var run = 1;
            for (var i = 1; i < values.Count; i++)
            {
                run = values[i] == values[i - 1] + 1 ? run + 1 : 1;
                if (run >= requiredLength)
                {
                    return true;
                }
            }

            return requiredLength <= 1 && values.Count > 0;
        }

        private static PatternInfo DetectBestDesignPattern(IReadOnlyCollection<PlacedDomino> placedDominoes)
        {
            var cells = placedDominoes.SelectMany(placed => GridState.GetCells(placed.Position, placed.Orientation)).Distinct().ToList();
            if (cells.Count < 2)
            {
                return null;
            }

            var detected = new List<PatternInfo>();
            if (IsLoop(placedDominoes))
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.Loop));
            }

            var directionChanges = CountPathDirectionChanges(cells);
            if (directionChanges >= 2)
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.Snake));
            }
            else if (directionChanges == 1)
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.Corner));
            }

            if (cells.All(cell => cell.X == cells[0].X) || cells.All(cell => cell.Y == cells[0].Y))
            {
                detected.Add(PatternCatalog.GetByName(PatternNames.Line));
            }

            return detected.Where(pattern => pattern != null).OrderByDescending(pattern => pattern.Priority).FirstOrDefault();
        }

        private static bool IsLoop(IReadOnlyCollection<PlacedDomino> placedDominoes)
        {
            var cellValues = BuildCellValues(placedDominoes);
            var cells = cellValues.Keys.ToList();
            if (cells.Count < 6)
            {
                return false;
            }

            var cellSet = new HashSet<GridPosition>(cells);
            return cells.All(cell => CountAdjacentCells(cell, cellSet) == 2) && AllExternalContactsMatch(placedDominoes, cellValues);
        }

        private static Dictionary<GridPosition, int> BuildCellValues(IEnumerable<PlacedDomino> placedDominoes)
        {
            var values = new Dictionary<GridPosition, int>();
            foreach (var placed in placedDominoes)
            {
                var index = 0;
                foreach (var cell in GridState.GetCells(placed.Position, placed.Orientation))
                {
                    values[cell] = GridState.GetCellValue(placed.Domino, placed.Orientation, index++);
                }
            }

            return values;
        }

        private static bool AllExternalContactsMatch(IReadOnlyCollection<PlacedDomino> placedDominoes, Dictionary<GridPosition, int> cellValues)
        {
            var placedByCell = new Dictionary<GridPosition, PlacedDomino>();
            foreach (var placed in placedDominoes)
            {
                foreach (var cell in GridState.GetCells(placed.Position, placed.Orientation))
                {
                    placedByCell[cell] = placed;
                }
            }

            foreach (var pair in placedByCell)
            {
                foreach (var adjacent in GetAdjacentCells(pair.Key, new HashSet<GridPosition>(placedByCell.Keys)))
                {
                    if (placedByCell[adjacent] != pair.Value && cellValues[pair.Key] != cellValues[adjacent])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static string ToPatternId(string name)
        {
            return name.ToLowerInvariant().Replace(' ', '_');
        }

        private static int CountPathDirectionChanges(IReadOnlyCollection<GridPosition> cells)
        {
            var cellSet = new HashSet<GridPosition>(cells);
            var endpoints = cells.Where(cell => CountAdjacentCells(cell, cellSet) == 1).ToList();
            if (endpoints.Count != 2 || cells.Any(cell => CountAdjacentCells(cell, cellSet) > 2))
            {
                return 0;
            }

            var previous = endpoints[0];
            var current = GetAdjacentCells(previous, cellSet).First();
            var previousDirection = new GridPosition(current.X - previous.X, current.Y - previous.Y);
            var changes = 0;

            while (!current.Equals(endpoints[1]))
            {
                var nextCandidates = GetAdjacentCells(current, cellSet).Where(cell => !cell.Equals(previous)).ToList();
                if (nextCandidates.Count == 0)
                {
                    break;
                }

                var next = nextCandidates[0];

                var direction = new GridPosition(next.X - current.X, next.Y - current.Y);
                if (!direction.Equals(previousDirection))
                {
                    changes++;
                }

                previousDirection = direction;
                previous = current;
                current = next;
            }

            return changes;
        }

        private static int CountAdjacentCells(GridPosition cell, HashSet<GridPosition> cellSet)
        {
            return GetAdjacentCells(cell, cellSet).Count();
        }

        private static IEnumerable<GridPosition> GetAdjacentCells(GridPosition cell, HashSet<GridPosition> cellSet)
        {
            foreach (var offset in AdjacentOffsets)
            {
                var adjacent = new GridPosition(cell.X + offset.X, cell.Y + offset.Y);
                if (cellSet.Contains(adjacent))
                {
                    yield return adjacent;
                }
            }
        }
    }
}
