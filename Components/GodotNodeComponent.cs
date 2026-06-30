using Godot;
using CitySim.ECS;

namespace CitySim.Components;

public class GodotNodeComponent : IComponent
{
    public required Node2D Node { get; init; }
}
