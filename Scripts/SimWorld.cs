using System.Linq;
using Godot;
using CitySim.ECS;
using CitySim.Presenters;
using CitySim.Registries;
using CitySim.Systems;
using System;

namespace CitySim.Scripts;

public partial class SimWorld : Node
{
    public static SimWorld Instance { get; private set; } = null!;

    [Export] public float TimeSpeed { get; set; } = 1f;
    [Export] public float TimeMultiplier {get;set;} = 60;
    public DateTime DateTime {get;set;} = new DateTime(2000, 01, 01, 9, 0, 0);
    public World World { get; } = new();

    public override void _Ready()
    {
        Instance = this;
        CallDeferred(MethodName.Bootstrap);
    }

    public override void _Process(double delta)
    {
        World.Update(delta * TimeSpeed);
        DateTime = DateTime.AddSeconds(delta * TimeMultiplier * TimeSpeed);
    }

    private void Bootstrap()
    {
        var overworldLayers = GetTree().CurrentScene
            .GetChildren()
            .OfType<TileMapLayer>()
            .ToArray();
        if (overworldLayers.Length > 0)
            MapRegistry.Register(MapRegistry.OverworldId, overworldLayers);

        foreach (var node in GetTree().GetNodesInGroup("ecs_entity"))
        {
            if (node is not PresenterNode presenter) continue;
            var entity = World.CreateEntity();
            presenter.AssignEntity(entity);
            presenter.Bootstrap();
        }

        World.Register(new PathfindingSystem(World));
        World.Register(new MoveToSystem(World));
        World.Register(new ScheduleSystem(World));
        World.Register(new NeedsSystem(World));
        World.Register(new StateSystem(World));
        World.Initialize();
    }
}
