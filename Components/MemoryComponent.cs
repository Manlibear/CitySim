using System.Collections.Generic;
using System.Linq;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;

public class MemoryComponent : IComponent
{
    public List<IMemory> Memories { get; set; } = [];

    public ShopQueryMemory[] GetNegativeShopQueryMemories(ItemType itemType, string tag)
    {
        return [.. Memories.Where(x => x.Satisfaction < 0 && x is ShopQueryMemory sq && sq.Tag == tag && sq.ItemType == itemType).Select(x => (ShopQueryMemory)x)];
    }

    public string GetAvoidStringByNegativeShopQuery(ItemType itemType, string tag)
    {
        var avoidString = "";
        var negativeShopQueries = GetNegativeShopQueryMemories(itemType, tag).Select(x => x.EntityID);
        if (negativeShopQueries.Any()) avoidString = "!" + string.Join(',', negativeShopQueries);

        return avoidString;
    }
}
