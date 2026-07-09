using CitySim.Components;
using CitySim.ECS;
using CitySim.Registries;

namespace CitySim.Helpers;

public static class EntityHelpers
{
    public static void InterruptPathfinding(this Entity entity)
    {
        if (entity.Has<PathfindingComponent>())
            entity.Detach<PathfindingComponent>();

        entity.ReleaseOccupancy();
    }

    public static void ReleaseOccupancy(this Entity entity)
    {
        OccupancyRegistry.ReleaseAllLocations(entity.Id);
    }
}
