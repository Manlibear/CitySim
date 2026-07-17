using System.Collections.Generic;
using System.Linq;
using Godot;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Presenters;
using CitySim.Presenters.Person;
using CitySim.Registries;
using CitySim.Systems;
using System;
using CitySim.Data.Facts;

namespace CitySim.Scripts;

public partial class SimWorld : Node
{
    // internal (not private) so tests can construct a bare SimWorld and assign Instance directly,
    // without going through _Ready()/Bootstrap() (which expects a full game scene tree).
    public static SimWorld Instance { get; internal set; } = null!;

    [Export] public float TimeSpeed { get; set; } = 1f;
    [Export] public float TimeMultiplier { get; set; } = 60;
    [Export] public int Seed {get;set;} = 42;
    public DateTime DateTime { get; set; } = new DateTime(2000, 01, 01, 9, 0, 0);
    public decimal InterestRate { get; internal set; } = .0375m;
    public World World { get; private set; } = null!;

    public override void _Ready()
    {
        World = new(Seed);
        Instance = this;
        CallDeferred(MethodName.Bootstrap);
    }

    public override void _Process(double delta)
    {
        World.Update(delta * TimeSpeed);
        DateTime = DateTime.AddSeconds(delta * TimeMultiplier * TimeSpeed);
    }

    // Systems receive delta already scaled by TimeSpeed (see World.Update above), and DateTime
    // advances by delta * TimeMultiplier in-game seconds per that same delta — so converting an
    // in-game duration into "delta units" only needs to divide out TimeMultiplier, not TimeSpeed.
    public float SecondsFromMinutes(float minutes) => minutes * 60f / TimeMultiplier;

    private void Bootstrap()
    {
        var overworldLayers = GetTree().CurrentScene
            .FindChild("Map")
            .GetChildren()
            .OfType<TileMapLayer>()
            .ToArray();
        if (overworldLayers.Length > 0)
            MapRegistry.Register(MapRegistry.OverworldId, overworldLayers);

        var mapChunks = GetTree().CurrentScene.FindChild("MapChunks").GetChildren().OfType<MapChunk>();
        foreach (var chunk in mapChunks)
            chunk.Bootstrap();

        foreach (var node in GetTree().GetNodesInGroup("ecs_entity"))
        {
            if (node is not PresenterNode presenter) continue;
            SpawnEntity(presenter);
            presenter.PreBootstrap();
        }

        foreach (var node in GetTree().GetNodesInGroup("ecs_entity"))
        {
            if (node is not PresenterNode presenter) continue;
            presenter.Bootstrap();
        }

        foreach (var node in GetTree().GetNodesInGroup("ecs_entity"))
        {
            if (node is not PresenterNode presenter) continue;
            presenter.PostBootstrap();
        }


        WalletRegistry.Initialize();
        InventoryRegistry.Initialize();
        OccupancyRegistry.Initialize();
        QueueRegistry.Initialize(World);

        World.Register(new PathfindingSystem(World));
        World.Register(new MoveToSystem(World));
        World.Register(new ScheduleSystem(World));
        World.Register(new NeedsSystem(World));
        World.Register(new StateSystem(World));
        World.Register(new InventorySystem(World));
        World.Register(new MemorySystem(World));
        World.Register(new ConsumptionSystem(World));
        World.Register(new ShopSystem(World));
        World.Register(new WalletSystem(World));
        World.Register(new SleepSystem(World));
        World.Register(new DelayedEffectSystem(World));
        World.Register(new JobSystem(World));
        World.Initialize();
    }

    public Entity SpawnEntity(PresenterNode presenter) => SpawnEntity(presenter, Guid.NewGuid());

    public Entity SpawnEntity(PresenterNode presenter, Guid id)
    {
        var entity = World.CreateEntity(id);
        presenter.AssignEntity(entity);
        return entity;
    }

