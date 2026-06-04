using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Dominex
{
    public sealed class DomiNexInventory
    {
        private readonly List<DomiNexDefinition> active = new List<DomiNexDefinition>();

        public IReadOnlyList<DomiNexDefinition> Active => active;

        public void SetActive(IEnumerable<DomiNexDefinition> definitions)
        {
            active.Clear();
            active.AddRange(definitions.Where(definition => definition != null));
        }
    }
}
