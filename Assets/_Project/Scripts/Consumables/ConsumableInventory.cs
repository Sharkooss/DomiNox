using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Consumables
{
    public sealed class ConsumableInventory
    {
        private readonly List<string> active = new List<string>();

        public IReadOnlyList<string> ActiveIds => active;
        public int Count => active.Count;

        public bool CanAdd(int maxSlots) => active.Count < maxSlots;

        public bool Add(string id, int maxSlots)
        {
            if (string.IsNullOrWhiteSpace(id) || active.Count >= maxSlots)
            {
                return false;
            }

            active.Add(id);
            return true;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= active.Count)
            {
                return false;
            }

            active.RemoveAt(index);
            return true;
        }

        public bool Contains(string id) => active.Any(item => item == id);
    }
}
