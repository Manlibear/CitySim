using System;
using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.ECS;
using CitySim.Registries;
using CitySim.Scripts;

namespace CitySim.Systems;

public class ConsumptionSystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<HungerComponent>()
                                             .With<MemoryComponent>()
                                             .With<WorldPositionComponent>()
                                             .Without<PathfindingComponent>()
                                             .Without<BrowseShopComponent>()
                                             .Without<ItemTransferRequestComponent>())
        {
            if (InventoryRegistry.TryGet(entity.Id, out var inventory))
            {
                var hungerComp = entity.Get<HungerComponent>();
                var memoryComp = entity.Get<MemoryComponent>();
                var worldPos = entity.Get<WorldPositionComponent>().Position;
                var satisfiedByInventory = inventory!.GetByTag(hungerComp.Tag);

                if (satisfiedByInventory.Any())
                {
                    var chosenItem = satisfiedByInventory.First();
                    var diningLocation = LocationRegistry.Resolve("#dining", worldPos);
                    if (diningLocation != null)
                    {
                        var consumedAmount = 1f;
                        if (chosenItem.Definition.PartialUsage && chosenItem.Definition.PartialUsageStep.HasValue)
                        {
                            consumedAmount = chosenItem.Definition.PartialUsageStep.Value;
                        }

                        if (chosenItem.Definition.NeedsDelta == null) throw new ArgumentException("NeedsDelta must be set for " + chosenItem.Item.ToString());

                        entity.Attach(new PathfindingComponent()
                        {
                            Destination = diningLocation.Position,
                            OnArriveEffects = [
                                new ActivityTypeEffect(ActivityType.Eat),
                                new NeedsSatisfierEffect(){ NeedsDelta = chosenItem.Definition.NeedsDelta },
                                new InventoryEffect(chosenItem.Item, -consumedAmount),
                                AttachComponentEffect.Create(new DelayedEffectComponent(){
                                     Effects = [new ActivityTypeEffect(ActivityType.Idle)],
                                     Delay = chosenItem.Definition.NeedsDelta!.Duration,
                                })
                             ]
                        });
                    }
                }
                else
                {
                    var shopLocation = LocationRegistry.Resolve("#" + hungerComp.Tag + memoryComp.GetAvoidStringByNegativeShopQuery(ItemType.Food, hungerComp.Tag), worldPos);
                    if (shopLocation != null)
                    {
                        entity.Attach(new PathfindingComponent()
                        {
                            Destination = shopLocation.Position,
                            OnArriveEffects = [
                                  AttachComponentEffect.Create(new BrowseShopComponent(){
                                       ItemType = ItemType.Food,
                                       Tag = hungerComp.Tag
                                  })
                              ]
                        });
                    }
                }
            }
        }
    }
}
