using System;
using System.Collections.Generic;
using CitySim.Components;

namespace CitySim.Data;

public class SaveGameData
{
    public DateTime WorldTime { get; set; }
    public required List<CitizenSaveData> Citizens { get; set; }
}

public class CitizenSaveData
{
    public required NameComponent Name { get; set; }
    public WorldPosition Position { get; set; }
    public string? HomeMap { get; set; }
    public required NeedsComponent Needs { get; set; }
    public required ActivityTypeComponent ActivityType { get; set; }
    public required List<ScheduleEntry> Schedule { get; set; }
    public PathfindingComponent? Pathfinding { get; set; }
}