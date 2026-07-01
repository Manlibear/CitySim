using CitySim.Components;
using CitySim.ECS;
using CitySim.Presenters.Person;

namespace CitySim.Data.StateEffects;

public class PlayAnimationEffect(string name) : IStateEffect
{
    private string Name {get;set;} = name;

    public void Apply(Entity entity)
    {
        if(entity.TryGet<GodotNodeComponent>(out var compNode))
        {
            if(compNode!.Node is PersonPresenter person)
            {
                person.PlayAnimation(Name);
            }
        }
    }
}