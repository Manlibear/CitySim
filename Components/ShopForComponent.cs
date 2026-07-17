using System.Collections.Generic;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;

public class ShopForComponent : IComponent
{
    public List<(Item Item, float Amount)> NeededItems { get; set; } = [];
    public Location ShopLocation { get; set; }
}
