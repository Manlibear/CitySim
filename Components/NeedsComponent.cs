using System;
using System.Collections.Generic;
using System.Net.Http.Metrics;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;

public class NeedsComponent : IComponent
{
    public float Satiety { get; set; } = 1;
    public float Energy { get; set; } = 1;
    public float Social { get; set; } = 1;

    public DateTime? LastSatietySchedule { get; set; }
    public DateTime? LastEnergySchedule { get; set; }
    public DateTime? LastSocialSchedule { get; set; }

    public List<NeedsDelta> NeedsDeltas { get; set; } = [];

    public float Sum()
    {
        return Satiety + Social + Energy - 1.5f;
    }
}
