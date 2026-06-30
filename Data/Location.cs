namespace CitySim.Data;

public struct Location(string name, WorldPosition position, string[] tags, LocationType type, FacingDirection facing)
{
    public string Name {get;set;} = name;
    public WorldPosition Position { get; set; } = position;
    public string[] Tags {get;set;} = tags;
    public LocationType Type {get;set;} = type;
    public FacingDirection FacingDirection {get;set;} = facing;
}

public enum LocationType
{
    Generic,
    Home,
    Shop,
    Office
}