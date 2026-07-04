using System.Collections.Generic;
using System.Linq;
using Godot;
using CitySim.Data;
using System.Drawing;
using System;

namespace CitySim.Registries;

public static class MapRegistry
{
    public const string OverworldId = "Overworld";

    private sealed class MapData
    {
        public TileMapLayer PrimaryLayer { get; }
        public HashSet<Vector2I> Walkable { get; } = [];

        // Blocked directed edges: (from, to) means you cannot step from `from` to `to`.
        // Fences are always added bidirectionally.
        public HashSet<(Vector2I From, Vector2I To)> FenceEdges { get; } = [];

        // Each entry: MapID = destination map, Tile = warp tile position in THIS map
        public List<WorldPosition> Warps { get; } = [];

        public MapData(TileMapLayer[] layers)
        {
            PrimaryLayer = layers[0];
            foreach (var layer in layers)
            {
                ScanWalkability(layer, Walkable, Vector2I.Zero);
                ScanFenceEdges(layer, FenceEdges, Vector2I.Zero);
            }
        }

        public void Append(TileMapLayer[] layers, Vector2I offset)
        {
            foreach (var layer in layers)
            {
                ScanWalkability(layer, Walkable, offset);
                ScanFenceEdges(layer, FenceEdges, offset);
            }
        }

        private static void ScanWalkability(TileMapLayer layer, HashSet<Vector2I> walkable, Vector2I offset)
        {
            if (layer.TileSet == null || layer.TileSet.GetNavigationLayersCount() == 0) return;

            foreach (var cell in layer.GetUsedCells())
            {
                var data = layer.GetCellTileData(cell);
                if (data?.GetNavigationPolygon(0) != null)
                    walkable.Add(cell + offset);
            }
        }

        // fence_south on (x,y) blocks movement between (x,y) and (x,y+1).
        // fence_east  on (x,y) blocks movement between (x,y) and (x+1,y).
        // Both are bidirectional — mark the tile on either side of the fence.
        private static void ScanFenceEdges(TileMapLayer layer, HashSet<(Vector2I, Vector2I)> edges, Vector2I offset)
        {
            var tileSet = layer.TileSet;
            if (tileSet == null) return;

            var hasSouth = tileSet.GetCustomDataLayerByName("fence_south") >= 0;
            var hasEast = tileSet.GetCustomDataLayerByName("fence_east") >= 0;
            if (!hasSouth && !hasEast) return;

            foreach (var cell in layer.GetUsedCells())
            {
                var data = layer.GetCellTileData(cell);
                if (data == null) continue;

                if (hasSouth && data.GetCustomData("fence_south").AsBool())
                {
                    var south = cell + new Vector2I(0, 1);
                    edges.Add((cell + offset, south + offset));
                    edges.Add((south + offset, cell + offset));
                }

                if (hasEast && data.GetCustomData("fence_east").AsBool())
                {
                    var east = cell + new Vector2I(1, 0);
                    edges.Add((cell + offset, east + offset));
                    edges.Add((east + offset, cell + offset));
                }
            }
        }
    }

    private static readonly Dictionary<string, MapData> _maps = [];

    // Pass any number of layers: walkability is read from nav polygons, fences from custom data.
    // All layers are scanned for both, so a dedicated fence layer just works alongside ground layers.
    public static void Register(string mapID, params TileMapLayer[] layers)
    {
        if (layers.Length == 0) return;
        _maps[mapID] = new MapData(layers);
    }

    public static void RegisterSubMap(string mapID, Vector2I location,  params TileMapLayer[] layers)
    {
        if(layers.Length == 0) return;

        if(!_maps.ContainsKey(mapID)) throw new ArgumentException($"Base Map {mapID} not reqistered yet");

        _maps[mapID].Append(layers, location);

    }

    public static void Unregister(string MapID) => _maps.Remove(MapID);

    public static bool HasMap(string MapID) => _maps.ContainsKey(MapID);

    public static Node GetMapInstance(string MapID) => GetLayer(MapID)!.GetParent();

    public static TileMapLayer? GetLayer(string MapID) =>
        _maps.TryGetValue(MapID, out var data) ? data.PrimaryLayer : null;

    // Registers a warp tile: standing on `fromTile` in `fromMapID` transitions to `destination`.

    public static void AddWarp(WorldPosition from, WorldPosition destination) => AddWarp(from.MapID, from.Tile, destination);
    public static void AddWarp(string fromMapID, Vector2I fromTile, WorldPosition destination)
    {
        if (!_maps.TryGetValue(fromMapID, out var data)) return;
        data.Warps.Add(new WorldPosition(destination.MapID, fromTile));
    }

    // Tile in `fromMapID` that warps to `toMapID`. Null if no direct warp exists.
    public static WorldPosition? GetWarpTile(string fromMapID, string toMapID)
    {
        if (!_maps.TryGetValue(fromMapID, out var data)) return null;
        return data.Warps.Cast<WorldPosition?>().FirstOrDefault(w => w!.Value.MapID == toMapID);
    }

    // Arrival tile in `targetMapID` for travellers coming from `sourceMapID`.
    public static WorldPosition? GetPairedWarpTile(string sourceMapID, string targetMapID)
    {
        if (!_maps.TryGetValue(targetMapID, out var data)) return null;
        return data.Warps.Cast<WorldPosition?>().FirstOrDefault(w => w!.Value.MapID == sourceMapID);
    }

    public static WorldPosition? IsOnWarpTile(string MapID, Vector2I tile)
    {
        if (!_maps.TryGetValue(MapID, out var data)) return null;
        return data.Warps.Cast<WorldPosition?>().FirstOrDefault(w => w!.Value.Tile == tile);
    }

    public static HashSet<Vector2I>? GetWalkable(string MapID) =>
        _maps.TryGetValue(MapID, out var data) ? data.Walkable : null;

    public static HashSet<(Vector2I, Vector2I)>? GetFenceEdges(string MapID) =>
        _maps.TryGetValue(MapID, out var data) ? data.FenceEdges : null;

    public static void MarkBlocked(string MapID, IEnumerable<Vector2I> tiles)
    {
        if (!_maps.TryGetValue(MapID, out var data)) return;
        foreach (var tile in tiles)
            data.Walkable.Remove(tile);
    }
}
