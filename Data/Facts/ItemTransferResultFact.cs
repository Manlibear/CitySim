using System;

namespace CitySim.Data.Facts;

public record ItemTransferResultFact : IFact
{
    public bool Succeeded { get; internal set; }
    public Item Item { get; internal set; }
    public Guid EntityID { get; internal set; }
}
