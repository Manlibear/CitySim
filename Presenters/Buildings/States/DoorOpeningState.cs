using Godot;
using CitySim.Presenters;
using CitySim.Presenters.Buildings;

namespace CitySim.Presenters.Buildings.States;

public sealed class DoorOpeningState : IPresenterState
{
    private bool _started;

    public void Enter(PresenterNode presenter)
    {
        _started = false;
        if (presenter is BuildingPresenter building &&
            building.DoorAnimation?.HasAnimation("door_open") == true)
            building.DoorAnimation.Play("door_open");
        _started = true;
    }

    public IPresenterState Poll(PresenterNode presenter, double delta)
    {
        if (!_started) return this;

        if (presenter is BuildingPresenter building)
        {
            var anim = building.DoorAnimation;
            if (anim == null || !anim.IsPlaying())
                return IdleState.Instance;
        }

        return this;
    }

    public void Exit(PresenterNode presenter)
    {
        if (presenter is BuildingPresenter building)
        {
            building.DoorAnimation?.Stop();
            building.OpenInterior();
        }
    }
}
