using System;
using CitySim.Data;
using CitySim.Registries;
using Godot;

namespace CitySim.Presenters;


public partial class ShopLocationPresenter : LocationPresenter
{
    public new LocationType Type { get; } = LocationType.Shop;

    [Export] public LocationPresenter? CashierLocation { get; set; }

    public override void PostBootstrap()
    {
        if (CashierLocation != null)
            Location.PairedLocation = CashierLocation.Location;
    }
}
