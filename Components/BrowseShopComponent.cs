using System;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;


public class BrowseShopComponent : IComponent
{
    public Guid EntityID { get; set; }
    public ItemType ItemType { get; set; }
    public string Tag { get; set; }
}
