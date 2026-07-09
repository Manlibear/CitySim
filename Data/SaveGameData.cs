using System;
using System.Collections.Generic;
using CitySim.Components;
using CitySim.Data.Facts;
using CitySim.Registries;

namespace CitySim.Data;

public class SaveGameData
{
    public DateTime WorldTime { get; set; }
    public required List<CitizenSaveData> Citizens { get; set; }
    public required RegistrySaveData Registries { get; set; }
}

public class CitizenSaveData
{
    public required Guid Id { get; set; }
    public required NameComponent Name { get; set; }
    public WorldPosition Position { get; set; }
    public string? HomeMap { get; set; }
    public required NeedsComponent Needs { get; set; }
    public JobComponent? Job { get; set; }
    public required IFact[] Fact { get; set; }
    public required ActivityTypeComponent ActivityType { get; set; }
    public required List<ScheduleEntry> Schedule { get; set; }
    public PathfindingComponent? Pathfinding { get; set; }
    public required Wallet Wallet { get; set; }
    public required MemoryComponent MemoryComponent { get; set; }
    public SleepComponent? SleepComponent { get; set; }
    public TiredComponent? TiredComponent { get; set; }
    public HungerComponent? HungerComponent { get; set; }
    public BrowseShopComponent? BrowseShopComponent { get; set; }
    public DelayedEffectComponent? DelayedEffectComponent { get; set; }
    public required PreferenceComponent PreferenceComponent {get;set;}
}

public class RegistrySaveData
{
    public required Dictionary<Guid, Wallet> Wallets { get; set; }
    public required Dictionary<Guid, Inventory> Inventories { get; set; }
    // Dictionaries need string/numeric/Guid/enum keys to serialize — tuple and struct keys don't,
    // so occupancy/queue state round-trips as a flat list of entries instead.
    public required List<OccupiedLocationEntry> OccupiedLocations { get; set; }
    public required List<QueueEntry> Queues { get; set; }
}

public record OccupiedLocationEntry(LocationKey Location, Guid? EntityID);
public record QueueEntry(Location Location, QueueSlot?[] Slots);
