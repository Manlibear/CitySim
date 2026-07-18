using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Chickensoft.GoDotTest;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Presenters.Person;
using CitySim.Registries;
using CitySim.Scripts;
using CitySim.Systems;
using Xunit;

namespace CitySim.Tests;

// Story-driven integration test: Anna stays put while Ben's schedule sends him across town to
// stand two tiles away from her, linger for half an hour, then walk off again — exercising
// ScheduleSystem, PathfindingSystem, MoveToSystem and SocialSystem together to confirm a full
// social interaction actually lands (paired up, needs/relationship moved, memory recorded).
public class SocialLoopTest(Node testScene) : TestClass(testScene)
{
    private const string MapId = "TestTown";
    private static readonly Vector2I AnnaTile = new(0, 0);
    private static readonly Vector2I BenStartTile = new(10, 0);
    private static readonly Vector2I MeetingTile = new(2, 0); // 2 tiles from Anna — within InteractionTileRange
    private static readonly Vector2I DepartureTile = new(9, 0); // well outside InteractionTileRange of Anna

    private readonly List<string> _story = [];

    private World _world = default!;
    private Entity _anna;
    private Entity _ben;
    private DateTime _startTime;

    [Setup]
    public void Setup()
    {
        _world = new World(42);

        _startTime = new DateTime(2026, 1, 1, 12, 0, 0);
        SimWorld.Instance = new SimWorld
        {
            TimeSpeed = 1f,
            TimeMultiplier = 60f,
            DateTime = _startTime,
        };

        OccupancyRegistry.Initialize();
        QueueRegistry.Initialize(_world);
        WalletRegistry.Initialize();

        // An 11-tile walkable strip. Pass a bare movement layer (just TileSet.TileSize set, no
        // painted cells) so PathfindingSystem/MoveToSystem step Ben across it tile-by-tile
        // instead of teleporting him there in a single tick — we need his WorldPositionComponent
        // to genuinely pass through, and settle at, the meeting tile for SocialSystem's
        // distance checks to mean anything.
        var movementLayer = new TileMapLayer
        {
            TileSet = new TileSet { TileSize = new Vector2I(Globals.TileSize, Globals.TileSize) }
        };

        MapRegistry.RegisterTestMap(MapId,
            Enumerable.Range(0, 11).Select(x => new Vector2I(x, 0)),
            movementLayer: movementLayer);

        LocationRegistry.Register(new Location
        {
            Name = "MeetingSpot",
            Map = MapId,
            EntityID = Guid.NewGuid(),
            Position = new WorldPosition(MapId, MeetingTile),
            Tags = [],
            Type = LocationType.Generic,
            FacingDirection = FacingDirection.South,
        });

        LocationRegistry.Register(new Location
        {
            Name = "FarSide",
            Map = MapId,
            EntityID = Guid.NewGuid(),
            Position = new WorldPosition(MapId, DepartureTile),
            Tags = [],
            Type = LocationType.Generic,
            FacingDirection = FacingDirection.South,
        });

        _anna = _world.CreateEntity();
        _anna.Attach(new NameComponent("Anna", "Testerson"));
        _anna.Attach(new WorldPositionComponent { Position = new WorldPosition(MapId, AnnaTile) });
        _anna.Attach(new CitizenComponent());
        _anna.Attach(new ActivityTypeComponent { Type = ActivityType.Idle });
        _anna.Attach(new NeedsComponent { Satiety = 1f, Energy = 1f, Social = 0.3f });
        _anna.Attach(new FactComponent());
        _anna.Attach(new MemoryComponent());
        _anna.Attach(new RelationshipComponent());
        _anna.Attach(new InterestsComponent());
        WalletRegistry.Register(_anna.Id);

        _ben = _world.CreateEntity();
        _ben.Attach(new NameComponent("Ben", "Testerson"));
        _ben.Attach(new WorldPositionComponent { Position = new WorldPosition(MapId, BenStartTile) });
        _ben.Attach(new CitizenComponent());
        _ben.Attach(new ActivityTypeComponent { Type = ActivityType.Idle });
        _ben.Attach(new NeedsComponent { Satiety = 1f, Energy = 1f, Social = 0.3f });
        _ben.Attach(new FactComponent());
        _ben.Attach(new MemoryComponent());
        _ben.Attach(new RelationshipComponent());
        _ben.Attach(new InterestsComponent());
        _ben.Attach(new GodotNodeComponent { Node = new PersonPresenter { MoveSpeed = 64f } });
        WalletRegistry.Register(_ben.Id);

        // Ben's own itinerary: walk over to stand near Anna, then — half an hour later — head
        // off to the far side of town, well outside interaction range.
        var scheduleComp = new ScheduleComponent();
        scheduleComp.AddEntry(new ScheduleEntry { LocationPath = "/" + MapId + "/MeetingSpot", IsImmediate = true });
        scheduleComp.AddEntry(new ScheduleEntry
        {
            LocationPath = "/" + MapId + "/FarSide",
            Day = _startTime.DayOfWeek,
            Time = TimeOnly.FromDateTime(_startTime.AddMinutes(30)),
        });
        _ben.Attach(scheduleComp);

        // Bias the interaction to succeed deterministically — a fresh, scoreless relationship
        // has only a 50/50 shot per SocialSystem's logistic curve, which would make this test
        // flaky. A strongly positive prior relationship guarantees a positive roll.
        _anna.Get<RelationshipComponent>().UpdateRelationship(_ben.Id, 500f);
        _ben.Get<RelationshipComponent>().UpdateRelationship(_anna.Id, 500f);

        _story.Add("Anna is minding her own business in town.");
        _story.Add("Ben sets off to go say hello.");
    }

