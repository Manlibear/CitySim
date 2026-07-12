using System;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Presenters;

namespace CitySim.Components;


public class BrowseShopComponent : IComponent
{
    public ItemType ItemType { get; set; }
    public string? Tag { get; set; }
    public Location? ShopLocation { get; set; }
    public Location? CashierLocation { get; set; }
}
