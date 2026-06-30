using Godot;
using CitySim.ECS;

namespace CitySim.Components;

public class BuildingComponent : IComponent
{
    public required string InteriorScenePath { get; init; }
    public required string InteriorMapID { get; init; }
    public required Vector2I OverworldDoorTile { get; init; }
}
