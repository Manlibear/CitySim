using CitySim.Components;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.ECS;
using CitySim.Registries;
using Godot;

namespace CitySim.Systems;

public class SleepSystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<TiredComponent>().With<NeedsComponent>().Without<PathfindingComponent>())
        {
            var needsComp = entity.Get<NeedsComponent>();
            var sleepHours = Mathf.Clamp((1.0f - needsComp.Energy) * 12f, 4f, 10f);
            WorldPosition? sleepPos = null;

            if (entity.TryGet<HomeComponent>(out var homeComp))
            {
                var bed = LocationRegistry.Resolve($"/{homeComp!.MapID}/Bed");
                if (bed != null)
                    sleepPos = bed.Position;
            }
            else
            {
                //TODO: Handle homeless people
            }


            if (sleepPos.HasValue)
            {
                entity.Attach(new PathfindingComponent()
                {
                    Destination = sleepPos.Value,
                    OnArriveEffects = [
                        new ActivityTypeEffect(ActivityType.Sleep, ActivityPriority.Exhausted, sleepHours),
                        new DetachComponentEffect<TiredComponent>(),
                    ]
                });
            }
        }
    }
}
