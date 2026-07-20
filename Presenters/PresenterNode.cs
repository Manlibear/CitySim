using Godot;
using CitySim.ECS;
using CitySim.Scripts;

namespace CitySim.Presenters;

public abstract partial class PresenterNode : Node2D
{
    public Entity Entity { get; private set; }
    public Entity? ParentEntity { get; private set; }

    public World World => SimWorld.Instance.World;

    private IPresenterState? _state;

    public override void _Ready()
    {
        AddToGroup("ecs_entity");
    }

    internal void AssignEntity(Entity entity) => Entity = entity;
    internal void AssignParentEntity(Entity entity) => ParentEntity = entity;

    // Called by SimWorld after entity is assigned. Attach initial components here.
    public abstract void PreBootstrap();
    public abstract void Bootstrap();
    public abstract void PostBootstrap();

    protected void TransitionTo(IPresenterState newState)
    {
        _state?.Exit(this);
        _state = newState;
        _state.Enter(this);
    }

    public override void _Process(double delta)
    {
        if (_state == null) return;

        var next = _state.Poll(this, delta);
        if (next == _state) return;

        _state.Exit(this);
        _state = next;
        _state.Enter(this);
    }
}
