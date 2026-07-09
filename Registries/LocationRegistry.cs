using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Data;
using CitySim.Helpers;

namespace CitySim.Registries;

public static class LocationRegistry
{
    static readonly Dictionary<(string Name, string MapID), Location> _locations = [];
    static readonly Dictionary<(string GroupName, string MapID), List<Location>> _groups = [];

    public static Location? Get(string name, string map)
    {
        if (name.EndsWith("*"))
        {
            // location group
            var foundGroup = _groups.Where(x => x.Key.MapID == map && x.Key.GroupName == name[..^1]);
            if (!foundGroup.Any()) return null;

            return foundGroup.First().Value
                .Select(TryResolveOccupancy)
                .FirstOrDefault(loc => loc != null);
        }

        return _locations.TryGetValue((name, map), out var found) ? TryResolveOccupancy(found) : null;
    }

    // Single-slot locations are free/reserved wholesale; queueable ones hand back a copy with a
    // reserved queue tile baked into Position — null means "not available, try the next candidate".
    private static Location? TryResolveOccupancy(Location loc)
    {
        if (loc.MaxQueuePositions == 1)
            return OccupancyRegistry.IsLocationReserved(loc.Name, loc.Map) ? null : loc;

        var queuePos = QueueRegistry.GetNextQueuePositon(loc);
        return queuePos.Tile == null
            ? null
            : loc with { Position = new WorldPosition(loc.Map, queuePos.Tile.Value), QueuePosition = queuePos.Index };
    }
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

    public static void AddGroupEntry(string groupName, string locationName, string mapID)
    {
        if (!_groups.ContainsKey((groupName, mapID)))
            _groups[(groupName, mapID)] = [];

        _groups[(groupName, mapID)].Add(_locations[(locationName, mapID)]);
    }

    public static List<Location> GetLocationsInGroup(string groupName, string mapID)
    {
        if (_groups.ContainsKey((groupName, mapID))) return _groups[(groupName, mapID)];

        return [];
    }

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

            '@' when position == null => throw new ArgumentException("Must supply position for LocationType queries"),
            '@' when Enum.TryParse<LocationType>(pathContent, out var locationType) => NearestOfType(locationType, position.Value, avoids),
            '@' => throw new ArgumentException($"Unrecognized location type {pathContent}"),

            '#' when position == null => throw new ArgumentException("Must supply position for tag queries"),
            '#' => NearestWithTag(pathContent, position.Value, avoids),

            _ => throw new ArgumentException($"Unhandled path {path}")
        };
    }
}
