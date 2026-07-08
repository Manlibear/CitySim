using CitySim.ECS;

namespace CitySim.Components;

public class JobComponent : IComponent
{
    public required string Employer { get; set; }
    public required string Title { get; set; }
}
