using CitySim.ECS;
using CitySim.Registries;

namespace CitySim.Data.StateEffects;

public class InventoryEffect(Item item, float amount) : IStateEffect
{
    public Item Item { get; set; } = item;
    public float Amount { get; set; } = amount;

    public void Apply(Entity entity, params object[] info)
    {
        if (InventoryRegistry.TryGet(entity.Id, out var inventory))
        {
            inventory!.Remove(Item, Amount);
        }
    }
}
