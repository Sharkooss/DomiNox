using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Dominex
{
    public sealed class ActiveDomiNexInstance
    {
        public DomiNexDefinition Definition { get; }
        public int PurchasePrice { get; }

        public ActiveDomiNexInstance(DomiNexDefinition definition, int purchasePrice)
        {
            Definition = definition;
            PurchasePrice = purchasePrice;
        }
    }

    public sealed class DomiNexInventory
    {
        private readonly List<ActiveDomiNexInstance> slots = new List<ActiveDomiNexInstance>();

        public IReadOnlyList<ActiveDomiNexInstance> SlotInstances => slots;
        public IReadOnlyList<ActiveDomiNexInstance> ActiveInstances => slots.Where(instance => instance != null).ToList();
        public IReadOnlyList<DomiNexDefinition> Active => slots.Where(instance => instance != null).Select(instance => instance.Definition).ToList();
        public int Count => slots.Count(instance => instance != null);

        public void SetActive(IEnumerable<DomiNexDefinition> definitions)
        {
            slots.Clear();
            slots.AddRange(definitions.Where(definition => definition != null).Select(definition => new ActiveDomiNexInstance(definition, 0)));
        }

        public bool Add(DomiNexDefinition definition)
        {
            return Add(definition, 0);
        }

        public bool Add(DomiNexDefinition definition, int purchasePrice)
        {
            if (definition == null || Contains(definition.Id))
            {
                return false;
            }

            var emptySlot = slots.FindIndex(instance => instance == null);
            if (emptySlot >= 0)
            {
                slots[emptySlot] = new ActiveDomiNexInstance(definition, purchasePrice);
            }
            else
            {
                slots.Add(new ActiveDomiNexInstance(definition, purchasePrice));
            }

            return true;
        }

        public bool CanAdd(DomiNexDefinition definition, int maxSlots)
        {
            return definition != null && !Contains(definition.Id) && Count < maxSlots;
        }

        public bool Remove(string id)
        {
            var index = slots.FindIndex(item => item?.Definition.Id == id);
            if (index < 0)
            {
                return false;
            }

            slots[index] = null;
            TrimTrailingEmptySlots();
            return true;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= slots.Count || slots[index] == null)
            {
                return false;
            }

            slots[index] = null;
            TrimTrailingEmptySlots();
            return true;
        }

        public bool SwapSlots(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= slots.Count || toIndex < 0 || slots[fromIndex] == null || fromIndex == toIndex)
            {
                return false;
            }

            EnsureSlotIndex(toIndex);
            var temp = slots[fromIndex];
            slots[fromIndex] = slots[toIndex];
            slots[toIndex] = temp;
            TrimTrailingEmptySlots();
            return true;
        }

        public ActiveDomiNexInstance GetInstanceAt(int index)
        {
            return index < 0 || index >= slots.Count ? null : slots[index];
        }

        public bool Contains(string id) => slots.Any(instance => instance?.Definition.Id == id);

        private void EnsureSlotIndex(int index)
        {
            while (slots.Count <= index)
            {
                slots.Add(null);
            }
        }

        private void TrimTrailingEmptySlots()
        {
            while (slots.Count > 0 && slots[slots.Count - 1] == null)
            {
                slots.RemoveAt(slots.Count - 1);
            }
        }
    }
}
