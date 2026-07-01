using CitySim.Components;
using CitySim.ECS;
using CitySim.Presenters.Person;

namespace CitySim.Data.StateEffects;

public class FacingDirectionEffect(FacingDirection direction) : IStateEffect
{
    public void Apply(Entity entity)
    {
        if(entity.TryGet<GodotNodeComponent>(out var nodeComp))
        {
            if(nodeComp!.Node is PersonPresenter person)
            {
                person.Facing  = direction;
            }
        }
    }
}