using System;

namespace CitySim.Data;

public record Location
{

    public Location() { }
    public Location(string name, string map, WorldPosition position, string[] tags, LocationType type, FacingDirection facing, Guid entityID)
    {
        Name = name;
        Map = map;
        Position = position;
        Tags = tags;
        Type = type;
        FacingDirection = facing;
        EntityID = entityID;
    }

    public required string Name { get; set; }
    public required string Map { get; set; }
    public required Guid EntityID { get; set; }
    public required WorldPosition Position { get; set; }
    public required string[] Tags { get; set; }
    public required LocationType Type { get; set; }
    public required FacingDirection FacingDirection { get; set; }
    public int MaxQueuePositions { get; set; } = 1;
    public FacingDirection? QueueDirection { get; set; }
    public int? QueuePosition { get; set; }
}

public record LocationKey(string Name, string Map)
{
    public static implicit operator LocationKey(Location location) => new(location.Name, location.Map);
}

public enum LocationType
{
    Generic,
    Home,
    Shop,
    Office
}
