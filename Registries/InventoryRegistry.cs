using System;
using System.Collections.Generic;
using CitySim.Data;

namespace CitySim.Registries
{
    public class InventoryRegistry
    {
        private static Dictionary<Guid, Inventory> _inventories { get; set; } = [];

        public static void Register(Guid guid)
        {
            _inventories[guid] = new Inventory();
        }

        public static bool TryGet(Guid guid, out Inventory? inventory)
        {
            return _inventories.TryGetValue(guid, out inventory);
        }
    }
}
