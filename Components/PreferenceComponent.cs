using System.Collections.Generic;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;

public class PreferenceComponent : IComponent
{
    public Dictionary<ItemType, Dictionary<string, float>> Preferences { get; set; } = [];
}
