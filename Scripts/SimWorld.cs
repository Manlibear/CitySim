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

namespace CitySim.Scripts;

public partial class SimWorld : Node
{
    public static SimWorld Instance { get; private set; } = null!;

    [Export] public float TimeSpeed { get; set; } = 1f;
    [Export] public float TimeMultiplier { get; set; } = 60;
    public DateTime DateTime { get; set; } = new DateTime(2000, 01, 01, 9, 0, 0);
    public World World { get; } = new();

    public override void _Ready()
    {
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

        QueueRegistry.Init(World);

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
        World.Initialize();
    }

    public Entity SpawnEntity(PresenterNode presenter)
    {
        var entity = World.CreateEntity();
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
                Name = entity.Get<NameComponent>(),
                Position = entity.Get<WorldPositionComponent>().Position,
                HomeMap = entity.TryGet<HomeComponent>(out var home) ? home!.MapID : null,
                Needs = entity.Get<NeedsComponent>(),
                ActivityType = entity.Get<ActivityTypeComponent>(),
                Schedule = [.. entity.Get<ScheduleComponent>().Entries],
                Fact = [.. entity.Get<FactComponent>().Facts],
                Pathfinding = pathfinding,
                Wallet = WalletRegistry.Get(entity.Id),
            };

            if (entity.TryGet<JobComponent>(out var jobComponent))
            {
                citizenSaveData.Job = jobComponent;
            }

            if (entity.TryGet<HungerComponent>(out var hungerComponent))
            {
                citizenSaveData.HungerComponent = hungerComponent;
            }

            if (entity.TryGet<TiredComponent>(out var tiredComponent))
            {
                citizenSaveData.TiredComponent = tiredComponent;
            }

            if (entity.TryGet<BrowseShopComponent>(out var browseShopComponent))
            {
                citizenSaveData.BrowseShopComponent = browseShopComponent;
            }

            citizens.Add(citizenSaveData);
        }

        Scripts.SaveGame.Save(new SaveGameData { WorldTime = DateTime, Citizens = citizens }, CitySim.Scripts.SaveGame.DefaultSavePath);
    }

    public void LoadGame()
    {
        var data = Scripts.SaveGame.Load(CitySim.Scripts.SaveGame.DefaultSavePath);

        foreach (var entity in World.Entities.With<NeedsComponent>().With<WorldPositionComponent>().With<ScheduleComponent>().ToArray())
        {
            if (entity.TryGet<GodotNodeComponent>(out var nodeComp) && nodeComp!.Node is PersonPresenter existingPerson)
                existingPerson.QueueFree();
            entity.Destroy();
        }

        DateTime = data.WorldTime;

        var personScene = GD.Load<PackedScene>("res://Scenes/Prefabs/Characters/person.tscn");
        var overworld = GetParent();

        foreach (var citizenData in data.Citizens)
        {
            var presenter = personScene.Instantiate<PersonPresenter>();
            presenter.HomeMap = citizenData.HomeMap;
            presenter.FirstName = citizenData.Name.FirstName;
            presenter.Surname = citizenData.Name.Surname;
            presenter.GlobalPosition = citizenData.Position.ToGlobalPosition();
            WalletRegistry.Register(presenter.Entity.Id, citizenData.Wallet);

            overworld.AddChild(presenter);

            var entity = SpawnEntity(presenter);

            entity.Attach(citizenData.Name);
            entity.Attach(citizenData.Needs);
            entity.Attach(citizenData.ActivityType);
            entity.Attach(new WorldPositionComponent { Position = citizenData.Position });

            if (citizenData.Job != null)
                entity.Attach(citizenData.Job);

            if (citizenData.BrowseShopComponent != null)
                entity.Attach(citizenData.BrowseShopComponent);

            if (citizenData.HungerComponent != null)
                entity.Attach(citizenData.HungerComponent);

            if (citizenData.TiredComponent != null)
                entity.Attach(citizenData.TiredComponent);

            var schedule = entity.Get<ScheduleComponent>();
            foreach (var entry in citizenData.Schedule)
                schedule.AddEntry(entry);

            if (citizenData.Pathfinding != null)
                entity.Attach(citizenData.Pathfinding);
        }
    }
}
