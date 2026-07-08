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

    public static Location Register(Location location) =>
        _locations[(location.Name, location.Position.MapID)] = location;

    public static void Unregister(Location location) =>
        _locations.Remove((location.Name, location.Position.MapID));

    public static void Unregister(string name, string mapID) =>
        _locations.Remove((name, mapID));

    public static Location? NearestOfType(LocationType locationType, WorldPosition position, Guid[] avoids)
    {
        return _locations.Where(x => x.Value.Position.MapID == position.MapID && x.Value.Type == locationType)
                         .Where(x => !avoids.Contains(x.Value.EntityID))
                         .OrderBy(x => x.Value.Position.Tile.ManhattanDistanceFrom(position.Tile)).FirstOrDefault().Value;
    }

    public static Location? NearestWithTag(string tag, WorldPosition position, Guid[] avoids)
    {
        return _locations.Where(x => x.Value.Position.MapID == position.MapID && (x.Value.Tags?.Contains(tag) ?? false))
                         .Where(x => !avoids.Contains(x.Value.EntityID))
                         .OrderBy(x => x.Value.Position.Tile.ManhattanDistanceFrom(position.Tile)).FirstOrDefault().Value;
    }

    public static Location? GetLocationByTile(WorldPosition position) => _locations.FirstOrDefault(x => x.Value.Position == position).Value;

    public static Location? Resolve(string path, WorldPosition? position = null)
    {
        var pathContent = path[1..];
        var pathParts = pathContent.Split("/");

        var avoids = new Guid[0];
        var avoidsPart = pathContent.Split('!');
        if (avoidsPart.Length > 1)
        {
            avoids = [.. avoidsPart[1].Split(',').Select(static x => Guid.Parse(x))];
        }

        return path[0] switch
        {
            '/' => pathParts.Count() != 2
                ? throw new ArgumentException($"Invalid concrete path {path}")
                : Get(pathParts[1], pathParts[0]),

            '*' when position == null => throw new ArgumentException("Must supply position for LocationType queries"),
            '*' when Enum.TryParse<LocationType>(pathContent, out var locationType) => NearestOfType(locationType, position.Value, avoids),
            '*' => throw new ArgumentException($"Unrecognized location type {pathContent}"),

            '@' when position == null => throw new ArgumentException("Must supply position for tag queries"),
            '@' => NearestWithTag(pathContent, position.Value, avoids),

            _ => throw new ArgumentException($"Unhandled path {path}")
        };
    }
}
