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

        public bool CanPlaceDomino(DominoInstance domino, GridPosition position, DominoOrientation orientation)
        {
            if (domino == null || placedDominoes.Any(placed => placed.Domino == domino))
            {
                return false;
            }

            foreach (var cell in GetCells(position, orientation))
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

            return true;
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

        public static IEnumerable<GridPosition> GetCells(GridPosition position, DominoOrientation orientation)
        {
            yield return position;
            yield return orientation == DominoOrientation.Horizontal
                ? new GridPosition(position.X + 1, position.Y)
                : new GridPosition(position.X, position.Y + 1);
        }
    }
}
