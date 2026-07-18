using System;
using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Registries;
using CitySim.Scripts;
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

        // Average mood across the shift just worked (since the last review), rather than the
        // instantaneous mood at knock-off time, so a shift that trended down doesn't get scored
        // as if they'd felt great the whole way through.
        var shiftMoodSamples = moodComp!.History.Where(x => jobComp.LastPerformanceReviewTime == null || x.Timestamp > jobComp.LastPerformanceReviewTime).ToList();
        var averageShiftMood = shiftMoodSamples.Count > 0 ? shiftMoodSamples.Average(x => x.Mood) : moodComp.Mood;
        jobComp.LastPerformanceReviewTime = SimWorld.Instance.DateTime;

        // Mood is 0-1, so centre it on .5f - a sour shift drags performance down, a good one lifts it.
        var moodModifier = (averageShiftMood - .5f) * Globals.MoodJobPerformanceModifier;

        var minPerformance = -requiredSkills.Sum(x => x.Value * .05f) + moodModifier;
        jobComp.Performance += new RandomNumberGenerator().RandfRange(minPerformance, performanceModifier + moodModifier) + performanceModifier;

    }
}
