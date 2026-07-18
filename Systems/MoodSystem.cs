using System;
using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Scripts;

namespace CitySim.Systems;

public class MoodSystem(World world, float needsModifier = Globals.MoodNeedsModifier, float memoryMultiplier = Globals.MoodMemoryMultiplier) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<MoodComponent>().With<MemoryComponent>())
        {
            var moodComp = entity.Get<MoodComponent>();
            var memoryComp = entity.Get<MemoryComponent>();
            var needsComp = entity.Get<NeedsComponent>();

            var memorySum = memoryComp.Memories.Sum(x => x.Satisfaction) * memoryMultiplier;
            var needsSum = needsComp.Sum() * needsModifier;

            moodComp.Mood = Math.Clamp(memorySum + needsSum, 0f, 1f);

            var now = SimWorld.Instance.DateTime;
            if (moodComp.LastSampleTime == null || (now - moodComp.LastSampleTime.Value).TotalMinutes >= Globals.MoodSampleIntervalMinutes)
            {
                moodComp.History.Add(new MoodSample { Timestamp = now, Mood = moodComp.Mood });
                moodComp.LastSampleTime = now;

                if (moodComp.History.Count > Globals.MoodHistoryMaxSamples)
                    moodComp.History.RemoveRange(0, moodComp.History.Count - Globals.MoodHistoryMaxSamples);
            }
        }
    }
}
