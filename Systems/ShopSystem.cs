using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Components;
using CitySim.Data.Facts;
using CitySim.ECS;
using CitySim.Registries;
using Godot;

namespace CitySim.Systems;

public class ShopSystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<BrowseShopComponent>().With<PreferenceComponent>().ToList())
        {
            var browseComp = entity.Get<BrowseShopComponent>();


            if (browseComp.Tag == null) throw new ArgumentException("Tried to Browse with a null Tag");
            if (browseComp.ShopLocation == null) throw new ArgumentException("Tried to Browse with a null ShopLocation");
            if (browseComp.ShopLocation.ParentEntityID == null) throw new ArgumentException("Tried to Browse with a null ParentEntityID");
            if (browseComp.CashierLocation == null) throw new ArgumentException("Tried to Browse with a null CashierLocation");

            // check if someone is behind the counter
            if (!OccupancyRegistry.IsLocationReserved(browseComp.CashierLocation!.Name, browseComp.CashierLocation.Map))
            {
                // nobody here, attach a negative fact (for this specific shop location, not the parent shop) and detach the browse
                entity.Detach<BrowseShopComponent>();
                entity.Get<FactComponent>().Add(new BrowseResultFact(browseComp.ShopLocation.EntityID, browseComp.ItemType, browseComp.Tag, false, -5));
                continue;
            }

            var preferences = entity.Get<PreferenceComponent>().Preferences;

            Dictionary<string, float> typePreferences = [];
            preferences.TryGetValue(browseComp.ItemType, out typePreferences!);

            if (InventoryRegistry.TryGet(browseComp.ShopLocation.ParentEntityID.Value, out var sellerInventory))
            {

                var browseResults = sellerInventory!.GetPreferenceScoredCollection(browseComp.ItemType, browseComp.Tag, typePreferences);
                var success = browseResults.Count > 0 && browseResults[0].Score > 0;

                entity.Get<FactComponent>().Add(new BrowseResultFact(browseComp.ShopLocation.ParentEntityID.Value, browseComp.ItemType, browseComp.Tag, success, browseResults.Sum(x => x.Score)));

                if (success)
                {
                    entity.Attach(new ItemTransferRequestComponent()
                    {
                        Amount = 1,
                        Item = browseResults[0].Item.Item,
                        SourceEntityID = browseComp.ShopLocation.ParentEntityID.Value,
                        Cost = browseResults[0].Item.Cost!.Value
                    });
                }

                entity.Detach<BrowseShopComponent>();

            }
        }
    }
}
