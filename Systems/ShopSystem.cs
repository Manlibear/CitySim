using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Components;
using CitySim.Data.Facts;
using CitySim.ECS;
using CitySim.Registries;

namespace CitySim.Systems;

public class ShopSystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<BrowseShopComponent>().With<PreferenceComponent>().ToList())
        {
            var browseComp = entity.Get<BrowseShopComponent>();
            var preferences = entity.Get<PreferenceComponent>().Preferences;

            Dictionary<string, float> typePreferences = [];
            preferences.TryGetValue(browseComp.ItemType, out typePreferences!);

            if (InventoryRegistry.TryGet(browseComp.EntityID, out var sellerInventory))
            {
                if(browseComp.Tag == null) throw new ArgumentException("Tried to Browse with a null Tag");

                var browseResults = sellerInventory!.GetPreferenceScoredCollection(browseComp.ItemType, browseComp.Tag, typePreferences);
                var success = browseResults.Count > 0 && browseResults[0].Score > 0;

                entity.Get<FactComponent>().Add(new BrowseResultFact(browseComp.EntityID, browseComp.ItemType, browseComp.Tag, success, browseResults.Sum(x => x.Score)));

                if (success)
                {
                    entity.Attach(new ItemTransferRequestComponent()
                    {
                        Amount = 1,
                        Item = browseResults[0].Item.Item,
                        SourceEntityID = browseComp.EntityID,
                        Cost = browseResults[0].Item.Cost!.Value
                    });
                }

                entity.Detach<BrowseShopComponent>();

            }
        }
    }
}
