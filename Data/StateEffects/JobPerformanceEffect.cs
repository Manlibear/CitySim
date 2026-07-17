using System;
using System.Linq;
using CitySim.Components;
using CitySim.ECS;
using CitySim.Registries;
using Godot;

namespace CitySim.Data.StateEffects;

public class JobPerformanceEffect : IStateEffect
{
    public void Apply(Entity entity, params object[] info)
    {
        if (!entity.TryGet<JobComponent>(out var jobComp)) throw new ArgumentException("JobPerformanceEffect without JobComponent");
        if (!entity.TryGet<SkillsComponent>(out var skillsComp)) throw new ArgumentException("JobPerformanceEffect without SkillsComponent");
        if (!entity.TryGet<MoodComponent>(out var moodComp)) throw new ArgumentException("JobPerformanceEffect without MoodComponent");

        var requiredSkills = EmployerRegistry.GetJobSkills(jobComp!.Employer, jobComp.Title);

        var performanceModifier = 0f;

        foreach (var skill in requiredSkills)
        {
            performanceModifier += (1 - skillsComp!.GetSkill(skill.Key) / skill.Value) * .5f;
        }

        // TOOD: Factor mood into it once mood is a thing

        var minPerformance = -requiredSkills.Sum(x => x.Value * .05f);
        jobComp.Performance += new RandomNumberGenerator().RandfRange(minPerformance, performanceModifier) + performanceModifier;

    }
}
