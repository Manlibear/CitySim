using CitySim.Components;
using CitySim.ECS;

namespace CitySim.Data.StateEffects;

public class NeedsSatisfierEffect : IStateEffect
{
    public required NeedsDelta[]? NeedsDeltas { get; set; }

    void IStateEffect.Apply(Entity entity, params object[] info)
    {
        var needsComp = entity.Get<NeedsComponent>();
        foreach (var nd in NeedsDeltas ?? [])
            needsComp.NeedsDeltas.Add(nd);
    }
}
