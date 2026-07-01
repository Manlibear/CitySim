using CitySim.Components;
using CitySim.ECS;

namespace CitySim.Data.StateEffects;

public class ActivityTypeEffect(ActivityType type, int priority = 3) : IStateEffect
{
    public void Apply(Entity entity)
    {
        if(entity.TryGet<ActivityTypeComponent>(out var activityTypeComponent))
        {
            activityTypeComponent!.Type = type;
            activityTypeComponent!.Priority = priority;
        }
    }
}