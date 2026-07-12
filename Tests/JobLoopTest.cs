using System;
using System.Collections.Generic;
using System.Linq;
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

// Story-driven integration test: an unemployed citizen picks up JobSeekingComponent, applies
// to the one vacancy in town, crosses a map warp to interview for it, and (being wildly
// overqualified so the RNG-driven interview roll can't fail her) walks away hired — exercising
// JobSystem, PathfindingSystem, StateSystem and MemorySystem together.
public class JobLoopTest(Node testScene) : TestClass(testScene)
{
    private const string EmployerName = "Kwik-E-Mart";
    private static readonly Vector2I HomeTile = new(0, 0);
    private static readonly Vector2I EmployerDoorTile = new(5, 0);
    private static readonly Vector2I InteriorEntryTile = new(0, 0);
    private static readonly Vector2I ManagerDeskTile = new(1, 0);

    private readonly List<string> _story = [];

    private World _world = default!;
    private Entity _jamie;

    [Setup]
    public void Setup()
    {
        _world = new World();

        SimWorld.Instance = new SimWorld
        {
            TimeSpeed = 1f,
            TimeMultiplier = 60f,
            DateTime = new DateTime(2026, 1, 1, 9, 0, 0),
        };

        // Overworld: a walkable strip from home (0,0) to the employer's front door (5,0).
        MapRegistry.RegisterTestMap(MapRegistry.OverworldId, [
            HomeTile, new Vector2I(1, 0), new Vector2I(2, 0), new Vector2I(3, 0), new Vector2I(4, 0), EmployerDoorTile,
        ]);

        // Employer interior: the tile the overworld warp lands on, through to the manager's desk.
        MapRegistry.RegisterTestMap(EmployerName, [InteriorEntryTile, ManagerDeskTile]);

        MapRegistry.AddWarp(new WorldPosition(MapRegistry.OverworldId, EmployerDoorTile), new WorldPosition(EmployerName, InteriorEntryTile));
        MapRegistry.AddWarp(new WorldPosition(EmployerName, InteriorEntryTile), new WorldPosition(MapRegistry.OverworldId, EmployerDoorTile));

        LocationRegistry.Register(new Location
        {
            Name = "Home",
            Map = MapRegistry.OverworldId,
            EntityID = Guid.NewGuid(),
            Position = new WorldPosition(MapRegistry.OverworldId, HomeTile),
            Tags = [],
            Type = LocationType.Home,
            FacingDirection = FacingDirection.South,
        });

        // The employer's front door — this is what JobSystem measures commute distance from.
        LocationRegistry.Register(new Location
        {
            Name = EmployerName,
            Map = MapRegistry.OverworldId,
            EntityID = Guid.NewGuid(),
            Position = new WorldPosition(MapRegistry.OverworldId, EmployerDoorTile),
            Tags = [],
            Type = LocationType.Shop,
            FacingDirection = FacingDirection.South,
        });

        // The manager's desk, inside the employer's own map — where interviews happen.
        LocationRegistry.Register(new Location
        {
            Name = "ManagerVisitor",
            Map = EmployerName,
            EntityID = Guid.NewGuid(),
            Position = new WorldPosition(EmployerName, ManagerDeskTile),
            Tags = [],
            Type = LocationType.Office,
            FacingDirection = FacingDirection.South,
        });

        EmployerRegistry.AddEmployer(EmployerName);
        EmployerRegistry.AddJob(EmployerName, "Cashier_1", 12m, [], new() { [Skill.Charisma] = 2f });

        _jamie = _world.CreateEntity();
        _jamie.Attach(new NameComponent("Jamie", "Testerson"));
        _jamie.Attach(new WorldPositionComponent { Position = new WorldPosition(MapRegistry.OverworldId, HomeTile) });
        _jamie.Attach(new CitizenComponent());
        _jamie.Attach(new HomeComponent("Home"){ Cost = new(){ Amount = 500, DayOfMonth = 2}});
        _jamie.Attach(new ActivityTypeComponent { Type = ActivityType.Idle });
        _jamie.Attach(new NeedsComponent { Satiety = 1f, Energy = 1f, Social = 1f });
        _jamie.Attach(new FactComponent());
        _jamie.Attach(new MemoryComponent());
        // Charisma 10 against a Charisma-2 requirement clears the interview at any roll of the
        // dice, so the test isn't flaky on the hiring decision itself — only the mechanical
        // apply/travel/interview/hire pipeline is under test here.
        _jamie.Attach(new SkillsComponent().WithSkill(Skill.Charisma, 10f));

        _story.Add("Jamie is out of work and living at Home.");
    }

    [Test]
    public void JamieAppliesInterviewsAndGetsHired()
    {
        var job = new JobSystem(_world);
        var pathfinding = new PathfindingSystem(_world);
        var state = new StateSystem(_world);
        var memory = new MemorySystem(_world);

        state.Initialize();

        var startedSeeking = false;
        var applied = false;
        var headedToInterview = false;
        var wasInterviewed = false;
        var wasHired = false;

        const int minutesToSimulate = 90;

        for (var minute = 0; minute < minutesToSimulate; minute++)
        {
            job.Update(1.0);

            if (!startedSeeking && _jamie.Has<JobSeekingComponent>())
            {
                startedSeeking = true;
                _story.Add("Jamie starts looking for work.");
            }

            if (!applied && _jamie.Has<JobApplicantComponent>())
            {
                applied = true;
                _story.Add($"Jamie applies for the Cashier job at {EmployerName}.");
            }

            pathfinding.Update(1.0);

            if (!headedToInterview && _jamie.TryGet<PathfindingComponent>(out var toInterview) && toInterview!.Destination.MapID == EmployerName)
            {
                headedToInterview = true;
                _story.Add("Jamie sets off across town for the interview.");
            }

            if (!wasInterviewed && _jamie.Get<ActivityTypeComponent>().Type == ActivityType.Interview)
            {
                wasInterviewed = true;
                _story.Add("Jamie sits down for the interview.");
            }

            state.Update(1.0);
            memory.Update(1.0);

            if (!wasHired && _jamie.Has<JobComponent>())
            {
                wasHired = true;
                _story.Add("Jamie gets the job!");
            }

            SimWorld.Instance.DateTime = SimWorld.Instance.DateTime.AddMinutes(1);

            Thread.Sleep(2); // give the background pathfinding Task.Run a chance to complete

            if (wasHired) break;
        }

        GD.Print("-- JobLoopTest --");
        foreach (var line in _story)
            GD.Print("- " + line);

        Assert.True(startedSeeking, "Jamie never picked up a JobSeekingComponent.");
        Assert.True(applied, "Jamie never applied for the Cashier vacancy.");
        Assert.True(headedToInterview, "Jamie never pathed toward the interview.");
        Assert.True(wasInterviewed, "Jamie's ActivityType never flipped to Interview.");
        Assert.True(wasHired, "Jamie never ended up with a JobComponent.");

        var jobComp = _jamie.Get<JobComponent>();
        Assert.Equal(EmployerName, jobComp.Employer);
        Assert.Equal("Cashier_1", jobComp.Title);

        Assert.True(
            _jamie.Get<MemoryComponent>().Memories.Any(x => x is ConfidenceMemory cm && cm.Type == ActivityType.Interview),
            "No ConfidenceMemory was recorded for the interview."
        );
    }
}
