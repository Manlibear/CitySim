using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Components;
using CitySim.Data.Facts;
using CitySim.Data.StateEffects;
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
                    var itemCost = sellerInventory.GetCost(browseResults[0].Item.Item);

                    entity.Attach(new ItemTransferRequestComponent()
                    {
                        Amount = 1,
                        Item = browseResults[0].Item.Item,
                        SourceEntityID = browseComp.ShopLocation.ParentEntityID.Value,
                        Cost = itemCost
                    });

                    entity.Get<FactComponent>().Add(new ItemCostFact(){
                         Item = browseResults[0].Item.Item,
                          ShopID = browseComp.ShopLocation.ParentEntityID.Value,
                          CostFactor = itemCost!.Value / ItemRegistry.Get(browseResults[0].Item.Item).BaseCost
                    });
                }

                entity.Detach<BrowseShopComponent>();

            }
        }

        foreach (var entity in world.Entities.With<ShopForComponent>().Without<ItemTransferRequestComponent>())
        {
            var shopFor = entity.Get<ShopForComponent>();
            if (!InventoryRegistry.TryGet(shopFor.ShopLocation.ParentEntityID!.Value, out var sellerInventory)) throw new ArgumentException("Unable to get shop inventory for " + shopFor.ShopLocation.Name);

            if(!shopFor.NeededItems.Any()){
                entity.Detach<ShopForComponent>();
                continue;
            }

            var (Item, Amount) = shopFor.NeededItems.First();

            if (sellerInventory!.GetAmount(Item) >= Amount)
            {
                entity.Attach(new ItemTransferRequestComponent()
                {
                    Amount = Amount,
                    Item = Item,
                    SourceEntityID = shopFor.ShopLocation.ParentEntityID.Value,
                    Cost =  sellerInventory.GetCost(Item),
                });
            }

            shopFor.NeededItems.RemoveAt(0);
        }
    }
}
