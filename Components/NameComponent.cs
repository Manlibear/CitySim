using CitySim.ECS;

namespace CitySim.Components;

public class NameComponent(string firstName, string surname) : IComponent
{
    public string FirstName { get; set; } = firstName;
    public string Surname { get; set; } = surname;
}
