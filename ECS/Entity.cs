using System;

namespace CitySim.ECS;

public readonly struct Entity : IEquatable<Entity>
{
    public Guid Id { get; }

    private readonly World _world;

    internal Entity(Guid id, World world)
    {
        Id = id;
        _world = world;
    }

    public T Attach<T>(T component) where T : class, IComponent => _world.Attach(this, component);

    public T Get<T>() where T : class, IComponent => _world.Get<T>(this);

    public bool TryGet<T>(out T? component) where T : class, IComponent => _world.TryGet(this, out component);

    public bool Has<T>() where T : class, IComponent => _world.Has<T>(this);

    public void Detach<T>() where T : class, IComponent => _world.Detach<T>(this);

    public void Destroy() => _world.DestroyEntity(this);

    public bool Equals(Entity other) => Id == other.Id && ReferenceEquals(_world, other._world);

    public override bool Equals(object? obj) => obj is Entity other && Equals(other);

    public override int GetHashCode() => Id.GetHashCode();
}
