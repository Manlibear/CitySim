using CitySim.ECS;

namespace CitySim.Components;

public class JobApplicantComponent : IComponent
{
    public string Employer { get; set; } = null!;
    public string Job { get; set; } = null!;
}
