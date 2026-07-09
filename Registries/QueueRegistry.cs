using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.ECS;
using CitySim.Helpers;
using Godot;

namespace CitySim.Registries;

public static class QueueRegistry
{
    private static World? _world;
    private static Dictionary<Location, QueueSlot?[]> _queues = [];

    public static void Init(World world)
    {
        _world = world;
    }

    public static (Vector2I? Tile, int Index) GetNextQueuePositon(Location location)
    {
        if (!_queues.ContainsKey(location)) _queues.Add(location, new QueueSlot?[location.MaxQueuePositions]);

        if (!_queues[location].Any(x => x == null)) return (null, -1);

        var firstEmptyIdx = Array.IndexOf(_queues[location], null);

        return (location.Position.Tile + Vector2IHelper.FromFacingDirection(location.QueueDirection!.Value) * firstEmptyIdx, firstEmptyIdx);
    }

    public static Vector2I? ReserveQueuePosition(Location location, Guid entityID)
    {
        var (tile, idx) = GetNextQueuePositon(location);

        if(tile == null || idx >= location.MaxQueuePositions) return null;

        _queues[location][idx] = new QueueSlot(tile.Value, entityID);
        return tile.Value;
    }

    public static void ReleaseQueuePosition(Guid entityID, bool activateEffects = true)
    {
        if (_world == null) throw new Exception("Init hasn't been called on QueueRegistry");

        if (!_queues.Any(x => x.Value.Any(y => y?.EntityID == entityID))) throw new ArgumentException("Unexpected queue location");

        var location = _queues.FirstOrDefault(x => x.Value.Any(x => x?.EntityID == entityID)).Key;

        if (!_queues[location].Any(x => x?.EntityID == entityID)) throw new ArgumentException("Unexpected entity in queue");

        var queuePosition = _queues[location].First(x => x?.EntityID == entityID);
        var queuePositionIdx = Array.IndexOf(_queues[location], queuePosition);

        if (activateEffects)
        {
            if (queuePosition!.Value.OnArriveEffects != null)
            {
                var entity = _world!.FindEntityByID(queuePosition!.Value.EntityID);

                if (entity != null)
                {

                    foreach (var effect in queuePosition!.Value.OnArriveEffects)
                    {
                        effect.Apply(entity.Value);
                    }
                }
            }
        }

        _queues[location][queuePositionIdx] = null;

        for (int i = queuePositionIdx; i < location.MaxQueuePositions - 1; i++)
        {
            _queues[location][i] = _queues[location][i + 1];

            if (_queues[location][i] != null)
            {
                _world!.FindEntityByID(_queues[location][i]!.Value.EntityID)?.Attach(new PathfindingComponent()
                {
                    Destination = new WorldPosition(location.Position.MapID,
                                                       location.Position.Tile + (Vector2IHelper.FromFacingDirection(location.QueueDirection!.Value) * i))
                });
            }
        }
    }
}

public struct QueueSlot(Vector2I position, Guid entity)
{
    public Vector2I Position { get; set; } = position;
    public Guid EntityID { get; set; } = entity;
    public IStateEffect[]? OnArriveEffects { get; set; } = null;
    public bool Occupied { get; set; } = false;

}
