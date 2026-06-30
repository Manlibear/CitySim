using CitySim.ECS;
using CitySim.Data;

namespace CitySim.Components;

public class WorldPositionComponent : IComponent
{
    public WorldPosition Position { get; set; }
}
