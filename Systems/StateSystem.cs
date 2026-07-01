using System;
using System.Collections.Generic;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Registries;

namespace CitySim.Systems;

public class StateSystem(World world) : IUpdateSystem
{
    private readonly World _world = world;
    private readonly Dictionary<Guid, ActivityType> _lastSeen = [];

    public void Initialize() => StateEffectRegistry.Initialize();

    public void Update(double delta)
    {
        foreach (var entity in _world.Entities.With<ActivityTypeComponent>())
        {
            var current = entity.Get<ActivityTypeComponent>().Type;

            if (_lastSeen.TryGetValue(entity.Id, out var previous))
            {
                if (previous == current) continue;
                foreach (var effect in StateEffectRegistry.Get(previous, current))
                    effect.Apply(entity);
            }

            _lastSeen[entity.Id] = current;
        }
    }
}