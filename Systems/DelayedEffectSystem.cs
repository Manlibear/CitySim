using CitySim.Components;
using CitySim.Data.StateEffects;
using CitySim.ECS;

namespace CitySim.Systems;

public class DelayedEffectSystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach(var entity in world.Entities.With<DelayedEffectComponent<IStateEffect>>())
        {
            var delayedComp = entity.Get<DelayedEffectComponent<IStateEffect>>();

            delayedComp.Elapsed += (float)delta;

            if(delayedComp.Elapsed >= delayedComp.Delay)
            {
                foreach(var effect in delayedComp.Effects)
                {
                    effect.Apply(entity);
                }
                
                entity.Detach<DelayedEffectComponent<IStateEffect>>();
            }
        }
    }
}