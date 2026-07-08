using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Data;
using CitySim.Scripts;

namespace CitySim.Registries;

public static class ItemRegistry
{
    const float FractionEpsilon = 0.001f;

    public static ItemDefinition Get(Item item) => _items[item];

    public static string GetAmountDescription(Item item, float amount)
    {
        var itemDef = Get(item);

        var whole = (int)MathF.Floor(amount);
        var frac = amount - whole;

        if (!itemDef.PartialUsage || frac < FractionEpsilon)
            return $"{whole} {UnitFor(itemDef, whole)} of {itemDef.Name}";

        var tier = itemDef.PartialTiers!
            .OrderByDescending(t => t.Threshold)
            .First(t => frac >= t.Threshold - FractionEpsilon);

        if (whole == 0)
            return $"{tier.StandalonePhrase} {itemDef.Name.ToLower()}";

        return tier.CombinedJoinsUnit
            ? $"{whole} and {tier.CombinedFragment} {itemDef.UnitPlural} of {itemDef.Name.ToLower()}"
            : $"{whole} {itemDef.UnitPlural} and {tier.CombinedFragment} of {itemDef.Name.ToLower()}";
    }

    static string UnitFor(ItemDefinition itemDef, int whole) => whole == 1 ? itemDef.UnitSingular : itemDef.UnitPlural;

    private readonly static Dictionary<Item, ItemDefinition> _items = new()
    {
        [Item.WhiteBread] = {
            Type = ItemType.Food,
            Name = "White bread",
            UnitSingular = "loaf",
            UnitPlural = "loaves",
            Tags = ["staple", "carbohydrate", "bread"],
            Description = "A loaf of white bread.",
            NeedsDelta = [
              // per slice
              new() { Duration = SimWorld.Instance.SecondsFromMinutes(3), SatietyDelta = .2f }
            ],
            SlotMax = 1,
            PartialUsage = true,
            PartialUsageStep = .05f, // 20 slices of bread per loaf
            PartialTiers = [
                new(.75f, "Most of a loaf of", "a bit extra", true),
                new(.5f, "Half a loaf of", "a half", true),
                new(.25f, "A quarter of a loaf of", "a quarter", true),
                new(.0f, "A few slices of", "a few slices", false),
            ]
        }
    };

}
