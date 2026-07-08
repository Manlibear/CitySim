using System;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.ECS;
using CitySim.Helpers;
using CitySim.Scripts;
using Godot;

namespace CitySim.Systems;

public class NeedsSystem(World world) : IUpdateSystem
{
    private World _world = world;

    // delta here already has TimeSpeed baked in (SimWorld._Process calls World.Update(delta * TimeSpeed)) —
    // do not multiply anything below by SimWorld.Instance.TimeSpeed again, or decay/consumption scales by TimeSpeed^2.
    public void Update(double delta)
    {
        foreach (var entity in _world.Entities.With<NeedsComponent>().With<ScheduleComponent>().With<HomeComponent>())
        {
            var needsComp = entity.Get<NeedsComponent>();
            var scheduleComp = entity.Get<ScheduleComponent>();

            foreach (var nd in needsComp.NeedsDeltas)
            {
                var step = Mathf.Min((float)delta, Mathf.Max(nd.Duration, 0f));
                var fraction = nd.Duration > 0f ? step / nd.Duration : 1f;

                if (nd.SatietyDelta.HasValue)
                {
                    var applied = nd.SatietyDelta.Value * fraction;
                    needsComp.Satiety += applied;
                    nd.SatietyDelta -= applied;
                }

                if (nd.EnergyDelta.HasValue)
                {
                    var applied = nd.EnergyDelta.Value * fraction;
                    needsComp.Energy += applied;
                    nd.EnergyDelta -= applied;
                }

                if (nd.SocialDelta.HasValue)
                {
                    var applied = nd.SocialDelta.Value * fraction;
                    needsComp.Social += applied;
                    nd.SocialDelta -= applied;
                }

                nd.Duration -= step;
            }

            needsComp.NeedsDeltas.RemoveAll(nd => nd.Duration <= 0f);

            if (entity.Has<SleepComponent>())
            {
                needsComp.Energy += Globals.EnergyRecoverySleepRate * (float)delta;
                needsComp.Satiety -= Globals.SleepMetabolismFactor * Globals.SatietyDecayRate * (float)delta;
            }
            else
            {
                needsComp.Energy -= Globals.EnergyDecayRate * (float)delta;
                needsComp.Satiety -= Globals.SatietyDecayRate * (float)delta;
            }

            needsComp.Social -= Globals.SocialDecayRate * (float)delta;

            needsComp.Energy = Mathf.Clamp(needsComp.Energy, 0, 1);
            needsComp.Satiety = Mathf.Clamp(needsComp.Satiety, 0, 1);
            needsComp.Social = Mathf.Clamp(needsComp.Social, 0, 1);

            if (needsComp.Energy < Globals.MinEnergyNeed && !entity.Has<TiredComponent>())
            {
                entity.InterruptPathfinding();
                entity.Attach(new TiredComponent());
            }

            if (needsComp.Satiety < Globals.MinSatietyNeed && !entity.Has<HungerComponent>())
            {
                entity.InterruptPathfinding();
                entity.Attach(new HungerComponent("snack"));
            }
        }
    }
}
