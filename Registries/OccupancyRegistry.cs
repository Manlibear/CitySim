using System;
using System.Collections.Generic;
using System.Linq;

namespace CitySim.Registries;

public static class OccupancyRegistry
{
    private static Dictionary<(string LocationName, string MapID), Guid?> _occupiedLocations = [];

    public static bool IsLocationReserved(string locationName, string mapID)
    {
        return _occupiedLocations.ContainsKey((locationName, mapID)) && _occupiedLocations[(locationName, mapID)] != null;
    }

    public static bool ReserveLocation(string locationName, string mapID, Guid entityID)
    {
        if (IsLocationReserved(locationName, mapID)) return false;

        _occupiedLocations[(locationName, mapID)] = entityID;
        return true;
    }

    public static void ReleaseLocation(string locationName, string mapID)
    {
        _occupiedLocations[(locationName, mapID)] = null;
    }

    public static void ReleaseAllLocations(Guid entityID)
    {
        foreach (var key in _occupiedLocations.Where(x => x.Value == entityID).Select(x => x.Key).ToList())
            _occupiedLocations[key] = null;
    }
}
