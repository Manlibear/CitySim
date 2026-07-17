using System;
using System.Linq;
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
        foreach (var entity in _world.Entities.With<ScheduleComponent>().With<WorldPositionComponent>().Without<PathfindingComponent>().With<ActivityTypeComponent>())
        {
            var scheduleComp = entity.Get<ScheduleComponent>();
            var activityComp = entity.Get<ActivityTypeComponent>();
            var schedule = scheduleComp.Entries.ToList();
            var currentWorldPos = entity.Get<WorldPositionComponent>().Position;
            var node = entity.Get<GodotNodeComponent>().Node;

            if (entity.TryGet<JobComponent>(out var jobComp))
            {
                schedule.AddRange(jobComp!.Schedule);
            }

            var nextEntry = schedule.GetNext();
            if (nextEntry != null)
            {
                if (activityComp.End.HasValue && activityComp.Priority <= nextEntry.Priority) continue; // blocked by current acitivty, do not schedule

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
                        nextEntry.ResolvedLocation = resolvedLocaton;
                        nextEntry.Position = resolvedLocaton.Position;
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
                        // hate to do double the work, but we need to re-resolve the location
                        // and recompute pathfinding as the occupancy/queueing might have change

                        var resolvedLocaton = LocationRegistry.Resolve(nextEntry.LocationPath, currentWorldPos);
                        if (resolvedLocaton == null)
                        {
                            // we could find it before, but it's not avaiable now, have to chuck it out
                            scheduleComp.RemoveEntry(nextEntry);
                            continue;
                        }

                        nextEntry.Position = resolvedLocaton.Position;
                        nextEntry.CachedPath = PathfindingHelper.FindCrossLevelPath(currentWorldPos.MapID, currentWorldPos.Tile, nextEntry.Position.Value);
                        OccupancyRegistry.ReserveLocation(resolvedLocaton.Name, resolvedLocaton.Map, entity.Id);

                        entity.Attach(new PathfindingComponent
                        {
                            Destination = nextEntry.Position.Value,
                            Status = PathfindingStatus.Moving,
                            Path = nextEntry.CachedPath,
                            OnArriveEffects = nextEntry.OnArriveEffects
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
        if (entry.IsImmediate) return now;

        if (entry.Day == null || entry.Time == null) throw new ArgumentException("For non-immediate tasks Day and Time must be set");

        var nowMins = (int)now.DayOfWeek * 1440 + now.Hour * 60 + now.Minute;
        var entryMins = (int)entry.Day! * 1440 + entry.Time!.Value.Hour * 60 + entry.Time!.Value.Minute;
        var delta = entryMins - nowMins;
        if (delta <= 0) delta += 7 * 1440;
        return now.AddMinutes(delta).Date.Add(entry.Time.Value.ToTimeSpan());
    }
}
