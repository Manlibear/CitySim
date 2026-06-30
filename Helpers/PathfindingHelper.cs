using System;
using System.Collections.Generic;
using Godot;
using CitySim.Data;
using CitySim.Registries;

namespace CitySim.Helpers;

public static class PathfindingHelper
{
    private static readonly Vector2I[] Neighbours =
    [
        new Vector2I(0, -1), new Vector2I(0, 1), new Vector2I(-1, 0), new Vector2I(1, 0)
    ];

    public static Queue<WorldPosition> FindPath(string MapID, Vector2I start, Vector2I goal)
    {
        var walkable = MapRegistry.GetWalkable(MapID);
        var fenceEdges = MapRegistry.GetFenceEdges(MapID);
        if (walkable == null || fenceEdges == null) return [];
        if (!walkable.Contains(start) || !walkable.Contains(goal)) return [];

        var openSet = new PriorityQueue<Vector2I, int>();
        openSet.Enqueue(start, 0);

        var cameFrom = new Dictionary<Vector2I, Vector2I>();
        var costSoFar = new Dictionary<Vector2I, int> { [start] = 0 };

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(MapID, cameFrom, current);

            foreach (var offset in Neighbours)
            {
                var next = current + offset;
                if (!walkable.Contains(next)) continue;
                if (fenceEdges.Contains((current, next))) continue;

                var newCost = costSoFar[current] + 1;
                if (costSoFar.TryGetValue(next, out var existing) && newCost >= existing) continue;

                costSoFar[next] = newCost;
                cameFrom[next] = current;
                openSet.Enqueue(next, newCost + Heuristic(next, goal));
            }
        }

        return [];
    }

    // Cross-map pathfinding. Uses the overworld as a routing hub when there is no direct warp.
    public static Queue<WorldPosition> FindCrossLevelPath(string fromMapID, Vector2I start, WorldPosition to)
    {
        if (fromMapID == to.MapID)
            return FindPath(fromMapID, start, to.Tile);

        // Direct warp between the two maps?
        var directWarp = MapRegistry.GetWarpTile(fromMapID, to.MapID);
        if (directWarp != null)
        {
            var path = FindPath(fromMapID, start, directWarp.Value.Tile);
            var arrival = MapRegistry.GetPairedWarpTile(fromMapID, to.MapID);
            if (arrival != null)
                Merge(path, FindPath(to.MapID, arrival.Value.Tile, to.Tile));
            return path;
        }

        // Route through the overworld as a hub.
        var toOverworld = MapRegistry.GetWarpTile(fromMapID, MapRegistry.OverworldId);
        if (toOverworld == null) return [];

        var result = FindPath(fromMapID, start, toOverworld.Value.Tile);

        var fromOverworld = MapRegistry.GetPairedWarpTile(fromMapID, MapRegistry.OverworldId);
        var overworldWarp = MapRegistry.GetWarpTile(MapRegistry.OverworldId, to.MapID);
        if (fromOverworld == null || overworldWarp == null) return result;

        Merge(result, FindPath(MapRegistry.OverworldId, fromOverworld.Value.Tile, overworldWarp.Value.Tile));

        var intoTarget = MapRegistry.GetPairedWarpTile(MapRegistry.OverworldId, to.MapID);
        if (intoTarget != null)
            Merge(result, FindPath(to.MapID, intoTarget.Value.Tile, to.Tile));

        return result;
    }

    private static void Merge(Queue<WorldPosition> into, Queue<WorldPosition> from)
    {
        foreach (var step in from)
            into.Enqueue(step);
    }

    private static Queue<WorldPosition> ReconstructPath(string MapID, Dictionary<Vector2I, Vector2I> cameFrom, Vector2I current)
    {
        var reversed = new List<WorldPosition> { new WorldPosition(MapID, current) };

        while (cameFrom.TryGetValue(current, out var previous))
        {
            current = previous;
            reversed.Add(new WorldPosition(MapID, current));
        }

        reversed.Reverse();

        // Drop the start tile (character is already there).
        var path = new Queue<WorldPosition>();
        foreach (var step in reversed[1..])
            path.Enqueue(step);

        return path;
    }

    private static int Heuristic(Vector2I a, Vector2I b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
}
