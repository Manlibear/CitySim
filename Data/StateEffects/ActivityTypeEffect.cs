using System.Text.Json.Serialization;
using CitySim.Components;
using CitySim.ECS;

namespace CitySim.Data.StateEffects;

public class ActivityTypeEffect(ActivityType type, int priority = 3) : IStateEffect
{
    [JsonInclude]
    private ActivityType Type {get;set;} = type;

    [JsonInclude]
    private int Priority {get;set; } = priority;

    public void Apply(Entity entity)
    {
        if(entity.TryGet<ActivityTypeComponent>(out var activityTypeComponent))
        {
            activityTypeComponent!.Type = Type;
            activityTypeComponent!.Priority = Priority;
        }
    }
}