using System;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;

public class ActivityTypeComponent : IComponent
{
    public ActivityType Type { get; set; }
    public ActivityPriority Priority { get; set; }
    public DateTime? End {get;set;}
}