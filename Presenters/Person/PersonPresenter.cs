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
    [Export] public string? HomeMap { get; set; }
    [Export] public string FirstName { get; set; } = "";
    [Export] public string Surname { get; set; } = "";

    private string _currentAnimation { get; set; }

    public FacingDirection Facing { get; set; } = FacingDirection.South;

    public override void PreBootstrap() { }

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
            Entity.Attach(new HomeComponent(HomeMap) { Cost = new ScheduledTransaction() { Amount = 500, DayOfMonth = 2 } });
        }

        Entity.Attach(new ScheduleComponent());
        Entity.Attach(new NeedsComponent());
        Entity.Attach(new ActivityTypeComponent());
        Entity.Attach(new FactComponent());
        Entity.Attach(new WalletComponent());
        Entity.Attach(new PreferenceComponent());
        Entity.Attach(new SkillsComponent());
        Entity.Attach(new CitizenComponent());
        Entity.Attach(new MoodComponent());
        Entity.Attach(new InterestsComponent());
        Entity.Attach(new MemoryComponent());
        Entity.Attach(new RelationshipComponent());

        var nameComp = Entity.Attach(new NameComponent(FirstName, Surname));
        Name = $"{nameComp.FirstName} {nameComp.Surname}";

        RebuildLayers();
        TransitionTo(IdleState.Instance);
        PlayAnimation("idle");
    }

    public override void PostBootstrap() { }

    public void RebuildLayers()
    {
        if (SpriteLayout == null) return;
        foreach (var layer in GetChildren().OfType<CharacterLayer>())
            layer.Rebuild(SpriteLayout);
    }

    public void PlayAnimation(string stateName)
    {
        _currentAnimation = stateName;
        var anim = $"{stateName}_{Facing.ToString().ToLower()}";
        foreach (var layer in GetChildren().OfType<CharacterLayer>())
        {
            if (layer.SpriteFrames?.HasAnimation(anim) == true)
                layer.Play(anim);
        }
    }

    public void PlayCurrentAnimation()
    {
        PlayAnimation(_currentAnimation);
    }
}
