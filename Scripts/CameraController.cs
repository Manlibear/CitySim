using Godot;

namespace CitySim.Scripts;

public partial class CameraController : Camera2D
{
    [Export] public float ZoomSpeed = 0.1f;
    [Export] public float MinZoom = 0.25f;
    [Export] public float MaxZoom = 4.0f;

    private bool _panning;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            switch (mouseButton.ButtonIndex)
            {
                case MouseButton.WheelUp:
                    ApplyZoom(1 + ZoomSpeed);
                    GetViewport().SetInputAsHandled();
                    break;
                case MouseButton.WheelDown:
                    ApplyZoom(1 - ZoomSpeed);
                    GetViewport().SetInputAsHandled();
                    break;
                case MouseButton.Middle:
                    _panning = mouseButton.Pressed;
                    GetViewport().SetInputAsHandled();
                    break;
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _panning)
        {
            Position -= mouseMotion.Relative / Zoom;
            GetViewport().SetInputAsHandled();
        }
    }

    private void ApplyZoom(float factor)
    {
        var mouseWorldBefore = GetGlobalMousePosition();
        Zoom = (Zoom * factor).Clamp(new Vector2(MinZoom, MinZoom), new Vector2(MaxZoom, MaxZoom));
        Position += mouseWorldBefore - GetGlobalMousePosition();
    }
}
