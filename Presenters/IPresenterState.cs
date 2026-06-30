namespace CitySim.Presenters;

public interface IPresenterState
{
    void Enter(PresenterNode presenter) {}
    IPresenterState Poll(PresenterNode presenter, double delta);
    void Exit(PresenterNode presenter) {}
}
