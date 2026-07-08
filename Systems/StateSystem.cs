using System;
using System.Collections.Generic;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Registries;
using CitySim.Scripts;

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
            var activityComp = entity.Get<ActivityTypeComponent>();
            
            if (_lastSeen.TryGetValue(entity.Id, out var previous))
            {
                if (previous == activityComp.Type) continue;
                foreach (var effect in StateEffectRegistry.Get(previous, activityComp.Type))
                    effect.Apply(entity);
            }

            _lastSeen[entity.Id] = activityComp.Type;

            if(activityComp.End <= SimWorld.Instance.DateTime)
            {
                activityComp.Type = ActivityType.Idle;
                activityComp.Priority = ActivityPriority.Idle;
            }
        }
    }
}