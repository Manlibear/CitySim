using System.Text.Json.Serialization;
using CitySim.Components;
using CitySim.ECS;
using CitySim.Scripts;

namespace CitySim.Data.StateEffects;

/// <summary>
///
/// </summary>
/// <param name="type"></param>
/// <param name="priority"></param>
/// <param name="duration"></param>
public class ActivityTypeEffect(ActivityType type, ActivityPriority priority = ActivityPriority.Default, double? durationHours = null) : IStateEffect
{
    [JsonInclude]
    private ActivityType Type { get; set; } = type;

    [JsonInclude]
    private ActivityPriority Priority { get; set; } = priority;

    [JsonInclude]
    private double? DurationHours { get; set; } = durationHours;

    public void Apply(Entity entity, params object[] info)
    {
        if (entity.TryGet<ActivityTypeComponent>(out var activityTypeComponent))
        {
            activityTypeComponent!.Type = Type;
            activityTypeComponent!.Priority = Priority;
            if (DurationHours.HasValue)
            {
                activityTypeComponent!.End = SimWorld.Instance.DateTime.AddHours(DurationHours.Value);
            }
        }
    }
}
