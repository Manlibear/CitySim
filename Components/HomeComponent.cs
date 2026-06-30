using CitySim.ECS;

namespace CitySim.Components;

public class HomeComponent(string mapID) : IComponent
{
    public string MapID {get;set;} = mapID;
}