
using CitySim.Data.StateEffects;
using CitySim.ECS;

namespace CitySim.Components;

public class DelayedEffectComponent<T>(float delay, params T[] effect) : IComponent where T : IStateEffect
{
    public T[] Effects { get; } = effect;
    public float Delay { get; } = delay;
    public float Elapsed { get; set; }

}