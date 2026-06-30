namespace CitySim.ECS;

public interface IUpdateSystem
{
    void Initialize() {}
    void Update(double delta);
}
