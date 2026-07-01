using System.Linq;
using Godot;
using CitySim.ECS;
using CitySim.Presenters.Buildings.States;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.Characters;
using CitySim.Registries;
using CitySim.Scripts;
using CitySim.Helpers;

namespace CitySim.Presenters.Person;

public partial class PersonPresenter : PresenterNode
{
    [Export] public float MoveSpeed { get; set; } = 64f;
    [Export] public CharacterSpriteLayout? SpriteLayout { get; set; }
    [Export] public string? HomeMap {get;set;}

    public FacingDirection Facing { get; set; } = FacingDirection.South;

    public override void Bootstrap()
    {
        var layer = MapRegistry.GetLayer(MapRegistry.OverworldId);
        var spawnTile = layer != null
            ? layer.LocalToMap(layer.ToLocal(GlobalPosition))
            : Vector2I.Zero;

        if (layer != null)
            GlobalPosition = layer.MapToGlobal(spawnTile);

        Entity.Attach(new WorldPositionComponent
        {
            Position = new WorldPosition(MapRegistry.OverworldId, spawnTile),
        });

        Entity.Attach(new GodotNodeComponent { Node = this });

        if (!string.IsNullOrEmpty(HomeMap))
        {
            Entity.Attach(new HomeComponent(HomeMap));
        }

        Entity.Attach(new ScheduleComponent());
        Entity.Attach(new NeedsComponent());
        Entity.Attach(new ActivityTypeComponent());

        RebuildLayers();
        TransitionTo(IdleState.Instance);
        PlayAnimation("idle");
    }

    public void RebuildLayers()
    {
        if (SpriteLayout == null) return;
        foreach (var layer in GetChildren().OfType<CharacterLayer>())
            layer.Rebuild(SpriteLayout);
    }

    public void PlayAnimation(string stateName)
    {
        var animName = $"{stateName}_{Facing.ToString().ToLower()}";
        foreach (var layer in GetChildren().OfType<CharacterLayer>())
        {
            if (layer.SpriteFrames?.HasAnimation(animName) == true)
                layer.Play(animName);
        }
    }
}
