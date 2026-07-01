using Godot;

namespace CitySim.Scripts;

public partial class DayNightCycle : CanvasModulate
{
    public static DayNightCycle? Instance { get; private set; }

    [Export] public Gradient AmbientGradient = null!;
    [Export] public Gradient WindowColourGradient = null!;
    [Export] public Curve DayBlendCurve = null!;

    public float DayBlend { get; private set; } = 1f;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        var time = SimWorld.Instance.DateTime;
        var normalizedHour = (float)(time.Hour + time.Minute / 60.0 + time.Second / 3600.0) / 24f;

        Color = AmbientGradient.Sample(normalizedHour);
        DayBlend = DayBlendCurve.Sample(normalizedHour);
    }

    public Color CurrentWindowColour
    {
        get
        {
            var time = SimWorld.Instance.DateTime;
            var normalizedHour = (float)(time.Hour + time.Minute / 60.0 + time.Second / 3600.0) / 24f;

            return WindowColourGradient.Sample(normalizedHour);
        }

    }
}
