using System;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.ECS;

namespace CitySim.Components;

public class ActivityTypeComponent : IComponent
{
    public ActivityType Type { get; set; }
    public ActivityPriority Priority { get; set; }
    public DateTime? End {get;set;}
    public IStateEffect[]? OnCompleteEffects { get; set; }
}
