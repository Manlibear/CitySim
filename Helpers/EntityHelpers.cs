using System;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Presenters.Person;
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

    public static FacingDirection Opposite(this FacingDirection dir)
    {
        return dir switch
        {
            FacingDirection.South => FacingDirection.North,
            FacingDirection.North => FacingDirection.South,
            FacingDirection.East => FacingDirection.West,
            FacingDirection.West => FacingDirection.East,
            _ => throw new Exception("Unexpected FacingDirection " + dir.ToString()),
        };
    }

    public static void SetFacingDirection(this Entity entity, FacingDirection dir)
    {
        if (entity.TryGet<GodotNodeComponent>(out var nodeComp))
        {
            if (nodeComp!.Node is PersonPresenter person)
            {
                person.Facing = dir;
                person.PlayCurrentAnimation();
            }
        }
    }
}
