using System;
using Godot;

namespace CitySim.Data;

[GlobalClass]
public partial class OpeningHours : Resource
{
    [Export]
    public DayOfWeek Day { get; set; }

    [Export]
    public int Open { get; set; }

    [Export]
    public int Close { get; set; }
}
