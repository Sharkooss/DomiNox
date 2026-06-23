using System.Collections.Generic;
using System.Linq;
using DomiNox.Core;
using DomiNox.Dominoes;

namespace DomiNox.Grid
{
    public sealed class GridState
    {
        private readonly Dictionary<GridPosition, PlacedDomino> occupiedCells = new Dictionary<GridPosition, PlacedDomino>();
        private readonly List<PlacedDomino> placedDominoes = new List<PlacedDomino>();

        // Max number of disconnected regions allowed on the board. 1 by default; raised by the Faille (le_schisme).
        public int MaxClusters { get; set; } = 1;
        private static readonly GridPosition[] AdjacentOffsets =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1)
        };

        public bool CanPlaceDomino(DominoInstance domino, GridPosition position, DominoOrientation orientation)
        {
            if (domino == null || placedDominoes.Any(placed => placed.Domino == domino))
            {
                return false;
            }

            var cells = GetCells(position, orientation).ToList();
            foreach (var cell in cells)
            {
                if (cell.X < 0 || cell.X >= GameConstants.GridWidth || cell.Y < 0 || cell.Y >= GameConstants.GridHeight)
                {
                    return false;
                }

                if (occupiedCells.ContainsKey(cell))
                {
                    return false;
                }
            }

            if (placedDominoes.Count == 0 || HasMatchingAdjacentValue(domino, orientation, cells))
            {
                return true;
            }

            // Faille: seed a brand new disconnected region, as long as it touches nothing and we are under the cluster cap.
            return MaxClusters > 1 && !HasAnyOccupiedNeighbor(cells) && CountClusters() < MaxClusters;
        }

        private bool HasAnyOccupiedNeighbor(IReadOnlyList<GridPosition> cells)
        {
            foreach (var cell in cells)
            {
                foreach (var offset in AdjacentOffsets)
                {
                    if (occupiedCells.ContainsKey(new GridPosition(cell.X + offset.X, cell.Y + offset.Y)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Counts disconnected regions by flood-filling occupied cells over orthogonal adjacency.
        public int CountClusters()
        {
            var visited = new HashSet<GridPosition>();
            var clusters = 0;
            foreach (var start in occupiedCells.Keys)
            {
                if (visited.Contains(start))
                {
                    continue;
                }

                clusters++;
                var stack = new Stack<GridPosition>();
                stack.Push(start);
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    if (!visited.Add(current))
                    {
                        continue;
                    }

                    foreach (var offset in AdjacentOffsets)
                    {
                        var neighbor = new GridPosition(current.X + offset.X, current.Y + offset.Y);
                        if (occupiedCells.ContainsKey(neighbor) && !visited.Contains(neighbor))
                        {
                            stack.Push(neighbor);
                        }
                    }
                }
            }

            return clusters;
        }

        public bool PlaceDomino(DominoInstance domino, GridPosition position, DominoOrientation orientation)
        {
            if (!CanPlaceDomino(domino, position, orientation))
            {
                return false;
            }

            var placed = new PlacedDomino(domino, position, orientation);
            placedDominoes.Add(placed);

            foreach (var cell in GetCells(position, orientation))
            {
                occupiedCells[cell] = placed;
            }

            return true;
        }

        public bool RemoveDomino(DominoInstance domino)
        {
            var placed = placedDominoes.FirstOrDefault(item => item.Domino == domino);
            if (placed == null)
            {
                return false;
            }

            placedDominoes.Remove(placed);
            foreach (var cell in GetCells(placed.Position, placed.Orientation))
            {
                occupiedCells.Remove(cell);
            }

            return true;
        }

        public List<PlacedDomino> GetPlacedDominoes() => new List<PlacedDomino>(placedDominoes);

        public void Clear()
        {
            placedDominoes.Clear();
            occupiedCells.Clear();
        }

        public PlacedDomino GetAt(GridPosition position) => occupiedCells.TryGetValue(position, out var placed) ? placed : null;

        public bool TryGetCellValue(GridPosition position, out int value)
        {
            if (!occupiedCells.TryGetValue(position, out var placed))
            {
                value = 0;
                return false;
            }

            value = GetPlacedCellValue(placed, position);
            return true;
        }

        public static IEnumerable<GridPosition> GetCells(GridPosition position, DominoOrientation orientation)
        {
            yield return position;
            yield return IsHorizontal(orientation)
                ? new GridPosition(position.X + 1, position.Y)
                : new GridPosition(position.X, position.Y + 1);
        }

        public static bool IsHorizontal(DominoOrientation orientation)
        {
            return orientation == DominoOrientation.HorizontalRight || orientation == DominoOrientation.HorizontalLeft;
        }

        private bool HasMatchingAdjacentValue(DominoInstance domino, DominoOrientation orientation, IReadOnlyList<GridPosition> cells)
        {
            for (var i = 0; i < cells.Count; i++)
            {
                var value = GetCellValue(domino, orientation, i);
                foreach (var offset in AdjacentOffsets)
                {
                    var adjacent = new GridPosition(cells[i].X + offset.X, cells[i].Y + offset.Y);
                    if (TryGetCellValue(adjacent, out var adjacentValue) && adjacentValue == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int GetPlacedCellValue(PlacedDomino placed, GridPosition position)
        {
            if (position.Equals(placed.Position))
            {
                return GetCellValue(placed.Domino, placed.Orientation, 0);
            }

            return GetCellValue(placed.Domino, placed.Orientation, 1);
        }

        public static int GetCellValue(DominoInstance domino, DominoOrientation orientation, int cellIndex)
        {
            var reversed = orientation == DominoOrientation.HorizontalLeft || orientation == DominoOrientation.VerticalUp;
            var firstValue = reversed ? domino.Definition.Right : domino.Definition.Left;
            var secondValue = reversed ? domino.Definition.Left : domino.Definition.Right;
            return cellIndex == 0 ? firstValue : secondValue;
        }
    }
}
