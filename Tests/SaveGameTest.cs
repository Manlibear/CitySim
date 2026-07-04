using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Chickensoft.GoDotTest;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.Scripts;
using Xunit;

namespace CitySim.Tests;

public class SaveGameTest : TestClass
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), "citysim_savegame_test.json");
    private SaveGameData _original = default!;
    private SaveGameData _loaded = default!;
    private CitizenSaveData _originalCitizen = default!;
    private CitizenSaveData _loadedCitizen = default!;

    public SaveGameTest(Node testScene) : base(testScene) { }

    [Setup]
    public void Setup()
    {
        _original = new SaveGameData
        {
            WorldTime = new DateTime(2026, 3, 14, 9, 30, 0),
            Citizens =
            [
                new CitizenSaveData
                {
                    Name = new NameComponent("Alice", "Testerson"),
                    Position = new WorldPosition("Overworld", new Vector2I(12, -7)),
                    HomeMap = "SmallHouse1",
                    Needs = new NeedsComponent { Satiety = 0.42f, Energy = 0.81f, Social = 0.15f },
                    ActivityType = new ActivityTypeComponent { Type = ActivityType.Sleep, Priority = 1 },
                    Schedule =
                    [
                        new ScheduleEntry
                        {
                            Day = DayOfWeek.Saturday,
                            Time = new TimeOnly(9, 45),
                            LocationPath = "/Overworld/SmallHouse2",
                            OnArriveEffects = [new ActivityTypeEffect(ActivityType.Liesure), new FacingDirectionEffect(FacingDirection.South)],
                        },
                    ],
                    Pathfinding = new PathfindingComponent
                    {
                        Destination = new WorldPosition("Overworld", new Vector2I(20, 20)),
                        Status = PathfindingStatus.Moving,
                        Path = new Queue<WorldPosition>([
                            new WorldPosition("Overworld", new Vector2I(13, -7)),
                            new WorldPosition("Overworld", new Vector2I(14, -7)),
                        ]),
                        OnArriveEffects = [new PlayAnimationEffect("idle")],
                    },
                },
            ],
        };

        SaveGame.Save(_original, _path);
        _loaded = SaveGame.Load(_path);

        _originalCitizen = _original.Citizens[0];
        _loadedCitizen = _loaded.Citizens[0];
    }

    [Cleanup]
    public void Cleanup()
    {
        if (File.Exists(_path)) File.Delete(_path);
    }

    [Test]
    public void WorldTimeRoundTrips() => Assert.Equal(_original.WorldTime, _loaded.WorldTime);

    [Test]
    public void PositionRoundTrips() => Assert.Equal(_originalCitizen.Position, _loadedCitizen.Position);

    [Test]
    public void HomeMapRoundTrips() => Assert.Equal(_originalCitizen.HomeMap, _loadedCitizen.HomeMap);

    [Test]
    public void NameRoundTrips()
    {
        Assert.Equal(_originalCitizen.Name.FirstName, _loadedCitizen.Name.FirstName);
        Assert.Equal(_originalCitizen.Name.Surname, _loadedCitizen.Name.Surname);
    }

    [Test]
    public void NeedsRoundTrip()
    {
        Assert.Equal(_originalCitizen.Needs.Satiety, _loadedCitizen.Needs.Satiety);
        Assert.Equal(_originalCitizen.Needs.Energy, _loadedCitizen.Needs.Energy);
        Assert.Equal(_originalCitizen.Needs.Social, _loadedCitizen.Needs.Social);
    }

    [Test]
    public void ActivityTypeRoundTrips()
    {
        Assert.Equal(_originalCitizen.ActivityType.Type, _loadedCitizen.ActivityType.Type);
        Assert.Equal(_originalCitizen.ActivityType.Priority, _loadedCitizen.ActivityType.Priority);
    }

    [Test]
    public void ScheduleCacheFieldsAreNotPersisted()
    {
        var entry = _loadedCitizen.Schedule[0];
        Assert.Null(entry.Position);
        Assert.Null(entry.CachedPath);
        Assert.Null(entry.DispatchTime);
    }

    [Test]
    public void SchedulePolymorphicEffectsRoundTrip()
    {
        var effects = _loadedCitizen.Schedule[0].OnArriveEffects;
        Assert.NotNull(effects);
        Assert.Equal(2, effects!.Length);
        Assert.IsType<ActivityTypeEffect>(effects[0]);
        Assert.IsType<FacingDirectionEffect>(effects[1]);
    }

    [Test]
    public void PathfindingRoundTrips()
    {
        Assert.NotNull(_loadedCitizen.Pathfinding);
        Assert.Equal(PathfindingStatus.Moving, _loadedCitizen.Pathfinding!.Status);
        Assert.Equal(2, _loadedCitizen.Pathfinding.Path?.Count);
        Assert.IsType<PlayAnimationEffect>(_loadedCitizen.Pathfinding.OnArriveEffects?[0]);
    }

    [Test]
    public void PathfindingPendingTaskIsNeverPersisted() =>
        Assert.Null(_loadedCitizen.Pathfinding?.PendingTask);

    // Note: SimWorld.SaveGame() normalizes Status.Resolving -> Pending before it ever
    // reaches SaveGame.Save() (a live PendingTask can't serialize, so it's rewritten
    // upstream). That normalization isn't covered here — it needs a running SimWorld/
    // scene to exercise. This test only guards that the serializer itself round-trips
    // every PathfindingStatus value without choking, Resolving included.
    [Test]
    public void AllPathfindingStatusValuesSerializeWithoutError()
    {
        foreach (var status in Enum.GetValues<PathfindingStatus>())
        {
            var citizen = new CitizenSaveData
            {
                Name = new NameComponent("Bob", "Testerson"),
                Position = new WorldPosition("Overworld", Vector2I.Zero),
                Needs = new NeedsComponent(),
                ActivityType = new ActivityTypeComponent(),
                Schedule = [],
                Pathfinding = new PathfindingComponent
                {
                    Destination = new WorldPosition("Overworld", new Vector2I(5, 5)),
                    Status = status,
                },
            };

            var data = new SaveGameData { WorldTime = DateTime.Now, Citizens = [citizen] };
            SaveGame.Save(data, _path);
            var reloaded = SaveGame.Load(_path);

            Assert.Equal(status, reloaded.Citizens[0].Pathfinding!.Status);
        }
    }
}
