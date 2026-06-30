using CitySim.Components;
using CitySim.ECS;

namespace CitySim.Helpers;

public static class EntityHelpers
{
    public static void InterruptPathfinding(this Entity entity)
    {
        if(entity.Has<PathfindingComponent>())
            entity.Detach<PathfindingComponent>();
    }
}