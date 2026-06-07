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
            if (placedDominoes.Count == 0)
            {
                return new List<PatternInfo>();
            }

            var disabled = new HashSet<string>(disabledPatternIds ?? new string[0]);
            var patterns = new List<PatternInfo>();
            var value = DetectBestValuePattern(placedDominoes, disabled);
            if (value != null)
            {
                patterns.Add(value);
            }

            var design = DetectBestDesignPattern(placedDominoes, disabled);
            if (design != null)
            {
                patterns.Add(design);
            }

            return patterns;
        }

        private static PatternInfo DetectBestValuePattern(IReadOnlyCollection<PlacedDomino> placedDominoes, HashSet<string> disabled)
        {
            var detected = new List<PatternInfo>();
            var doubleCount = placedDominoes.Count(placed => placed.Domino.Definition.IsDouble);
            var jackpotSevenCount = placedDominoes.Count(placed => placed.Domino.Definition.Sum == 7);

            if (jackpotSevenCount >= 3)
            {
                detected.Add(PatternCatalog.GetById(PatternNames.JackpotSevenId));
            }

            if (HasConnectedStraight(placedDominoes, 5))
            {
                detected.Add(PatternCatalog.GetById(PatternNames.LongTileStraightId));
            }

            if (HasConnectedStraight(placedDominoes, 3))
            {
                detected.Add(PatternCatalog.GetById(PatternNames.SmallTileStraightId));
            }

            if (doubleCount >= 3)
            {
                detected.Add(PatternCatalog.GetById(PatternNames.TripleDoubleId));
            }

            if (doubleCount >= 2)
            {
                detected.Add(PatternCatalog.GetById(PatternNames.DoubleTileId));
            }

            if (placedDominoes.All(placed => placed.Domino.Definition.Sum > 8))
            {
                detected.Add(PatternCatalog.GetById(PatternNames.HighTileId));
            }

            if (placedDominoes.All(placed => placed.Domino.Definition.Sum <= 5))
            {
                detected.Add(PatternCatalog.GetById(PatternNames.LowTileId));
            }

            return detected.Where(pattern => pattern != null && !disabled.Contains(pattern.Id)).OrderByDescending(pattern => pattern.Priority).FirstOrDefault()
                ?? PatternCatalog.GetById(PatternNames.TileHighId);
        }

        private static PatternInfo DetectBestDesignPattern(IReadOnlyCollection<PlacedDomino> placedDominoes, HashSet<string> disabled)
        {
            var graph = BuildConnectionGraph(placedDominoes);
            if (IsBigLoop(placedDominoes, graph) && !disabled.Contains(PatternNames.BigLoopId))
            {
                return PatternCatalog.GetById(PatternNames.BigLoopId);
            }

            if (IsLoop(placedDominoes, graph) && !disabled.Contains(PatternNames.TileLoopId))
            {
                return PatternCatalog.GetById(PatternNames.TileLoopId);
            }

            if (TryGetChristCross(placedDominoes, graph, out var christExtensionCount) && !disabled.Contains(PatternNames.ChristCrossId))
            {
                return PatternCatalog.GetById(PatternNames.ChristCrossId)?.WithRuntimeData(1, christExtensionCount);
            }

            if (TryGetCrossCenter(placedDominoes, graph, out _, out _, out var extensionCount))
            {
                if (!disabled.Contains(PatternNames.CrossTileId))
                {
                    return PatternCatalog.GetById(PatternNames.CrossTileId)?.WithRuntimeData(1, extensionCount);
                }
            }

            if (IsLine(placedDominoes, graph) && !disabled.Contains(PatternNames.TileLineId))
            {
                return PatternCatalog.GetById(PatternNames.TileLineId);
            }

            return null;
        }

        private static bool HasConnectedStraight(IReadOnlyCollection<PlacedDomino> placedDominoes, int requiredLength)
        {
            var candidates = placedDominoes
                .Where(placed => System.Math.Abs(placed.Domino.Definition.Left - placed.Domino.Definition.Right) == 1)
                .ToList();

            foreach (var start in candidates)
            {
                var low = System.Math.Min(start.Domino.Definition.Left, start.Domino.Definition.Right);
                if (FindStraightPath(start, low, requiredLength, candidates, new HashSet<PlacedDomino> { start }))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool FindStraightPath(PlacedDomino current, int currentLow, int requiredLength, IReadOnlyCollection<PlacedDomino> candidates, HashSet<PlacedDomino> used)
        {
            if (used.Count >= requiredLength)
            {
                return true;
            }

            foreach (var next in candidates)
            {
                if (used.Contains(next))
                {
                    continue;
                }

                var nextLow = System.Math.Min(next.Domino.Definition.Left, next.Domino.Definition.Right);
                if (nextLow != currentLow + 1 || !AreDominoesAdjacent(current, next))
                {
                    continue;
                }

                used.Add(next);
                if (FindStraightPath(next, nextLow, requiredLength, candidates, used))
                {
                    return true;
                }

                used.Remove(next);
            }

            return false;
        }

        private static bool IsLine(IReadOnlyCollection<PlacedDomino> placedDominoes, Dictionary<PlacedDomino, List<DominoConnection>> graph)
        {
            if (placedDominoes.Count < 3 || !IsValidGraphConnected(placedDominoes, graph))
            {
                return false;
            }

            var cells = placedDominoes.SelectMany(placed => GridState.GetCells(placed.Position, placed.Orientation)).Distinct().ToList();
            return cells.All(cell => cell.X == cells[0].X) || cells.All(cell => cell.Y == cells[0].Y);
        }

        private static bool TryGetCrossCenter(IReadOnlyCollection<PlacedDomino> placedDominoes, Dictionary<PlacedDomino, List<DominoConnection>> graph, out PlacedDomino center, out HashSet<GridPosition> directions, out int extensionCount)
        {
            center = null;
            directions = null;
            extensionCount = 0;
            if (placedDominoes.Count < 4 || !IsValidGraphConnected(placedDominoes, graph))
            {
                return false;
            }

            foreach (var placed in placedDominoes)
            {
                var currentDirections = new HashSet<GridPosition>(graph[placed].Select(connection => connection.Direction));
                if (currentDirections.Count < 3)
                {
                    continue;
                }

                center = placed;
                directions = currentDirections;
                extensionCount = System.Math.Max(0, placedDominoes.Count - 4);
                return true;
            }

            return false;
        }

        private static bool TryGetChristCross(IReadOnlyCollection<PlacedDomino> placedDominoes, Dictionary<PlacedDomino, List<DominoConnection>> graph, out int extensionCount)
        {
            extensionCount = 0;
            if (placedDominoes.Count < 5 || !IsValidGraphConnected(placedDominoes, graph))
            {
                return false;
            }

            foreach (var placed in placedDominoes)
            {
                var directions = new HashSet<GridPosition>(graph[placed].Select(connection => connection.Direction));
                if (directions.Count < 3 || !directions.Contains(new GridPosition(-1, 0)) || !directions.Contains(new GridPosition(1, 0)))
                {
                    continue;
                }

                if (IsChristCross(placedDominoes, graph, placed, directions))
                {
                    extensionCount = System.Math.Max(0, placedDominoes.Count - 5);
                    return true;
                }
            }

            return false;
        }

        private static bool IsChristCross(IReadOnlyCollection<PlacedDomino> placedDominoes, Dictionary<PlacedDomino, List<DominoConnection>> graph, PlacedDomino center, HashSet<GridPosition> directions)
        {
            if (placedDominoes.Count < 5)
            {
                return false;
            }

            var up = GetBranchLength(graph, center, new GridPosition(0, -1));
            var down = GetBranchLength(graph, center, new GridPosition(0, 1));
            var left = GetBranchLength(graph, center, new GridPosition(-1, 0));
            var right = GetBranchLength(graph, center, new GridPosition(1, 0));
            var vertical = up + down;
            var horizontal = left + right;

            return left >= 1 && right >= 1 && vertical >= 2 && vertical + 1 > horizontal;
        }

        private static int GetBranchLength(Dictionary<PlacedDomino, List<DominoConnection>> graph, PlacedDomino center, GridPosition direction)
        {
            var best = 0;
            foreach (var connection in graph[center].Where(connection => connection.Direction.Equals(direction)))
            {
                best = System.Math.Max(best, 1 + GetBranchLength(graph, connection.Other, direction, new HashSet<PlacedDomino> { center, connection.Other }));
            }

            return best;
        }

        private static int GetBranchLength(Dictionary<PlacedDomino, List<DominoConnection>> graph, PlacedDomino current, GridPosition direction, HashSet<PlacedDomino> visited)
        {
            var best = 0;
            foreach (var connection in graph[current].Where(connection => connection.Direction.Equals(direction) && !visited.Contains(connection.Other)))
            {
                visited.Add(connection.Other);
                best = System.Math.Max(best, 1 + GetBranchLength(graph, connection.Other, direction, visited));
                visited.Remove(connection.Other);
            }

            return best;
        }

        private static bool IsLoop(IReadOnlyCollection<PlacedDomino> placedDominoes, Dictionary<PlacedDomino, List<DominoConnection>> graph)
        {
            if (placedDominoes.Count < 4 || !IsValidGraphConnected(placedDominoes, graph))
            {
                return false;
            }

            return placedDominoes.All(placed => graph[placed].Select(connection => connection.Other).Distinct().Count() == 2);
        }

        private static bool IsBigLoop(IReadOnlyCollection<PlacedDomino> placedDominoes, Dictionary<PlacedDomino, List<DominoConnection>> graph)
        {
            if (!IsLoop(placedDominoes, graph))
            {
                return false;
            }

            var cells = placedDominoes.SelectMany(placed => GridState.GetCells(placed.Position, placed.Orientation)).Distinct().ToList();
            var minX = cells.Min(cell => cell.X);
            var maxX = cells.Max(cell => cell.X);
            var minY = cells.Min(cell => cell.Y);
            var maxY = cells.Max(cell => cell.Y);
            if (maxX - minX + 1 != 4 || maxY - minY + 1 != 4)
            {
                return false;
            }

            var cellSet = new HashSet<GridPosition>(cells);
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var onPerimeter = x == minX || x == maxX || y == minY || y == maxY;
                    var occupied = cellSet.Contains(new GridPosition(x, y));
                    if (onPerimeter != occupied)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsValidGraphConnected(IReadOnlyCollection<PlacedDomino> placedDominoes, Dictionary<PlacedDomino, List<DominoConnection>> graph)
        {
            if (placedDominoes.Count == 0)
            {
                return false;
            }

            var visited = new HashSet<PlacedDomino>();
            var stack = new Stack<PlacedDomino>();
            stack.Push(placedDominoes.First());
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!visited.Add(current))
                {
                    continue;
                }

                foreach (var connection in graph[current])
                {
                    stack.Push(connection.Other);
                }
            }

            return visited.Count == placedDominoes.Count;
        }

        private static bool AreDominoesAdjacent(PlacedDomino a, PlacedDomino b)
        {
            var aCells = GridState.GetCells(a.Position, a.Orientation).ToList();
            var bCells = new HashSet<GridPosition>(GridState.GetCells(b.Position, b.Orientation));
            return aCells.Any(cell => GetAdjacentCells(cell, bCells).Any());
        }

        private static Dictionary<PlacedDomino, List<DominoConnection>> BuildConnectionGraph(IReadOnlyCollection<PlacedDomino> placedDominoes)
        {
            var graph = placedDominoes.ToDictionary(placed => placed, _ => new List<DominoConnection>());
            var cells = new Dictionary<GridPosition, CellInfo>();
            foreach (var placed in placedDominoes)
            {
                var index = 0;
                foreach (var cell in GridState.GetCells(placed.Position, placed.Orientation))
                {
                    cells[cell] = new CellInfo(placed, GridState.GetCellValue(placed.Domino, placed.Orientation, index++));
                }
            }

            foreach (var pair in cells)
            {
                foreach (var offset in AdjacentOffsets)
                {
                    var adjacentPosition = new GridPosition(pair.Key.X + offset.X, pair.Key.Y + offset.Y);
                    if (!cells.TryGetValue(adjacentPosition, out var adjacent) || adjacent.Placed == pair.Value.Placed || adjacent.Value != pair.Value.Value)
                    {
                        continue;
                    }

                    graph[pair.Value.Placed].Add(new DominoConnection(adjacent.Placed, offset));
                }
            }

            return graph;
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

            var cellSet = new HashSet<GridPosition>(placedByCell.Keys);
            foreach (var pair in placedByCell)
            {
                foreach (var adjacent in GetAdjacentCells(pair.Key, cellSet))
                {
                    if (placedByCell[adjacent] != pair.Value && cellValues[pair.Key] != cellValues[adjacent])
                    {
                        return false;
                    }
                }
            }

            return true;
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

        private readonly struct CellInfo
        {
            public PlacedDomino Placed { get; }
            public int Value { get; }

            public CellInfo(PlacedDomino placed, int value)
            {
                Placed = placed;
                Value = value;
            }
        }

        private readonly struct DominoConnection
        {
            public PlacedDomino Other { get; }
            public GridPosition Direction { get; }

            public DominoConnection(PlacedDomino other, GridPosition direction)
            {
                Other = other;
                Direction = direction;
            }
        }
    }
}
