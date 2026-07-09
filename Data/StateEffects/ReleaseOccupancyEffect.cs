using CitySim.ECS;
using CitySim.Helpers;

namespace CitySim.Data.StateEffects;

public class ReleaseOccupancyEffect : IStateEffect
{
    public void Apply(Entity entity, params object[] info)
    {
        entity.ReleaseOccupancy();
    }
}