    [Test]
    public void BenVisitsAnnaAndTheyHaveASocialInteraction()
    {
        var schedule = new ScheduleSystem(_world);
        var pathfinding = new PathfindingSystem(_world);
        var moveTo = new MoveToSystem(_world);
        var social = new SocialSystem(_world);
        var memory = new MemorySystem(_world);

        var initialAnnaSocial = _anna.Get<NeedsComponent>().Social;
        var initialBenSocial = _ben.Get<NeedsComponent>().Social;

        var headedToMeeting = false;
        var gotPaired = false;
        var headedAway = false;
        var partedWays = false;

        const int minutesToSimulate = 90;

        for (var minute = 0; minute < minutesToSimulate; minute++)
        {
            schedule.Update(1.0);

            if (!headedToMeeting && _ben.TryGet<PathfindingComponent>(out var toMeeting) && toMeeting!.Destination.Tile == MeetingTile)
            {
                headedToMeeting = true;
                _story.Add("Ben heads off toward Anna.");
            }

            pathfinding.Update(1.0);
            moveTo.Update(1.0);
            social.Update(1.0);

            if (!gotPaired && _ben.Has<SocialInteractionComponent>() && _anna.Has<SocialInteractionComponent>())
            {
                gotPaired = true;
                _story.Add("Ben and Anna strike up a conversation.");
            }

            if (gotPaired && !headedAway && _ben.TryGet<PathfindingComponent>(out var toFarSide) && toFarSide!.Destination.Tile == DepartureTile)
            {
                headedAway = true;
                _story.Add("Half an hour on, Ben says his goodbyes and heads off.");
            }

            memory.Update(1.0);

            if (headedAway && !partedWays && !_ben.Has<SocialInteractionComponent>() && !_anna.Has<SocialInteractionComponent>())
            {
                partedWays = true;
                _story.Add("They've drifted out of earshot of each other.");
            }

            SimWorld.Instance.DateTime = SimWorld.Instance.DateTime.AddMinutes(1);

            if (partedWays) break;
        }

        GD.Print("-- SocialLoopTest --");
        foreach (var line in _story)
            GD.Print("- " + line);

        Assert.True(headedToMeeting, "Ben's schedule never sent him toward Anna's meeting spot.");
        Assert.True(gotPaired, "Ben and Anna never picked up a SocialInteractionComponent targeting each other.");
        Assert.True(headedAway, "Ben never headed off toward the far side of town after the visit.");
        Assert.True(partedWays, "Ben and Anna's SocialInteractionComponents were never cleared after separating.");

        var finalAnnaSocial = _anna.Get<NeedsComponent>().Social;
        var finalBenSocial = _ben.Get<NeedsComponent>().Social;
        GD.Print($"= Anna Social: {initialAnnaSocial:F3} -> {finalAnnaSocial:F3}");
        GD.Print($"= Ben Social: {initialBenSocial:F3} -> {finalBenSocial:F3}");

        Assert.True(finalAnnaSocial > initialAnnaSocial, "Anna's Social need never rose from the interaction.");
        Assert.True(finalBenSocial > initialBenSocial, "Ben's Social need never rose from the interaction.");

        var annaToBen = _anna.Get<RelationshipComponent>().GetRelationship(_ben.Id);
        var benToAnna = _ben.Get<RelationshipComponent>().GetRelationship(_anna.Id);
        Assert.NotNull(annaToBen);
        Assert.NotNull(benToAnna);
        Assert.True(annaToBen!.Score > 500f, "Anna's relationship score toward Ben never improved.");
        Assert.True(benToAnna!.Score > 500f, "Ben's relationship score toward Anna never improved.");

        Assert.True(
            _anna.Get<MemoryComponent>().Memories.Any(x => x is SocialInteractionMemory sim && sim.OtherPersonID == _ben.Id),
            "Anna never recorded a memory of the social interaction with Ben.");
        Assert.True(
            _ben.Get<MemoryComponent>().Memories.Any(x => x is SocialInteractionMemory sim && sim.OtherPersonID == _anna.Id),
            "Ben never recorded a memory of the social interaction with Anna.");
    }
}
