using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using Chickensoft.GoDotTest;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Registries;
using CitySim.Scripts;
using CitySim.Systems;
using Xunit;

namespace CitySim.Tests;

// Story-driven integration test: a citizen crosses the hunger threshold, shops for a
// preferred snack, eats it, and ends up measurably less hungry — exercising NeedsSystem,
// ConsumptionSystem, ShopSystem, InventorySystem, PathfindingSystem, StateSystem and
// DelayedEffectSystem together rather than any single one of them in isolation.
public class HungerLoopTest(Node testScene) : TestClass(testScene)
{
    private const string MapId = "TestTown";
    private static readonly Vector2I ShopTile = new(2, 0);
    private static readonly Vector2I DiningTile = new(4, 0);

    private readonly List<string> _story = [];

    private World _world = default!;
    private Entity _mark;
    private Guid _shopId;

    [Setup]
    public void Setup()
    {
        _world = new World();

        SimWorld.Instance = new SimWorld
        {
            TimeSpeed = 1f,
            TimeMultiplier = 60f,
            DateTime = new DateTime(2026, 1, 1, 12, 0, 0),
        };

        WalletRegistry.Initialize();
        InventoryRegistry.Initialize();
        OccupancyRegistry.Initialize();
        QueueRegistry.Initialize(_world);

        // A 5-tile walkable strip: Mark starts at (0,0), the Corner Shop sits at (2,0),
        // the dining bench at (4,0). No real TileMapLayer needed — pathfinding only ever
        // reads the walkable/fence sets, not the layer itself.
        MapRegistry.RegisterTestMap(MapId, [
            new Vector2I(0, 0), new Vector2I(1, 0), ShopTile, new Vector2I(3, 0), DiningTile,
        ]);

        _shopId = Guid.NewGuid();
        LocationRegistry.Register(new Location
        {
            Name = "Corner Shop",
            Map = MapId,
            EntityID = _shopId,
            Position = new WorldPosition(MapId, ShopTile),
            Tags = ["snack"],
            Type = LocationType.Shop,
            FacingDirection = FacingDirection.South,
        });

        LocationRegistry.Register(new Location
        {
            Name = "Bench",
            Map = MapId,
            EntityID = Guid.NewGuid(),
            Position = new WorldPosition(MapId, DiningTile),
            Tags = ["dining"],
            Type = LocationType.Generic,
            FacingDirection = FacingDirection.South,
        });

        var shopInventory = new Inventory();
        shopInventory.Add(Item.BagOfPeanuts, 5, cost: 1.5m);
        InventoryRegistry.Register(_shopId, shopInventory);
        WalletRegistry.Register(_shopId, new Wallet { Balance = 0 });

        _mark = _world.CreateEntity();
        _mark.Attach(new NameComponent("Mark", "Testerson"));
        _mark.Attach(new WorldPositionComponent { Position = new WorldPosition(MapId, Vector2I.Zero) });
        // Right at MinSatietyNeed — natural decay on the very first tick pushes him under it,
        // so he's guaranteed to go hungry this tick rather than needing a warm-up period.
        _mark.Attach(new NeedsComponent { Satiety = 0.3f, Energy = 1f, Social = 1f });
        _mark.Attach(new ActivityTypeComponent { Type = ActivityType.Idle });
        _mark.Attach(new ScheduleComponent());
        _mark.Attach(new HomeComponent(MapId));
        _mark.Attach(new FactComponent());
        _mark.Attach(new MemoryComponent());
        _mark.Attach(new PreferenceComponent
        {
            Preferences = new()
            {
                [ItemType.Food] = new() { ["salty"] = 5f },
            },
        });

        WalletRegistry.Register(_mark.Id, new Wallet { Balance = 20m });
        InventoryRegistry.Register(_mark.Id);

        // Sam works the counter — not mechanically wired to the shop's stock (StaffingComponent
        // doesn't exist yet), just here so the scene isn't a one-citizen town.
        var sam = _world.CreateEntity();
        sam.Attach(new NameComponent("Sam", "Retailer"));
        sam.Attach(new JobComponent { Employer = "Corner Shop", Title = "Cashier" });
        _story.Add("Sam clocks in at the Corner Shop.");
    }

