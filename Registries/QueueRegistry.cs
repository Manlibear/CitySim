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

    // Keyed on LocationKey (Name, Map) rather than the whole Location — Location carries a
    // string[] Tags field, and record-generated equality compares arrays by reference, not
    // content, so a deserialized Location (fresh array instances) would never match a live one
    // as a dictionary key.
    private static Dictionary<LocationKey, QueueEntry> _queues;

    public static void Initialize(World world)
    {
        _world = world;
        _queues = [];
    }

    public static List<QueueEntry> Get() => [.. _queues.Values];

    public static void Restore(List<QueueEntry> entries)
    {
        _queues = [];
        foreach (var entry in entries)
            _queues[entry.Location] = entry;
    }

    public static void Register(Location location, QueueSlot?[] slots) => _queues[location] = new QueueEntry(location, slots);

    public static (Vector2I? Tile, int Index) GetNextQueuePositon(Location location)
    {
        if (!_queues.ContainsKey(location)) _queues[location] = new QueueEntry(location, new QueueSlot?[location.MaxQueuePositions]);

        var slots = _queues[location].Slots;
        if (!slots.Any(x => x == null)) return (null, -1);

        var firstEmptyIdx = Array.IndexOf(slots, null);

        return (location.Position.Tile + Vector2IHelper.FromFacingDirection(location.QueueDirection!.Value) * firstEmptyIdx, firstEmptyIdx);
    }

    public static Vector2I? ReserveQueuePosition(Location location, Guid entityID)
    {
        var (tile, idx) = GetNextQueuePositon(location);

        if (tile == null || idx >= location.MaxQueuePositions) return null;

        _queues[location].Slots[idx] = new QueueSlot(tile.Value, entityID);
        return tile.Value;
    }

    public static void ReleaseQueuePosition(Guid entityID, bool activateEffects = true)
    {
        if (_world == null) throw new Exception("Init hasn't been called on QueueRegistry");

        if (!_queues.Any(x => x.Value.Slots.Any(y => y?.EntityID == entityID))) throw new ArgumentException("Unexpected queue location");

        var (_, entry) = _queues.FirstOrDefault(x => x.Value.Slots.Any(y => y?.EntityID == entityID));
        var location = entry.Location;
        var slots = entry.Slots;

        if (!slots.Any(x => x?.EntityID == entityID)) throw new ArgumentException("Unexpected entity in queue");

        var queuePosition = slots.First(x => x?.EntityID == entityID);
        var queuePositionIdx = Array.IndexOf(slots, queuePosition);

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

        slots[queuePositionIdx] = null;

        for (int i = queuePositionIdx; i < location.MaxQueuePositions - 1; i++)
        {
            slots[i] = slots[i + 1];

            if (slots[i] != null)
            {
                _world!.FindEntityByID(slots[i]!.Value.EntityID)?.Attach(new PathfindingComponent()
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
