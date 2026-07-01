using Godot;
using CitySim.ECS;
using CitySim.Components;
using CitySim.Data;
using CitySim.Scripts;
using CitySim.Presenters.Person;
using System;
using CitySim.Data.StateEffects;

namespace CitySim;

// Purely for quick and dirty testing
public partial class Main : Node2D
{
    [Export] public SimWorld simWorld { get; set; } = null!;
    [Export] public PersonPresenter testPerson { get; set; } = null!;

    public override void _Ready() { }

    public override void _Process(double delta) { }

    public void _on_button_pressed()
    {
        GD.Print("Working");
        var schedule = testPerson.Entity.Get<ScheduleComponent>();
        schedule.AddEntry(new ScheduleEntry()
        {
            Day = DayOfWeek.Saturday,
            Time = new TimeOnly(9, 10),
            LocationPath = "/Overworld/SmallHouse1",
            OnArriveEffects = [
                new ActivityTypeEffect(ActivityType.Liesure),
                new FacingDirectionEffect(FacingDirection.South)
             ]

        });
        testPerson.Entity.Attach(schedule);
    }
}
