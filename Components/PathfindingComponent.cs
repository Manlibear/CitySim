using System.Collections.Generic;
using System.Threading.Tasks;
using CitySim.ECS;
using CitySim.Data;

namespace CitySim.Components;

public enum PathfindingStatus { Pending, Resolving, Moving }

// Attach this with a Destination to trigger pathfinding. The system does the rest.
public class PathfindingComponent : IComponent
{
    public required WorldPosition Destination { get; init; }
    public PathfindingStatus Status { get; set; } = PathfindingStatus.Pending;
    public Queue<WorldPosition>? Path { get; set; }
    public Task<Queue<WorldPosition>>? PendingTask { get; set; }
    public StatePayload? StatePayload { get; set; } = null;
}
