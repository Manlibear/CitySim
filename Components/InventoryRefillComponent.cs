using System.Collections.Generic;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;


public class InventoryRefillComponent : IComponent
{
    public Dictionary<LocationType, List<(Item Item, float Amount)>> NeededItems { get; set; } = [];
    public InventoryRefillStatus Status { get; set; }
}

public enum InventoryRefillStatus
{
    Pending,
    InProgress
}
