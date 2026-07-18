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
using CitySim.Registries;
using CitySim.ECS;

namespace CitySim.Tests;

public class SaveGameTest(Node testScene) : TestClass(testScene)
{
    private Guid entityId { get; set; } = Guid.NewGuid();
    private readonly string _path = Path.Combine(Path.GetTempPath(), "citysim_savegame_test.json");
    private SaveGameData _original = default!;
    private SaveGameData _loaded = default!;
    private CitizenSaveData _originalCitizen = default!;
    private CitizenSaveData _loadedCitizen = default!;

    [Setup]
    public void Setup()
    {
        var world = new World(42);
        WalletRegistry.Initialize();
        InventoryRegistry.Initialize();
        OccupancyRegistry.Initialize();
        QueueRegistry.Initialize(world);

        WalletRegistry.Register(entityId, new Wallet() { Balance = 50 });

        SimWorld.Instance = new SimWorld
        {
            TimeSpeed = 1f,
            TimeMultiplier = 60f,
            DateTime = new DateTime(2026, 3, 14, 9, 0, 0),
        };

        var otherPersonId = Guid.NewGuid();
        var journal = new JournalComponent();
        journal.AddEntry("Had a chat on the corner about the weather.", otherPersonId);
        SimWorld.Instance.DateTime = SimWorld.Instance.DateTime.AddMinutes(30);
        journal.AddEntry("Got the job at Kwik-E-Mart!", null);

        _original = new SaveGameData
        {
            WorldTime = new DateTime(2026, 3, 14, 9, 30, 0),
            Citizens =
            [
                new CitizenSaveData
                {
                    Id = entityId,
                    Name = new NameComponent("Alice", "Testerson"),
                    Position = new WorldPosition("Overworld", new Vector2I(12, -7)),
                    HomeMap = "SmallHouse1",
                    Fact =  [],
                    Needs = new NeedsComponent { Satiety = 0.42f, Energy = 0.81f, Social = 0.15f },
                    ActivityType = new ActivityTypeComponent { Type = ActivityType.Sleep, Priority = ActivityPriority.Idle },
                    Wallet = WalletRegistry.Get(entityId),
                    PreferenceComponent = new PreferenceComponent(),
                    MemoryComponent = new MemoryComponent(),
                    JournalComponent = journal,
                    MoodComponent =  new MoodComponent
                    {
                        Mood = .62f,
                        LastSampleTime = new DateTime(2026, 3, 14, 9, 15, 0),
                        History =
                        [
                            new MoodSample { Timestamp = new DateTime(2026, 3, 14, 8, 45, 0), Mood = .55f },
                            new MoodSample { Timestamp = new DateTime(2026, 3, 14, 9, 0, 0), Mood = .58f },
                            new MoodSample { Timestamp = new DateTime(2026, 3, 14, 9, 15, 0), Mood = .62f },
                        ],
                    },
                    SkillsComponent = new SkillsComponent().WithSkill(Skill.Charisma, 5).WithSkill(Skill.Dexterity, 2),
                    CitizenComponent = new CitizenComponent(),
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
            Registries = new()
            {
                Inventories = InventoryRegistry.Get(),
                OccupiedLocations = OccupancyRegistry.Get(),
                Queues = QueueRegistry.Get(),
                Wallets = WalletRegistry.Get()
            }
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
    public void JournalRoundTrip()
    {
        Assert.Equal(_originalCitizen.JournalComponent.GetAllEntries(), _loadedCitizen.JournalComponent.GetAllEntries());
    }

    [Test]
    public void SkillsRoundTrip()
    {
        Assert.Equal(_originalCitizen.SkillsComponent.GetSkill(Skill.Charisma), _loadedCitizen.SkillsComponent.GetSkill(Skill.Charisma));
    }

    [Test]
    public void WalletRoundTrip()
    {
        Assert.Equal(_originalCitizen.Wallet.Balance, _loadedCitizen.Wallet.Balance);
    }

    [Test]
    public void MoodRoundTrip()
    {
        Assert.Equal(_originalCitizen.MoodComponent.Mood, _loadedCitizen.MoodComponent.Mood);
        Assert.Equal(_originalCitizen.MoodComponent.LastSampleTime, _loadedCitizen.MoodComponent.LastSampleTime);
        Assert.Equal(_originalCitizen.MoodComponent.History.Count, _loadedCitizen.MoodComponent.History.Count);
        for (var i = 0; i < _originalCitizen.MoodComponent.History.Count; i++)
        {
            Assert.Equal(_originalCitizen.MoodComponent.History[i].Timestamp, _loadedCitizen.MoodComponent.History[i].Timestamp);
            Assert.Equal(_originalCitizen.MoodComponent.History[i].Mood, _loadedCitizen.MoodComponent.History[i].Mood);
        }
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
                Id = Guid.Parse("cbd8c0d6-b4dd-4ec4-9855-aff9ca9191f8"),
                Name = new NameComponent("Bob", "Testerson"),
                Position = new WorldPosition("Overworld", Vector2I.Zero),
                Needs = new NeedsComponent(),
                ActivityType = new ActivityTypeComponent(),
                Schedule = [],
                Fact = [],
                MemoryComponent = new MemoryComponent(),
                PreferenceComponent = new PreferenceComponent(),
                Wallet = WalletRegistry.Get(entityId),
                CitizenComponent = new CitizenComponent(),
                MoodComponent = new MoodComponent(),
                JournalComponent = new JournalComponent(),
                SkillsComponent = new SkillsComponent().WithSkill(Skill.Charisma, 5).WithSkill(Skill.Dexterity, 2),
                Pathfinding = new PathfindingComponent
                {
                    Destination = new WorldPosition("Overworld", new Vector2I(5, 5)),
                    Status = status,
                },
            };

            var data = new SaveGameData
            {
                WorldTime = DateTime.Now,
                Citizens = [citizen],
                Registries = new()
                {
                    Inventories = InventoryRegistry.Get(),
                    OccupiedLocations = OccupancyRegistry.Get(),
                    Queues = QueueRegistry.Get(),
                    Wallets = WalletRegistry.Get()
                }
            };
            SaveGame.Save(data, _path);
            var reloaded = SaveGame.Load(_path);

            Assert.Equal(status, reloaded.Citizens[0].Pathfinding!.Status);
        }
    }
}
