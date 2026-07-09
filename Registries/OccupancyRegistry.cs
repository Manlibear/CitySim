using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Data;

namespace CitySim.Registries;

public static class OccupancyRegistry
{
    private static Dictionary<LocationKey, Guid?> _occupiedLocations;

    public static void Initialize() => _occupiedLocations = [];

    public static List<OccupiedLocationEntry> Get() =>
        [.. _occupiedLocations.Select(x => new OccupiedLocationEntry(x.Key, x.Value))];

    public static void Restore(List<OccupiedLocationEntry> entries)
    {
        _occupiedLocations = [];
        foreach (var entry in entries)
            _occupiedLocations[entry.Location] = entry.EntityID;
    }

    public static bool IsLocationReserved(string locationName, string mapID)
    {
        LocationKey key = new(locationName, mapID);
        return _occupiedLocations.ContainsKey(key) && _occupiedLocations[key] != null;
    }

    public static bool ReserveLocation(string locationName, string mapID, Guid entityID)
    {
        if (IsLocationReserved(locationName, mapID)) return false;

        _occupiedLocations[new(locationName, mapID)] = entityID;
        return true;
    }

    public static void ReleaseLocation(string locationName, string mapID)
    {
        _occupiedLocations[new(locationName, mapID)] = null;
    }

    public static void ReleaseAllLocations(Guid entityID)
    {
        foreach (var key in _occupiedLocations.Where(x => x.Value == entityID).Select(x => x.Key).ToList())
            _occupiedLocations[key] = null;
    }
}
