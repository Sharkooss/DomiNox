using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Consumables
{
    public sealed class ConsumableInstance
    {
        public string DefinitionId { get; }
        public int SellValue { get; }

        public ConsumableInstance(string definitionId, int sellValue)
        {
            DefinitionId = definitionId;
            SellValue = sellValue;
        }
    }

    public sealed class ConsumableInventory
    {
        private readonly List<ConsumableInstance> active = new List<ConsumableInstance>();

        public IReadOnlyList<ConsumableInstance> ActiveInstances => active;
        public IReadOnlyList<string> ActiveIds => active.Select(instance => instance.DefinitionId).ToList();
        public int Count => active.Count;

        public bool CanAdd(int maxSlots) => active.Count < maxSlots;

        public bool Add(string id, int maxSlots)
        {
            return Add(id, maxSlots, 1);
        }

        public bool Add(string id, int maxSlots, int sellValue)
        {
            if (string.IsNullOrWhiteSpace(id) || active.Count >= maxSlots)
            {
                return false;
            }

            active.Add(new ConsumableInstance(id, sellValue));
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

        public ConsumableInstance GetInstanceAt(int index)
        {
            return index < 0 || index >= active.Count ? null : active[index];
        }

        public bool Contains(string id) => active.Any(item => item.DefinitionId == id);
    }
}
