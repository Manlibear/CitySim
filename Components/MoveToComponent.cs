using Godot;
using CitySim.ECS;
using CitySim.Data;

namespace CitySim.Components;

public class MoveToComponent : IComponent
{
    public required WorldPosition Target { get; init; }
    public required Vector2 WorldPos { get; init; }

    // Set by MoveToSystem on first process to trigger animation/facing once per step.
    public bool HasStarted { get; set; }
}