    public void SaveGame()
    {
        var citizens = new List<CitizenSaveData>();

        foreach (var entity in World.Entities.With<NeedsComponent>().With<WorldPositionComponent>().With<ScheduleComponent>())
        {
            PathfindingComponent? pathfinding = null;
            if (entity.TryGet<PathfindingComponent>(out var pf))
            {
                var path = new Queue<WorldPosition>(pf!.Path ?? []);
                if (entity.TryGet<MoveToComponent>(out var moveTo))
                    path = new Queue<WorldPosition>([moveTo!.Target, .. path]);

                pathfinding = new PathfindingComponent
                {
                    Destination = pf.Destination,
                    Status = pf.Status == PathfindingStatus.Resolving ? PathfindingStatus.Pending : pf.Status,
                    Path = path,
                    OnArriveEffects = pf.OnArriveEffects,
                };
            }

            var citizenSaveData = new CitizenSaveData
            {
                Id = entity.Id,
                Name = entity.Get<NameComponent>(),
                Position = entity.Get<WorldPositionComponent>().Position,
                HomeMap = entity.TryGet<HomeComponent>(out var home) ? home!.MapID : null,
                Needs = entity.Get<NeedsComponent>(),
                ActivityType = entity.Get<ActivityTypeComponent>(),
                Schedule = [.. entity.Get<ScheduleComponent>().Entries],
                Fact = [.. entity.Get<FactComponent>().Facts],
                Pathfinding = pathfinding,
                Wallet = WalletRegistry.Get(entity.Id),
                MemoryComponent = entity.Get<MemoryComponent>(),
                PreferenceComponent = entity.Get<PreferenceComponent>(),
                SkillsComponent = entity.Get<SkillsComponent>(),
                CitizenComponent = entity.Get<CitizenComponent>(),
                MoodComponent = entity.Get<MoodComponent>()
            };

            if (entity.TryGet<JobComponent>(out var jobComponent))
                citizenSaveData.Job = jobComponent;

            if (entity.TryGet<HungerComponent>(out var hungerComponent))
                citizenSaveData.HungerComponent = hungerComponent;

            if (entity.TryGet<TiredComponent>(out var tiredComponent))
                citizenSaveData.TiredComponent = tiredComponent;

            if (entity.TryGet<BrowseShopComponent>(out var browseShopComponent))
                citizenSaveData.BrowseShopComponent = browseShopComponent;

            if (entity.TryGet<DelayedEffectComponent>(out var delayedEffectComponent))
                citizenSaveData.DelayedEffectComponent = delayedEffectComponent;

            if (entity.TryGet<SleepComponent>(out var sleepComponent))
                citizenSaveData.SleepComponent = sleepComponent;

            citizens.Add(citizenSaveData);
        }

        Scripts.SaveGame.Save(new SaveGameData
        {
            WorldTime = DateTime,
            Citizens = citizens,
            Registries = new()
            {
                Wallets = WalletRegistry.Get(),
                Inventories = InventoryRegistry.Get(),
                OccupiedLocations = OccupancyRegistry.Get(),
                Queues = QueueRegistry.Get()
            }

        }, Scripts.SaveGame.DefaultSavePath);
    }

    public void LoadGame()
    {
        var data = Scripts.SaveGame.Load(Scripts.SaveGame.DefaultSavePath);

        foreach (var entity in World.Entities.With<NeedsComponent>().With<WorldPositionComponent>().With<ScheduleComponent>().ToArray())
        {
            if (entity.TryGet<GodotNodeComponent>(out var nodeComp) && nodeComp!.Node is PersonPresenter existingPerson)
                existingPerson.QueueFree();
            entity.Destroy();
        }

        DateTime = data.WorldTime;

        WalletRegistry.Initialize();
        foreach (var (id, wallet) in data.Registries.Wallets)
            WalletRegistry.Register(id, wallet);

        InventoryRegistry.Initialize();
        foreach (var (id, inventory) in data.Registries.Inventories)
            InventoryRegistry.Register(id, inventory);

        OccupancyRegistry.Initialize();
        OccupancyRegistry.Restore(data.Registries.OccupiedLocations);

        QueueRegistry.Initialize(World);
        QueueRegistry.Restore(data.Registries.Queues);

        var personScene = GD.Load<PackedScene>("res://Scenes/Prefabs/Characters/person.tscn");
        var overworld = GetParent();

        foreach (var citizenData in data.Citizens)
        {
            var presenter = personScene.Instantiate<PersonPresenter>();
            presenter.HomeMap = citizenData.HomeMap;
            presenter.FirstName = citizenData.Name.FirstName;
            presenter.Surname = citizenData.Name.Surname;
            presenter.GlobalPosition = citizenData.Position.ToGlobalPosition();

            overworld.AddChild(presenter);

            var entity = SpawnEntity(presenter, citizenData.Id);
            presenter.PreBootstrap();
            presenter.Bootstrap();
            WalletRegistry.Register(entity.Id, citizenData.Wallet);

            entity.Attach(citizenData.Name);
            entity.Attach(citizenData.Needs);
            entity.Attach(citizenData.ActivityType);
            entity.Attach(citizenData.MemoryComponent);
            entity.Attach(citizenData.PreferenceComponent);
            entity.Attach(citizenData.SkillsComponent);
            entity.Attach(citizenData.CitizenComponent);
            entity.Attach(citizenData.MoodComponent);
            entity.Attach(new WorldPositionComponent { Position = citizenData.Position });

            if (citizenData.Job != null)
                entity.Attach(citizenData.Job);

            if (citizenData.BrowseShopComponent != null)
                entity.Attach(citizenData.BrowseShopComponent);

            if (citizenData.HungerComponent != null)
                entity.Attach(citizenData.HungerComponent);

            if (citizenData.TiredComponent != null)
                entity.Attach(citizenData.TiredComponent);

            if (citizenData.DelayedEffectComponent != null)
                entity.Attach(citizenData.DelayedEffectComponent);

            if (citizenData.SleepComponent != null)
                entity.Attach(citizenData.SleepComponent);

            entity.Attach(new FactComponent() { Facts = new Queue<IFact>(citizenData.Fact) });


            var schedule = entity.Get<ScheduleComponent>();
            foreach (var entry in citizenData.Schedule)
                schedule.AddEntry(entry);

            if (citizenData.Pathfinding != null)
                entity.Attach(citizenData.Pathfinding);
        }

        World.Initialize();
    }
}
