using System;

namespace CitySim.Data;

public struct Location(string name, WorldPosition position, string[] tags, LocationType type, FacingDirection facing)
{
    public string Name { get; set; } = name;
    public required Guid EntityID { get; set; }
    public WorldPosition Position { get; set; } = position;
    public string[] Tags { get; set; } = tags;
    public LocationType Type { get; set; } = type;
    public FacingDirection FacingDirection { get; set; } = facing;
    public int MaxQueuePositions { get; set; } = 1;
    public FacingDirection QueueDirection { get; set; }
}

public enum LocationType
{
    Generic,
    Home,
    Shop,
    Office
}
