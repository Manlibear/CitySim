using System.Collections.Generic;
using System.Linq;
using Godot;
using CitySim.ECS;
using CitySim.Presenters.Buildings.States;
using CitySim.Components;
using CitySim.Data;
using CitySim.Registries;
using CitySim.Scripts;
using CitySim.Helpers;

namespace CitySim.Presenters.Buildings;

public partial class BuildingPresenter : PresenterNode
{
    [Export] public PackedScene? InteriorScene { get; set; }
    [Export] public Vector2I DoorTileOffset { get; set; }
    [Export] public string[] Tags { get; set; } = [];
    [Export] public LocationType Type { get; set; }
    [Export] public Polygon2D? WindowColour {get;set;}

    public AnimationPlayer? DoorAnimation { get; private set; }

    [Export] public float MaxLightEnergy = 1.2f;

    private InteriorScene? _interior;

    public override void Bootstrap()
    {
        var layer = MapRegistry.GetLayer(MapRegistry.OverworldId);
        var buildingTile = layer != null
            ? layer.LocalToMap(layer.ToLocal(GlobalPosition))
            : Vector2I.Zero;
        var doorTile = buildingTile + DoorTileOffset;

        if (InteriorScene != null)
        {
            _interior = InteriorScene.Instantiate<InteriorScene>();
            _interior.MapID = Name;
            _interior.OverworldDoorTile = doorTile;
            _interior.Visible = false;
            AddChild(_interior);

            foreach (var node in _interior.GetChildren().Where(x => x.IsInGroup("ecs_entity")))
            {
                if (node is not PresenterNode presenter) continue;
                var entity = World.CreateEntity();
                presenter.AssignEntity(entity);
                presenter.Bootstrap();
                
            }
        }

        Entity.Attach(new BuildingComponent
        {
            InteriorScenePath = InteriorScene?.ResourcePath ?? "",
            InteriorMapID = Name,
            OverworldDoorTile = doorTile
        });

        DoorAnimation = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");


        MapRegistry.AddWarp(
            new WorldPosition(MapRegistry.OverworldId, doorTile),
            new WorldPosition(Name, _interior!.ExitTile)
        );

        MapRegistry.MarkBlocked(MapRegistry.OverworldId, BlockedTiles());

        LocationRegistry.Register(new Location()
        {
            Name = Name,
            Position = new WorldPosition(MapRegistry.OverworldId, doorTile),
            Tags = Tags,
            Type = Type
        });

        TransitionTo(IdleState.Instance);
    }

    public override void _Process(double delta)
    {
        //bad2e0
        if(WindowColour != null)
        {
            WindowColour.Color = DayNightCycle.Instance!.CurrentWindowColour;
        }

        var dayBlend = DayNightCycle.Instance?.DayBlend ?? 1f;
        foreach(var light in GetNode("Lights").FindChildren("*").OfType<PointLight2D>())
        {
            light.Energy = Mathf.Lerp(MaxLightEnergy, 0f, dayBlend);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, DoubleClick: true, Pressed: true })
            return;

        var mousePos = GetGlobalMousePosition();
        var hit = GetChildren().OfType<Sprite2D>()
            .Any(s => s.GetRect().HasPoint(s.ToLocal(mousePos)));

        if (hit)
        {
            OpenDoor();
            GetViewport().SetInputAsHandled();
        }
    }

    public void OpenDoor() => TransitionTo(new DoorOpeningState());

    public void OpenInterior()
    {
        if (_interior == null) return;
        InteriorWindow.Instance?.Open(_interior);
    }

    private IEnumerable<Vector2I> BlockedTiles()
    {
        var layer = MapRegistry.GetLayer(MapRegistry.OverworldId);
        if (layer == null) yield break;

        foreach (var node in FindChildren("*", "CollisionPolygon2D"))
        {
            if (node is not CollisionPolygon2D poly) continue;

            var worldPolygon = poly.Polygon.Select(p => poly.ToGlobal(p)).ToArray();

            var minX = worldPolygon.Min(p => p.X);
            var maxX = worldPolygon.Max(p => p.X);
            var minY = worldPolygon.Min(p => p.Y);
            var maxY = worldPolygon.Max(p => p.Y);

            var minTile = layer.LocalToMap(layer.ToLocal(new Vector2(minX, minY)));
            var maxTile = layer.LocalToMap(layer.ToLocal(new Vector2(maxX, maxY)));

            for (var x = minTile.X; x <= maxTile.X; x++)
                for (var y = minTile.Y; y <= maxTile.Y; y++)
                {
                    var tile = new Vector2I(x, y);
                    var tileCenter = layer.MapToGlobal(tile);
                    if (Geometry2D.IsPointInPolygon(tileCenter, worldPolygon))
                        yield return tile;
                }
        }
    }
}
