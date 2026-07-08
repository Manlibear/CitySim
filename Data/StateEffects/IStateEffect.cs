using System.Text.Json.Serialization;
using CitySim.ECS;

namespace CitySim.Data.StateEffects;

[JsonPolymorphic]
[JsonDerivedType(typeof(ActivityTypeEffect), "activityType")]
[JsonDerivedType(typeof(FacingDirectionEffect), "facingDirection")]
[JsonDerivedType(typeof(PlayAnimationEffect), "playAnimation")]
public interface IStateEffect
{
    public void Apply(Entity entity, params object[] info);
}
