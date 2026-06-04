using DomiNox.Dominoes;

namespace DomiNox.Grid
{
    public sealed class PlacedDomino
    {
        public DominoInstance Domino { get; }
        public GridPosition Position { get; }
        public DominoOrientation Orientation { get; }

        public PlacedDomino(DominoInstance domino, GridPosition position, DominoOrientation orientation)
        {
            Domino = domino;
            Position = position;
            Orientation = orientation;
        }
    }
}
