using System;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.ECS;
using CitySim.Helpers;
using CitySim.Scripts;
using Godot;

namespace CitySim.Systems;

public class NeedsSystem(World world) : IUpdateSystem
{
    private World _world = world;

    public void Update(double delta)
    {
        foreach (var entity in _world.Entities.With<NeedsComponent>().With<ScheduleComponent>().With<HomeComponent>())
        {
            var needsComp = entity.Get<NeedsComponent>();
            var scheduleComp = entity.Get<ScheduleComponent>();

            if (entity.Has<SleepComponent>())
            {
                needsComp.Energy += Globals.EnergyRecoverySleepRate * SimWorld.Instance.TimeSpeed * (float)delta;
                needsComp.Satiety -= Globals.SleepMetabolismFactor * Globals.SatietyDecayRate * SimWorld.Instance.TimeSpeed * (float)delta;
            }
            else
            {
                needsComp.Energy -= Globals.EnergyDecayRate * SimWorld.Instance.TimeSpeed * (float)delta;
                needsComp.Satiety -= Globals.SatietyDecayRate * SimWorld.Instance.TimeSpeed * (float)delta;
            }

            needsComp.Social -= Globals.SocialDecayRate * SimWorld.Instance.TimeSpeed * (float)delta;

            needsComp.Energy = Mathf.Clamp(needsComp.Energy, 0, 1);
            needsComp.Satiety = Mathf.Clamp(needsComp.Satiety, 0, 1);
            needsComp.Social = Mathf.Clamp(needsComp.Social, 0, 1);

            if (needsComp.Energy < Globals.MinEnergyNeed &&
            (needsComp.LastEnergySchedule == null || needsComp.LastEnergySchedule.Value.AddHours(Globals.NeedScheduleCooldownHours) < SimWorld.Instance.DateTime))
            {
                entity.InterruptPathfinding();
                var homeComp = entity.Get<HomeComponent>();

                var sleepDt = SimWorld.Instance.DateTime.AddMinutes(5);
                var sleepHours = Mathf.Clamp((1.0f - needsComp.Energy) * 12f, 4f, 10f);
                var wakeUpDt = sleepDt.AddHours(sleepHours);

                needsComp.LastEnergySchedule = sleepDt;

                scheduleComp.AddEntry(new ScheduleEntry()
                {
                    Day = sleepDt.DayOfWeek,
                    Time = TimeOnly.FromDateTime(sleepDt),
                    LocationPath = $"/{homeComp.MapID}/Bed",
                    OnArriveEffects = [
                        new ActivityTypeEffect(ActivityType.Sleep, 1)
                    ]
                    
                });

                scheduleComp.AddEntry(new ScheduleEntry()
                {
                    Day = wakeUpDt.DayOfWeek,
                    Time = TimeOnly.FromDateTime(wakeUpDt),
                    LocationPath = $"/{homeComp.MapID}/Bed",
                    OnArriveEffects = [
                        new ActivityTypeEffect(ActivityType.WakeUp)
                    ]
                });
            }
        }
    }
}