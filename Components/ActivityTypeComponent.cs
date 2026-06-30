using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;

public class ActivityTypeComponent : IComponent
{
    public ActivityType Type { get; set; }
    public int Priority { get; set; }
}