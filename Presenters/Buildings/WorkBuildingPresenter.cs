using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.Registries;
using Godot;

namespace CitySim.Presenters.Buildings;

public partial class WorkBuildingPresenter : BuildingPresenter
{

    [Export] public float ManagerWage { get; set; }
    [Export] public required Godot.Collections.Array<OpeningHours> OpeningHours { get; set; }
    [Export] public int LunchHour { get; set; } = 12;

    public override void PreBootstrap()
    {
        base.PreBootstrap();

        if(_interior == null) throw new ArgumentException("Interior scene not set");

        var managerWorkLocation = _interior.FindChild("ManagerLocation", recursive: true) as LocationPresenter ?? throw new KeyNotFoundException($"Cant find ManagerLocation for {Name}");

        var managerSchedule = OpeningHours?.Select(x => new ScheduleEntry()
        {
            LocationPath = $"/{Name}/ManagerLocation",
            Day = x.Day,
            Time = new TimeOnly(x.Open + 1),
            OnArriveEffects = [
                new ActivityTypeEffect(ActivityType.Work, ActivityPriority.Work, LunchHour - (x.Open + 1)),
                  new FacingDirectionEffect(managerWorkLocation.FacingDirection),
             ]
        }).ToList() ?? [];

        managerSchedule.Add(new ScheduleEntry()
        {
            LocationPath = $"/{Name}/BreakLocation*",
            Time = new TimeOnly(LunchHour),
            OnArriveEffects = [
                  new ActivityTypeEffect(ActivityType.Idle, ActivityPriority.Work, 1),
             ]
        });

        managerSchedule.AddRange(OpeningHours?.Select(x => new ScheduleEntry()
        {
            LocationPath = $"/{Name}/ManagerLocation",
            Day = x.Day,
            Time = new TimeOnly(LunchHour + 1),
            OnArriveEffects = [
                  new ActivityTypeEffect(ActivityType.Work, ActivityPriority.Work, x.Close - LunchHour)
                  {
                        OnCompleteEffects = [
                            new SkillDeltaEffect(new(){
                                [Skill.Charisma] = .1f,
                                [Skill.Wisdom] = .05f,
                            })
                        ]
                  },
                  new FacingDirectionEffect(managerWorkLocation.FacingDirection),
             ]
        }).ToList() ?? []);

        EmployerRegistry.AddJob(Name, "Manager", (decimal)ManagerWage, managerSchedule, new() { [Skill.Charisma] = 4f, [Skill.Intelligence] = 3f, [Skill.Wisdom] = 2f });

    }
}
