using CitySim.ECS;

namespace CitySim.Data.StateEffects;

public class DetachComponentEffect<T> : IStateEffect where T : class, IComponent, new()
{
    public void Apply(Entity entity) => entity.Detach<T>();
}