using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Data;
using CitySim.Helpers;

namespace CitySim.Registries;

public static class LocationRegistry
{
    static readonly Dictionary<(string Name, string MapID), Location> _locations = [];

    public static Location? Get(string name, string map) =>
        _locations.TryGetValue((name, map), out var loc) ? loc : null;

    public static bool TryGet(string name, string map, out Location? loc)
    {
        loc = Get(name, map);
        return loc != null;
    }

    public static void Register(Location location) =>
        _locations[(location.Name, location.Position.MapID)] = location;

    public static void Unregister(Location location) =>
        _locations.Remove((location.Name, location.Position.MapID));

    public static void Unregister(string name, string mapID) =>
        _locations.Remove((name, mapID));

    public static Location? NearestOfType(LocationType locationType, WorldPosition position)
    {
        return _locations.Where(x => x.Value.Position.MapID == position.MapID && x.Value.Type == locationType)
                  .OrderBy(x => x.Value.Position.Tile.ManhattanDistanceFrom(position.Tile)).FirstOrDefault().Value;
    }

    public static Location? NearestWithTag(string tag, WorldPosition position)
    {
        return _locations.Where(x => x.Value.Position.MapID == position.MapID && (x.Value.Tags?.Contains(tag) ?? false))
                  .OrderBy(x => x.Value.Position.Tile.ManhattanDistanceFrom(position.Tile)).FirstOrDefault().Value;
    }

    public static Location? Resolve(string path, WorldPosition? position = null)
    {
        var pathContent = path[1..];
        var pathParts = pathContent.Split("/");
        return path[0] switch
        {
            '/' => pathParts.Count() != 2
                ? throw new ArgumentException($"Invalid concrete path {path}")
                : Get(pathParts[1], pathParts[0]),

            '*' when position == null => throw new ArgumentException("Must supply position for LocationType queries"),
            '*' when Enum.TryParse<LocationType>(pathContent, out var locationType) => NearestOfType(locationType, position.Value),
            '*' => throw new ArgumentException($"Unrecognized location type {pathContent}"),

            '@' when position == null => throw new ArgumentException("Must supply position for tag queries"),
            '@' => NearestWithTag(pathContent, position.Value),

            _ => throw new ArgumentException($"Unhandled path {path}")
        };
    }
}