using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.Registries;
using Godot;

namespace CitySim.Presenters.Buildings;

public partial class ShopBuildingPresenter : WorkBuildingPresenter
{
    [Export] public float CashierWage { get; set; }
    [Export] public float PriceFactor { get; set; }

    public override void PreBootstrap()
    {
        base.PreBootstrap();

        if (_interior == null) return;

        EmployerRegistry.AddEmployer(Name);

        var managerVisitorLocation = _interior.FindChild("ManagerVisitorLocation", recursive: true) ?? throw new KeyNotFoundException($"Cant find ManagerVisitorLocation for {Name}");

        int idx = 0;
        foreach (var location in _interior.FindChildren("CashierLocation*", recursive: true).Cast<LocationPresenter>())
        {
            var schedule = OpeningHours?.Select(x => new ScheduleEntry()
            {
                LocationPath = $"/{Name}/{location.Name}",
                Day = x.Day,
                Time = new TimeOnly(x.Open),
                OnArriveEffects = [
                      new ActivityTypeEffect(ActivityType.Work, ActivityPriority.Work, LunchHour + idx - x.Open),
                      new FacingDirectionEffect(location.FacingDirection),
                 ]
            }).ToList() ?? [];

            schedule.Add(new ScheduleEntry()
            {
                LocationPath = $"/{Name}/BreakLocation*",
                Time = new TimeOnly(LunchHour + idx),
                OnArriveEffects = [
                      new ActivityTypeEffect(ActivityType.Idle, ActivityPriority.Work, 1),
                 ]
            });

            schedule.AddRange(OpeningHours?.Select(x => new ScheduleEntry()
            {
                LocationPath = $"/{Name}/{location.Name}",
                Day = x.Day,
                Time = new TimeOnly(LunchHour + idx + 1),
                OnArriveEffects = [
                      new ActivityTypeEffect(ActivityType.Work, ActivityPriority.Work, x.Close - LunchHour + 1 + idx)
                      {
                            OnCompleteEffects = [
                                new SkillDeltaEffect(new(){
                                    [Skill.Charisma] = .1f,
                                    [Skill.Dexterity] = .05f,
                                }),
                                new JobPerformanceEffect()
                            ]
                      },
                      new FacingDirectionEffect(location.FacingDirection),
                 ]
            }).ToList() ?? []);

            EmployerRegistry.AddJob(Name, $"Cashier_{location.EntityID}", (decimal)CashierWage, schedule, new() { [Skill.Charisma] = 2f });

            idx++;
        }
    }

    public override void Bootstrap()
    {
        base.Bootstrap();
        Entity.Attach(new WalletComponent());
        InventoryRegistry.Register(Entity.Id);
    }
}
