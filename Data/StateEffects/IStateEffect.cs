using CitySim.ECS;

namespace CitySim.Data.StateEffects;

public interface IStateEffect
{
    public void Apply(Entity entity);
}