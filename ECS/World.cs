using System;
using System.Collections.Generic;
using System.Linq;

namespace CitySim.ECS;

public sealed class World
{
    private readonly HashSet<Guid> _entityIds = [];
    private readonly Dictionary<Type, Dictionary<Guid, IComponent>> _components = [];
    private readonly List<IUpdateSystem> _updateSystems = [];

    public Entity CreateEntity() => CreateEntity(Guid.NewGuid());

    public Entity CreateEntity(Guid id)
    {
        if (!_entityIds.Add(id))
            throw new ArgumentException($"Entity {id} already exists");

        return new Entity(id, this);
    }

    public void DestroyEntity(Entity entity)
    {
        _entityIds.Remove(entity.Id);

        foreach (var store in _components.Values)
            store.Remove(entity.Id);
    }

    public Entity? FindEntityByID(Guid guid)
    {
        return Entities.FirstOrDefault(x => x.Id == guid);
    }

    public T Attach<T>(Entity entity, T component) where T : class, IComponent
    {
        if (!_components.TryGetValue(typeof(T), out var store))
        {
            store = [];
            _components[typeof(T)] = store;
        }

        store[entity.Id] = component;
        return component;
    }

    public T Get<T>(Entity entity) where T : class, IComponent
    {
        if (TryGet<T>(entity, out var component))
            return component!;

        throw new KeyNotFoundException($"Entity {entity.Id} does not have a component of type {typeof(T).Name}.");
    }

    public bool TryGet<T>(Entity entity, out T? component) where T : class, IComponent
    {
        if (_components.TryGetValue(typeof(T), out var store) && store.TryGetValue(entity.Id, out var value))
        {
            component = (T)value;
            return true;
        }

        component = null;
        return false;
    }

    public bool Has<T>(Entity entity) where T : class, IComponent => TryGet<T>(entity, out _);

    public void Detach<T>(Entity entity) where T : class, IComponent
    {
        if (_components.TryGetValue(typeof(T), out var store))
            store.Remove(entity.Id);
    }

    public EntityQuery Entities => new(this);

    internal IEnumerable<Entity> Query(List<Type> with, List<Type> without)
    {
        if (with.Count == 0)
        {
            foreach (var id in _entityIds)
                if (!ExcludedBy(id, without))
                    yield return new Entity(id, this);
            yield break;
        }

        if (!_components.TryGetValue(with[0], out var first))
            yield break;

        foreach (var id in first.Keys)
        {
            if (IncludesAll(id, with) && !ExcludedBy(id, without))
                yield return new Entity(id, this);
        }
    }

    private bool IncludesAll(Guid id, List<Type> with)
    {
        for (var i = 1; i < with.Count; i++)
            if (!_components.TryGetValue(with[i], out var store) || !store.ContainsKey(id))
                return false;

        return true;
    }

    private bool ExcludedBy(Guid id, List<Type> without)
    {
        foreach (var type in without)
            if (_components.TryGetValue(type, out var store) && store.ContainsKey(id))
                return true;

        return false;
    }

    public void Register(IUpdateSystem system) => _updateSystems.Add(system);

    public void Initialize()
    {
        foreach (var system in _updateSystems)
            system.Initialize();
    }

    public void Update(double delta)
    {
        foreach (var system in _updateSystems)
            system.Update(delta);
    }
}
