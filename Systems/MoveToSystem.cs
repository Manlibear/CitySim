using Godot;
using CitySim.ECS;
using CitySim.Components;
using CitySim.Data;
using CitySim.Presenters.Person;
using CitySim.Registries;

namespace CitySim.Systems;

public class MoveToSystem(World world) : IUpdateSystem
{
    private readonly World _world = world;

    public void Update(double delta)
    {
        foreach (var entity in _world.Entities.With<MoveToComponent>().With<GodotNodeComponent>())
        {
            var moveTo = entity.Get<MoveToComponent>();
            var node = entity.Get<GodotNodeComponent>().Node;
            var toTarget = moveTo.WorldPos - node.GlobalPosition;

            // Set facing and start walk animation once per step.
            if (!moveTo.HasStarted)
            {
                moveTo.HasStarted = true;
                if (node is PersonPresenter person)
                {
                    person.Facing = FacingFrom(toTarget);
                    person.PlayAnimation("walk");
                }
            }

            var speed = node is PersonPresenter p ? p.MoveSpeed : 64f;
            var stepDist = speed * (float)delta;

            if (toTarget.Length() <= stepDist)
            {
                node.GlobalPosition = moveTo.WorldPos;

                if (entity.TryGet<WorldPositionComponent>(out var posComp))
                {
                    posComp!.Position = moveTo.Target;

                    var wt = MapRegistry.IsOnWarpTile(posComp.Position.MapID, posComp.Position.Tile);

                    if (wt != null)
                    {
                        var pairedWarp = MapRegistry.GetPairedWarpTile(posComp.Position.MapID, wt.Value.MapID);
                        var map = MapRegistry.GetMapInstance(wt.Value.MapID);
                        posComp.Position = new WorldPosition(wt.Value.MapID, pairedWarp!.Value.Tile);
                        node.Reparent(map);
                        node.GlobalPosition = posComp.Position.ToGlobalPosition();
                    }
                }

                entity.Detach<MoveToComponent>();


            }
            else
            {
                node.GlobalPosition += toTarget.Normalized() * stepDist;
            }
        }
    }

    private static FacingDirection FacingFrom(Vector2 movement)
    {
        if (Mathf.Abs(movement.X) >= Mathf.Abs(movement.Y))
            return movement.X >= 0 ? FacingDirection.East : FacingDirection.West;
        return movement.Y >= 0 ? FacingDirection.South : FacingDirection.North;
    }
}
