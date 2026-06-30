using System;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Helpers;
using CitySim.Presenters.Person;
using CitySim.Registries;
using CitySim.Scripts;

namespace CitySim.Systems;

public class ScheduleSystem(World world) : IUpdateSystem
{
    private World _world = world;

    public void Update(double delta)
    {
        foreach (var entity in _world.Entities.With<ScheduleComponent>().With<WorldPositionComponent>().Without<PathfindingComponent>())
        {
            var scheduleComp = entity.Get<ScheduleComponent>();
            var currentWorldPos = entity.Get<WorldPositionComponent>().Position;
            var node = entity.Get<GodotNodeComponent>().Node;

            var nextEntry = scheduleComp.GetNext();
            if (nextEntry != null)
            {
                if (nextEntry.Position == null)
                {
                    var resolvedLocaton = LocationRegistry.Resolve(nextEntry.LocationPath, currentWorldPos);
                    if (resolvedLocaton == null)
                    {
                        // if we can't find a place, we need to remove that schedule entry, we can't handle it
                        scheduleComp.RemoveEntry(nextEntry);
                    }
                    else
                    {
                        nextEntry.Position = resolvedLocaton.Value.Position;
                        nextEntry.CachedPath = PathfindingHelper.FindCrossLevelPath(currentWorldPos.MapID, currentWorldPos.Tile, nextEntry.Position.Value);
                        var tps = (node as PersonPresenter)!.MoveSpeed / Globals.TileSize;
                        var entryDateTime = GetNextOccurrence(nextEntry);
                        nextEntry.DispatchTime = entryDateTime.AddSeconds(-(nextEntry.CachedPath.Count / tps) * SimWorld.Instance.TimeMultiplier);
                    }

                }
                else
                {
                    if (nextEntry.DispatchTime != null && SimWorld.Instance.DateTime >= nextEntry.DispatchTime)
                    {
                        entity.Attach(new PathfindingComponent
                        {
                            Destination = nextEntry.Position.Value,
                            Status = PathfindingStatus.Moving,
                            Path = nextEntry.CachedPath,
                            StatePayload = nextEntry.StatePayload,
                        });

                        if (!nextEntry.Repeats)
                            scheduleComp.RemoveEntry(nextEntry);
                        else
                        {
                            nextEntry.Position = null;
                            nextEntry.CachedPath = null;
                            nextEntry.DispatchTime = null;
                        }
                    }
                }
            }
        }
    }

    private static DateTime GetNextOccurrence(ScheduleEntry entry)
    {
        var now = SimWorld.Instance.DateTime;
        var nowMins = (int)now.DayOfWeek * 1440 + now.Hour * 60 + now.Minute;
        var entryMins = (int)entry.Day * 1440 + entry.Time.Hour * 60 + entry.Time.Minute;
        var delta = entryMins - nowMins;
        if (delta <= 0) delta += 7 * 1440;
        return now.AddMinutes(delta).Date.Add(entry.Time.ToTimeSpan());
    }
}