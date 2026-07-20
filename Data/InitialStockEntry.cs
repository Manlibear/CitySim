using Godot;

namespace CitySim.Data;

[GlobalClass]
public partial class InitialStockEntry : Resource
{
    [Export]
    public Item Item { get; set; }

    [Export]
    public float Amount { get; set; }

    [Export]
    public float Cost { get; set; }
}
