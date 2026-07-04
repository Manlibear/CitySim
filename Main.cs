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
    [Export] public SimWorld SimWorld { get; set; } = null!;

    public override void _Ready() { }

    public override void _Process(double delta) { }

    public void _on_save_button_pressed()
    {
        GD.Print("Saving...");
        SimWorld.SaveGame();
    }

    public void _on_load_button_pressed()
    {
        GD.Print("Loading...");
        SimWorld.LoadGame();
    }
}
