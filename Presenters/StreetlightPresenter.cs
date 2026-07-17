using CitySim.Registries;
using CitySim.Scripts;
using Godot;

namespace CitySim.Presenters;

public partial class StreetlightPresenter : PresenterNode
{
    [Export] public PointLight2D Light = null!;
    [Export] public float MaxEnergy = 1.2f;

    public override void _Process(double delta)
    {
        var dayBlend = DayNightCycle.Instance?.DayBlend ?? 1f;
        Light.Energy = Mathf.Lerp(MaxEnergy, 0f, dayBlend);
    }

    public override void PreBootstrap() { }

    public override void Bootstrap()
    {
        var mapID = GetTree().CurrentScene.Name;
        var mapLayer = MapRegistry.GetLayer(mapID);
        MapRegistry.MarkBlocked(mapID, [mapLayer!.LocalToMap(Position)]);
    }

    public override void PostBootstrap() { }
}
