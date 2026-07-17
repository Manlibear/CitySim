using System;

namespace CitySim.Data.Facts;

public class ItemCostFact : IFact
{
    public Guid ShopID { get; set; }
    public Item Item { get; set; }
    public decimal CostFactor { get; set; }
}
