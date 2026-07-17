using System;
using CitySim.Data;

namespace CitySim.Presenters;


public partial class ShopLocationPresenter : LocationPresenter
{
    public new LocationType Type { get; } = LocationType.Shop;
    public LocationPresenter? CashierLocation { get; set; }

    public override void PostBootstrap()
    {
        if (CashierLocation != null)
            Location.PairedLocation = CashierLocation.Location;
    }
}
