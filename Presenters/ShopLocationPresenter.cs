using System;
using CitySim.Data;

namespace CitySim.Presenters;


public partial class ShopLocationPresenter : LocationPresenter
{
    public new Guid EntityID { get; set; }
    public new LocationType Type { get; } = LocationType.Shop;
}
