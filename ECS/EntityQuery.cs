using System;
using System.Collections;
using System.Collections.Generic;

namespace CitySim.ECS;

public sealed class EntityQuery : IEnumerable<Entity>
{
    private readonly World _world;
    private readonly List<Type> _with = [];
    private readonly List<Type> _without = [];

    internal EntityQuery(World world) => _world = world;

    public EntityQuery With<T>() where T : class, IComponent
    {
        _with.Add(typeof(T));
        return this;
    }

    public EntityQuery Without<T>() where T : class, IComponent
    {
        _without.Add(typeof(T));
        return this;
    }

    public IEnumerator<Entity> GetEnumerator() => _world.Query(_with, _without).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
