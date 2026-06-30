using System.Threading.Tasks;
using Godot;
using CitySim.ECS;
using CitySim.Components;
using CitySim.Data;
using CitySim.Registries;
using CitySim.Helpers;
using CitySim.Presenters.Person;

namespace CitySim.Systems;

public class PathfindingSystem(World world) : IUpdateSystem
{
    private readonly World _world = world;

    public void Update(double delta)
    {
        foreach (var entity in _world.Entities.With<PathfindingComponent>())
        {
            var pf = entity.Get<PathfindingComponent>();

            switch (pf.Status)
            {
                case PathfindingStatus.Pending:
                    StartPathfinding(entity, pf);
                    break;

                case PathfindingStatus.Resolving:
                    if (pf.PendingTask!.IsFaulted)
                    {
                        GD.PushError($"Pathfinding task failed: {pf.PendingTask.Exception?.GetBaseException().Message}");
                        entity.Detach<PathfindingComponent>();
                    }
                    else if (pf.PendingTask.IsCompleted)
                    {
                        
                        pf.Path = pf.PendingTask!.Result;
                        pf.PendingTask = null;
                        pf.Status = PathfindingStatus.Moving;
                    }
                    break;

                case PathfindingStatus.Moving:
                    if (!entity.Has<MoveToComponent>())
                        AdvancePath(entity, pf);
                    break;
            }
        }
    }

    private void StartPathfinding(Entity entity, PathfindingComponent pf)
    {
        entity.TryGet<WorldPositionComponent>(out var posComp);
        var fromMapID = posComp?.Position.MapID ?? MapRegistry.OverworldId;
        var fromTile = posComp?.Position.Tile ?? Vector2I.Zero;
        var destination = pf.Destination;

        // FindCrossLevelPath only reads static map data built at bootstrap — safe off main thread.
        pf.PendingTask = Task.Run(() => PathfindingHelper.FindCrossLevelPath(fromMapID, fromTile, destination));
        pf.Status = PathfindingStatus.Resolving;
    }

    private void AdvancePath(Entity entity, PathfindingComponent pf)
    {
        while (pf.Path!.Count > 0)
        {
            var next = pf.Path.Dequeue();
            var layer = MapRegistry.GetLayer(next.MapID);
            if (layer == null) continue;

            entity.Attach(new MoveToComponent
            {
                Target = next,
                WorldPos = layer.MapToGlobal(next.Tile)
            });
            return;
        }

        PopStatePayload(pf, entity);
        entity.Detach<PathfindingComponent>();
    }

    private void PopStatePayload(PathfindingComponent pf, Entity entity)
    {
        if (pf.StatePayload != null)
        {
            if (pf.StatePayload.AnimationName != null)
            {
                if (entity.TryGet<GodotNodeComponent>(out var nodeComp) && nodeComp!.Node is PersonPresenter person)
                    person.PlayAnimation(pf.StatePayload.AnimationName);
            }

            if (pf.StatePayload.ActivityType != null || pf.StatePayload.ActivityPriority != null)
            {
                if (entity.TryGet<ActivityTypeComponent>(out var activityComp))
                {
                    if (pf.StatePayload.ActivityType != null)
                        activityComp!.Type = pf.StatePayload.ActivityType.Value;

                    if (pf.StatePayload.ActivityPriority != null)
                        activityComp!.Priority = pf.StatePayload.ActivityPriority.Value;
                }
            }

            if(pf.StatePayload.Component != null)
            {
                entity.Attach(pf.StatePayload.Component);
            }
        }
    }
}
