using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.Facts;
using CitySim.ECS;
using CitySim.Helpers;
using CitySim.Presenters.Person;
using Godot;

namespace CitySim.Systems;

public class SocialSystem(World world) : IUpdateSystem
{
    private readonly RandomNumberGenerator rng = new()
    {
        Seed = (ulong)world.Seed
    };

    public void Update(double delta)
    {
        var adjacencyMap = new Dictionary<string, List<(Guid Entity, Vector2I Position)>>();
        foreach (var entity in world.Entities.With<CitizenComponent>().Without<SocialInteractionComponent>().With<WorldPositionComponent>())
        {
            var worldPos = entity.Get<WorldPositionComponent>().Position;

            if (!adjacencyMap.ContainsKey(worldPos.MapID))
                adjacencyMap.Add(worldPos.MapID, []);

            adjacencyMap[worldPos.MapID].Add((entity.Id, worldPos.Tile));
        }

        foreach (var (map, citizenPositions) in adjacencyMap)
        {
            foreach (var citizenPos in citizenPositions)
            {
                var citizen = world.FindEntityByID(citizenPos.Entity)!.Value;
                if (citizen.Has<SocialInteractionComponent>()) continue; // already been grabbed by someone else

                var nearby = citizenPositions.Where(x => x.Entity != citizenPos.Entity && x.Position.ManhattanDistanceFrom(citizenPos.Position) <= Globals.InteractionTileRange).ToList();

                if (nearby.Any())
                {
                    var target = nearby[rng.RandiRange(0, nearby.Count() - 1)];

                    citizen.Attach(new SocialInteractionComponent() { TargetEntityID = target.Entity });
                    world.FindEntityByID(target.Entity)!.Value.Attach(new SocialInteractionComponent() { TargetEntityID = citizen.Id });
                }
            }
        }

        foreach (var entity in world.Entities.With<SocialInteractionComponent>().ToList())
        {
            // TryGet since we'll remove in a pair if outside of range
            if (entity.TryGet<SocialInteractionComponent>(out var socialComp))
            {
                var target = world.FindEntityByID(socialComp!.TargetEntityID)!.Value;
                var targetSocial = target.Get<SocialInteractionComponent>();
                var worldPos = entity.Get<WorldPositionComponent>().Position;
                var targetWorldPos = target.Get<WorldPositionComponent>().Position;

                if (worldPos.MapID != targetWorldPos.MapID || worldPos.Tile.ManhattanDistanceFrom(targetWorldPos.Tile) > Globals.InteractionTileRange)
                {
                    entity.Get<FactComponent>().Add(new SocialInteractionFact()
                    {
                        OtherPersonID = target.Id,
                        Duration = socialComp.Duration,
                        Positive = socialComp.Positive
                    });

                    target.Get<FactComponent>().Add(new SocialInteractionFact()
                    {
                        OtherPersonID = entity.Id,
                        Duration = socialComp.Duration,
                        Positive = socialComp.Positive
                    });

                    entity.Detach<SocialInteractionComponent>();
                    target.Detach<SocialInteractionComponent>();
                    continue;
                }

                if (socialComp.Status == SocialInteractionStatus.Pending)
                {
                    if (!entity.Has<PathfindingComponent>() && !target.Has<PathfindingComponent>())
                    {
                        // turn to face each other
                        var deltaPos = targetWorldPos.Tile - worldPos.Tile;
                        var entityFacingDirection = Math.Abs(deltaPos.X) > Math.Abs(deltaPos.Y)
                            ? (deltaPos.X > 0 ? FacingDirection.East : FacingDirection.West)
                            : (deltaPos.Y > 0 ? FacingDirection.South : FacingDirection.North);
                        var targetFacingDirection = entityFacingDirection.Opposite();

                        entity.SetFacingDirection(entityFacingDirection);
                        target.SetFacingDirection(targetFacingDirection);
                    }

                    socialComp.Status = SocialInteractionStatus.InProgress;
                    target.Get<SocialInteractionComponent>().Status = SocialInteractionStatus.InProgress;
                }
                else if (socialComp.Status == SocialInteractionStatus.InProgress)
                {
                    var interestsComp = entity.Get<InterestsComponent>();
                    var targetInterests = target.Get<InterestsComponent>();
                    var entityRelationship = entity.Get<RelationshipComponent>();
                    var targetRelationship = target.Get<RelationshipComponent>();
                    var entityMemory = entity.Get<MemoryComponent>();
                    var targetMemory = target.Get<MemoryComponent>();

                    var socialScore = 0f;

                    var entityToTarget = entityRelationship.GetRelationship(target.Id);
                    if (entityToTarget != null)
                        socialScore += entityToTarget.Score + entityMemory.Memories.OfOtherPerson(target.Id).Sum(x => x.Satisfaction);

                    var targetToEntity = targetRelationship.GetRelationship(entity.Id);
                    if (targetToEntity != null)
                        socialScore += targetToEntity.Score + targetMemory.Memories.OfOtherPerson(target.Id).Sum(x => x.Satisfaction);

                    var sharedInterests = interestsComp.FindCommonInterests(targetInterests.GetInterests());
                    if (sharedInterests.Any())
                        socialScore += sharedInterests.First().Intensity;


                    var successChance = 1f / (1f + MathF.Exp(-socialScore / Globals.SocialScoreSensitivity));
                    socialComp.Positive = targetSocial.Positive = rng.Randf() <= successChance;
                    socialComp.Status = targetSocial.Status = SocialInteractionStatus.Ticking;

                }
                else
                {
                    var entityNeeds = entity.Get<NeedsComponent>();
                    var targetNeeds = entity.Get<NeedsComponent>();

                    var entityRelationship = entity.Get<RelationshipComponent>();
                    var targetRelationship = target.Get<RelationshipComponent>();

                    var socialTick = Globals.SocialRelationshipPerSecond * (float)delta * (socialComp.Positive ? 1 : -1);
                    entityRelationship.UpdateRelationship(target.Id, socialTick);
                    targetRelationship.UpdateRelationship(entity.Id, socialTick);

                    entityNeeds.Social += socialTick;
                    targetNeeds.Social += socialTick;

                    socialComp.Duration += delta;
                    targetSocial.Duration += delta;
                }
            }
        }
    }
}
