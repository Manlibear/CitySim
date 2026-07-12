using System;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;

public class HomeComponent(string mapID) : IComponent
{
    public string MapID { get; set; } = mapID;
    public Guid HomeEntityID { get; set; }
    public bool HomeOwner { get; set; }
    public decimal? Mortgage { get; set; }
    public Guid? LandlordID { get; set; }
    public required ScheduledTransaction? Cost { get; set; }
}
