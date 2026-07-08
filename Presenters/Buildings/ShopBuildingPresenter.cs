using System.Linq;
using CitySim.Components;

namespace CitySim.Presenters.Buildings;

public partial class ShopBuildingPresenter : BuildingPresenter
{
    public override void PreBootstrap()
    {
        base.PreBootstrap();

        if (_interior == null) return;

        foreach (var location in _interior.FindChildren("ShopLocation*").Cast<ShopLocationPresenter>())
        {
            location.EntityID = Entity.Id;
        }
    }

    public override void Bootstrap()
    {
        base.Bootstrap();
        Entity.Attach(new WalletComponent());
    }
}
