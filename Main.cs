using Godot;
using CitySim.ECS;
using CitySim.Components;
using CitySim.Data;
using CitySim.Scripts;
using CitySim.Presenters.Person;
using System;

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
        var schedule = new ScheduleComponent();
        schedule.AddEntry(new ScheduleEntry()
        {
            Day = DayOfWeek.Saturday,
            Time = new TimeOnly(9, 45),
            LocationPath = "/Overworld/SmallHouse2",
            Type = ActivityType.Liesure,
        });
        testPerson.Entity.Attach(schedule);
    }
}
