using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using CitySim.Registries;

namespace CitySim.Data;

public class Inventory
{
    [JsonInclude]
    readonly List<InventorySlot> _slots = [];
    private float PriceFactor { get; set; } = 1f;

    public void SetPriceFactor(float priceFactor) => PriceFactor = priceFactor;
    public void AdjustPriceFactor(float factor) => PriceFactor *= factor;
    public float GetAmount(Item item) => _slots.Where(x => x.Item == item).Sum(x => x.Amount);
    public string GetAmountString(Item item) => ItemRegistry.GetAmountDescription(item, GetAmount(item));
    public decimal? GetCost(Item item) => _slots.FirstOrDefault(x => x.Item == item)?.Cost * (decimal)PriceFactor ?? null;
    public List<InventorySlot> GetSlots() => _slots;

    // cost is null for a citizen's own inventory (nothing to sell to themselves) and set when
    // stocking a shop — GetPreferenceScoredCollection only offers priced slots up for sale.
    // Matching slots only merge if their price matches too, so restocking at a new price starts
    // a fresh slot rather than blending into (and silently repricing) older stock.
    public void Add(Item item, float amount, decimal? cost = null)
    {
        var itemDef = ItemRegistry.Get(item);
        var existingSlots = _slots.Where(x => x.Item == item && x.Cost == cost);

        if (existingSlots.Any())
        {
            var spareSpaceSlot = existingSlots.FirstOrDefault(x => x.Amount + amount <= itemDef.SlotMax);

            if (spareSpaceSlot != null)
            {
                spareSpaceSlot.Amount += amount;
                return;
            }
        }

        if (amount <= itemDef.SlotMax)
        {
            _slots.Add(new(item, amount, cost));
            return;
        }

        while (amount > 0)
        {
            var amountToAdd = Math.Min(itemDef.SlotMax, amount);
            _slots.Add(new(item, amountToAdd, cost));
            amount -= amountToAdd;
        }
    }

    public bool Remove(Item item, float amount = 1)
    {
        var slots = _slots.Where(x => x.Item == item).ToList();
        if (!slots.Any()) return false;
        if (slots.Sum(x => x.Amount) < amount) return false;

        foreach (var s in slots.OrderBy(x => x.Amount))
        {
            var amountToRemove = Math.Min(amount, s.Amount);
            s.Amount -= amountToRemove;
            amount -= amountToRemove;

            if (s.Amount <= 0 && s.DesiredAmount == null)
                _slots.Remove(s);

        }

        return true;
    }

    public void TransferAll(ref Inventory other)
    {
        foreach (var slot in _slots)
        {
            other.Add(slot.Item, slot.Amount, slot.Cost);
        }

        _slots.Clear();
    }

    public IEnumerable<(Item Item, ItemDefinition Definition)> GetByTag(string tag) => _slots.Select(x => (x.Item, ItemRegistry.Get(x.Item))).Where(x => x.Item2.Tags.Contains(tag));

    public List<(InventorySlot Item, float Score)> GetPreferenceScoredCollection(ItemType type, string tag, Dictionary<string, float> preferences, int numOfResults = 10, bool selling = true)
    {
        var results = new List<(InventorySlot Item, float Score)>();

        foreach (var slot in _slots)
        {
            if (!selling || slot.Cost.HasValue)
            {
                var itemDef = ItemRegistry.Get(slot.Item);

                if (itemDef.Type == type && itemDef.Tags.Contains(tag))
                {
                    var score = preferences.Select(x => itemDef.Tags.Contains(x.Key) ? x.Value : 0).Sum(x => x);
                    results.Add((slot, score));
                }
            }
        }

        return [.. results.OrderByDescending(x => x.Score).Take(numOfResults)];
    }
}

public class InventorySlot(Item item, float amount, decimal? value = null)
{
    public Item Item { get; set; } = item;
    public float Amount { get; set; } = amount;
    public float? DesiredAmount { get; set; } = null!;
    public decimal? Cost { get; set; } = value;
}
