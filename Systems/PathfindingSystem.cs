using System.Threading.Tasks;
using Godot;
using CitySim.ECS;
using CitySim.Components;
using CitySim.Registries;
using CitySim.Helpers;

namespace CitySim.Systems;

public class PathfindingSystem(World world) : IUpdateSystem
{
    private readonly World _world = world;

    public void Update(double delta)
    {
        foreach (var entity in _world.Entities.With<PathfindingComponent>().Without<MoveToComponent>())
        {
            var pf = entity.Get<PathfindingComponent>();

            switch (pf.Status)
            {
                case PathfindingStatus.Pending:
                    entity.TryGet<WorldPositionComponent>(out var posComp);
                    var fromMapID = posComp?.Position.MapID ?? MapRegistry.OverworldId;
                    var fromTile = posComp?.Position.Tile ?? Vector2I.Zero;
                    var destination = pf.Destination;

                    // FindCrossLevelPath only reads static map data built at bootstrap — safe off main thread.
                    pf.PendingTask = Task.Run(() => PathfindingHelper.FindCrossLevelPath(fromMapID, fromTile, destination));
                    pf.Status = PathfindingStatus.Resolving;
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

                    var locationInfo = new object[0];
                    var location = LocationRegistry.GetLocationByTile(pf.Destination);

                    if (location != null)
                    {
                        locationInfo = [
                            ("EntityID", location.Value.EntityID)
                        ];
                    }

                    // locationInfo is an object[] passed as the params array itself (not wrapped in another array),
                    // so effects see each ("Name", value) tuple individually.
                    foreach (var effect in pf.OnArriveEffects ?? [])
                        effect.Apply(entity, locationInfo);

                    entity.Detach<PathfindingComponent>();
                    break;
            }
        }
    }

}
