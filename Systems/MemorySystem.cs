using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.Facts;
using CitySim.ECS;
using CitySim.Registries;

namespace CitySim.Systems;


public class MemorySystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<FactComponent>().With<MemoryComponent>().ToList())
        {
            var factComp = entity.Get<FactComponent>();
            var memoryComp = entity.Get<MemoryComponent>();
            var needsComp = entity.Get<NeedsComponent>();

            while (factComp.Facts.Any())
            {
                var newFact = factComp.Facts.Dequeue();

                switch (newFact)
                {
                    case ItemTransferResultFact itr:
                        if (!itr.Succeeded)
                        {
                            var itemDef = ItemRegistry.Get(itr.Item);

                            memoryComp.Memories.Add(new ShopQueryMemory()
                            {
                                Available = false,
                                EntityID = itr.EntityID,
                                Item = itr.Item,
                                Satisfaction = -ComputeMoodFromItem(entity, needsComp, itemDef.Type) //TODO: This needs inflating
                            });
                        }
                        break;

                    case BrowseResultFact br:
                        memoryComp.Memories.Add(new ShopQueryMemory()
                        {
                            Available = br.Success,
                            EntityID = br.EntityID,
                            Tag = br.Tag,
                            ItemType = br.ItemType,
                            Satisfaction = br.TotalScore
                        });
                        break;

                    case JobInterviewFact jif:
                        memoryComp.Memories.Add(new ConfidenceMemory()
                        {
                            // TODO: bullshit guess numbers
                            Satisfaction = jif.Success ? 20 : -5,
                            Type = ActivityType.Interview
                        });
                        break;
                }
            }
        }
    }

    private float ComputeMoodFromItem(Entity entity, NeedsComponent needsComp, ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Food => 1 - needsComp.Satiety,
            _ => 0,
        };
    }
}
