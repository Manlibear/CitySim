
using CitySim.Data.StateEffects;
using CitySim.ECS;

namespace CitySim.Components;

public class DelayedEffectComponent : IComponent
{
    public IStateEffect[]? Effects { get; set; }
    public float Delay { get; set;}
    public float Elapsed { get; set; }

}
