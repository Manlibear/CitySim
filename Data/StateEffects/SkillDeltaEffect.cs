using System.Collections.Generic;
using CitySim.Components;
using CitySim.ECS;

namespace CitySim.Data.StateEffects;

public class SkillDeltaEffect(Dictionary<Skill, float> skillsDelta) : IStateEffect
{
    private Dictionary<Skill, float> SkillsDelta { get; set; } = skillsDelta;
    public void Apply(Entity entity, params object[] info)
    {
        if (entity.TryGet<SkillsComponent>(out var skillsComp))
        {
            foreach (var sd in SkillsDelta ?? [])
                skillsComp!.IncreaseSkill(sd.Key, sd.Value);
        }
    }
}
