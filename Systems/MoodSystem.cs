using System;
using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Systems;

public class MoodSystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<MoodComponent>().With<MemoryComponent>())
        {
            var moodComp = entity.Get<MoodComponent>();
            var memoryComp = entity.Get<MemoryComponent>();
            var needsComp = entity.Get<NeedsComponent>();

            var memorySum = memoryComp.Memories.Sum(x => x.Satisfaction) * Globals.MoodMemoryMultiplier;
            var needsSum = needsComp.Sum() * Globals.MoodNeedsModifier;

            moodComp.Mood = Math.Clamp(memorySum + needsSum, 0f, 1f);

        }
    }
}
