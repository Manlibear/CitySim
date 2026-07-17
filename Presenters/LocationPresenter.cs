using System;
using CitySim.Components;
using CitySim.Data;
using CitySim.Registries;
using Godot;

namespace CitySim.Presenters;

public partial class LocationPresenter : PresenterNode
{
    [Export] public string[] Tags { get; set; } = [];
    [Export] public LocationType Type { get; set; }
    [Export] public FacingDirection FacingDirection { get; set; }
    public Guid EntityID => Entity.Id;
    public Guid? ParentEntityID => ParentEntity?.Id ?? null;
    public Location Location { get; set; } = null!;

    public override void PreBootstrap() { }
    public override void Bootstrap()
    {
        if (string.IsNullOrEmpty(Name)) throw new ArgumentException("Name is required on locations");

        var mapID = GetOwner().Name;

        if (FindChild("Area2D").FindChild("CollisionShape2D") is not CollisionShape2D locationTile)
            throw new ArgumentException("32x32 CollisionShape2D required");

        Location = new Location()
        {
            Position = new WorldPosition(mapID, (Vector2I)(GetOwner<Node2D>().ToLocal(locationTile.GlobalPosition) / Globals.TileSize)),
            Name = Name,
            Map = mapID,
            Tags = Tags,
            Type = Type,
            EntityID = EntityID,
            ParentEntityID = ParentEntityID,
            FacingDirection = FacingDirection
        };
        LocationRegistry.Register(Location);

        Entity.Attach(new GodotNodeComponent() { Node = this });
    }
    public override void PostBootstrap() { }
}