    [Test]
    public void MarkGetsHungryShopsAndEats()
    {
        var pathfinding = new PathfindingSystem(_world);
        var needs = new NeedsSystem(_world);
        var state = new StateSystem(_world);
        var inventory = new InventorySystem(_world);
        var memory = new MemorySystem(_world);
        var consumption = new ConsumptionSystem(_world);
        var shop = new ShopSystem(_world);
        var delayed = new DelayedEffectSystem(_world);

        state.Initialize();

        var initialSatiety = _mark.Get<NeedsComponent>().Satiety;
        var wasHungry = false;
        var headedToShop = false;
        var boughtPeanuts = false;
        var startedEating = false;
        var headedToDine = false;
        var satisfiedAgain = false;

        const int minutesToSimulate = 60;

        for (var minute = 0; minute < minutesToSimulate; minute++)
        {
            needs.Update(1.0);

            if (!wasHungry && _mark.Has<HungerComponent>())
            {
                wasHungry = true;
                _story.Add("Mark's stomach growls — he's hungry.");
            }

            pathfinding.Update(1.0);

            if (!headedToShop && _mark.TryGet<PathfindingComponent>(out var toShop) && toShop!.Destination.Tile == ShopTile)
            {
                headedToShop = true;
                _story.Add("Mark decides to head to the Corner Shop.");
            }

            if (boughtPeanuts && !headedToDine && _mark.TryGet<PathfindingComponent>(out var toBench) && toBench!.Destination.Tile == DiningTile)
            {
                headedToDine = true;
                _story.Add("Mark heads over to the bench to eat.");
            }

            // ActivityType flips to Eat here (via arrival OnArriveEffects) — check before
            // delayed.Update()/state.Update() below can flip it straight back to Idle in the
            // same tick (the peanuts' NeedsDelta.Duration is short enough that they often do).
            if (!startedEating && _mark.Get<ActivityTypeComponent>().Type == ActivityType.Eat)
            {
                startedEating = true;
                _story.Add("Mark sits down and eats the peanuts.");
            }

            consumption.Update(1.0);
            shop.Update(1.0);
            inventory.Update(1.0);

            if (!boughtPeanuts && InventoryRegistry.TryGet(_mark.Id, out var markInventory) && markInventory!.GetAmount(Item.BagOfPeanuts) > 0)
            {
                boughtPeanuts = true;
                _story.Add("Mark picks out a bag of peanuts and pays for it.");
            }

            delayed.Update(1.0);
            state.Update(1.0);
            memory.Update(1.0);

            if (startedEating && !satisfiedAgain && !_mark.Has<HungerComponent>())
            {
                satisfiedAgain = true;
                _story.Add("Mark feels full and gets back to his day.");
            }

            SimWorld.Instance.DateTime = SimWorld.Instance.DateTime.AddMinutes(1);

            Thread.Sleep(2); // give the background pathfinding Task.Run a chance to complete
        }

        foreach (var line in _story)
            GD.Print(line);

        var finalSatiety = _mark.Get<NeedsComponent>().Satiety;
        GD.Print($"Satiety: {initialSatiety:F3} -> {finalSatiety:F3}");

        Assert.True(wasHungry, "Mark never crossed the hunger threshold.");
        Assert.True(headedToShop, "Mark never set off for the Corner Shop.");
        Assert.True(boughtPeanuts, "Mark never ended up with peanuts in his inventory.");
        Assert.True(headedToDine, "Mark never headed to the dining bench.");
        Assert.True(startedEating, "Mark never sat down to eat.");
        Assert.True(satisfiedAgain, "Mark's HungerComponent was never cleared after eating.");

        // Peanuts grant +0.2 Satiety (Data/Globals.cs decay rates are tiny by comparison) —
        // allow slack for the decay that ticked throughout the whole scenario.
        Assert.True(finalSatiety >= initialSatiety + 0.15f,
            $"Expected Satiety to rise by ~0.2 after eating peanuts, went {initialSatiety:F3} -> {finalSatiety:F3}");
    }
}
