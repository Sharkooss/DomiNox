using System.Collections.Generic;

namespace DomiNox.Dominoes
{
    public sealed class HandState
    {
        public List<DominoInstance> Dominoes { get; } = new List<DominoInstance>();
        public int MaxHandSize { get; }

        public HandState(int maxHandSize)
        {
            MaxHandSize = maxHandSize;
        }

        public bool AddDomino(DominoInstance domino)
        {
            if (domino == null || Dominoes.Count >= MaxHandSize)
            {
                return false;
            }

            Dominoes.Add(domino);
            return true;
        }

        public bool RemoveDomino(DominoInstance domino) => Dominoes.Remove(domino);
        public bool Contains(DominoInstance domino) => Dominoes.Contains(domino);
    }
}
