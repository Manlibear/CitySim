using CitySim.Components;
using CitySim.ECS;

namespace CitySim.Data.StateEffects;

public class NeedsSatisfierEffect : IStateEffect
{
    public required NeedsDelta? NeedsDelta { get; set; }

    void IStateEffect.Apply(Entity entity, params object[] info)
    {
        var needsComp = entity.Get<NeedsComponent>();
        if(NeedsDelta != null)
            needsComp.NeedsDeltas.Add(NeedsDelta);
    }
}
