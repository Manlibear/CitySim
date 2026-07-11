using CitySim.ECS;

namespace CitySim.Components;

public class JobApplicationResultComponent : IComponent
{
    public string Employer { get; set; } = null!;
    public string Job { get; set; } = null!;
}
