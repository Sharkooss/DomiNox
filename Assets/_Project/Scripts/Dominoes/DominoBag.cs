using System;
using System.Collections.Generic;

namespace DomiNox.Dominoes
{
    public sealed class DominoBag
    {
        private readonly List<DominoInstance> drawPile = new List<DominoInstance>();
        private readonly List<DominoInstance> discardPile = new List<DominoInstance>();
        private readonly Random random = new Random();

        public void Initialize(List<DominoInstance> dominoes)
        {
            drawPile.Clear();
            discardPile.Clear();
            drawPile.AddRange(dominoes);
            Shuffle();
        }

        public List<DominoInstance> Draw(int amount)
        {
            var drawn = new List<DominoInstance>();
            var count = Math.Min(amount, drawPile.Count);

            for (var i = 0; i < count; i++)
            {
                var lastIndex = drawPile.Count - 1;
                drawn.Add(drawPile[lastIndex]);
                drawPile.RemoveAt(lastIndex);
            }

            return drawn;
        }

        public void Discard(DominoInstance domino)
        {
            if (domino != null)
            {
                discardPile.Add(domino);
            }
        }

        public void Shuffle()
        {
            for (var i = drawPile.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
            }
        }

        public int RemainingCount() => drawPile.Count;

        public IReadOnlyList<DominoInstance> RemainingDominoes => drawPile;
        public IReadOnlyList<DominoInstance> DiscardedDominoes => discardPile;
    }
}
