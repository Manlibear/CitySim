using System;
using CitySim.ECS;

namespace CitySim.Data.StateEffects;

public class AttachComponentEffect<T>(T? instance = null) : IStateEffect where T : class, IComponent, new()
{
    public void Apply(Entity entity)
    {
        entity.Attach(instance ?? new T());
    }
}
