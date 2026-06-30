using CitySim.ECS;

namespace CitySim.Data;

public class StatePayload
{
    public ActivityType? ActivityType { get; set; }
    public string? AnimationName { get; set; }
    public int? ActivityPriority { get; set; }
    public FacingDirection FaciringDirection {get;set;}
    public IComponent? Component {get;set;}
}