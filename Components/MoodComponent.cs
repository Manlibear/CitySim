using System;
using System.Collections.Generic;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;

public class MoodComponent : IComponent
{
    public float Mood { get; set; }
    public List<MoodSample> History { get; set; } = [];
    public DateTime? LastSampleTime { get; set; }
}
