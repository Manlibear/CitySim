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
                    var diningLocation = LocationRegistry.Resolve("@dining", worldPos);
                    if (diningLocation.HasValue)
                    {
                        var consumedAmount = 1f;
                        if (chosenItem.Definition.PartialUsage && chosenItem.Definition.PartialUsageStep.HasValue)
                        {
                            consumedAmount = chosenItem.Definition.PartialUsageStep.Value;
                        }

                        entity.Attach(new PathfindingComponent()
                        {
                            Destination = diningLocation.Value.Position,
                            OnArriveEffects = [
                                new ActivityTypeEffect(ActivityType.Eat),
                                new NeedsSatisfierEffect(){ NeedsDeltas = chosenItem.Definition.NeedsDelta },
                                new InventoryEffect(chosenItem.Item, -consumedAmount)
                             ]
                        });
                    }
                }
                else
                {
                    var shopLocation = LocationRegistry.Resolve("@" + hungerComp.Tag + memoryComp.GetAvoidStringByNegativeShopQuery(ItemType.Food, hungerComp.Tag), worldPos);
                    if (shopLocation.HasValue)
                    {
                        entity.Attach(new PathfindingComponent()
                        {
                            Destination = shopLocation.Value.Position,
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
