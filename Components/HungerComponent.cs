using CitySim.ECS;

namespace CitySim.Components;

public class HungerComponent(string tag) : IComponent
{
    public string Tag { get; set; } = tag;
}
