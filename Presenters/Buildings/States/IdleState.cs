using CitySim.Presenters;

namespace CitySim.Presenters.Buildings.States;

public sealed class IdleState : IPresenterState
{
    public static readonly IdleState Instance = new();

    public IPresenterState Poll(PresenterNode presenter, double delta) => this;
}
