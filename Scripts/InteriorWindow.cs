using System.Linq;
using CitySim.Helpers;
using Godot;

namespace CitySim.Scripts;

// Attach to the root Control node of the interior popup panel.
// Scene structure expected:
//   InteriorWindow (Control)  ← this script
//     └── SubViewportContainer
//          └── SubViewport   ← assign to Viewport export
//               └── Camera2D (with CameraController script)
public partial class InteriorWindow : Control
{
    public static InteriorWindow? Instance { get; private set; }

    [Export] public SubViewport Viewport { get; set; } = null!;

    private InteriorScene? _current;
    private Node? _originalParent;

    public override void _Ready()
    {
        Instance = this;
        Visible = false;
    }

    // Takes ownership of the scene temporarily — reparents it into the SubViewport.
    // BuildingPresenter keeps the scene alive; we just borrow it.
    public void Open(InteriorScene interior)
    {
        Close();

        _current = interior;
        _originalParent = interior.GetParent();
        interior.Reparent(Viewport);
        interior.Visible = true;

        FitCamera();
        Visible = true;
    }

    private void FitCamera()
    {
        var camera = Viewport.GetNodeOrNull<Camera2D>("Camera2D");
        if (camera == null || _current == null) return;

        var layers = _current.GetChildren().OfType<TileMapLayer>().ToList();
        if (layers.Count == 0) return;

        var minX = float.MaxValue; var minY = float.MaxValue;
        var maxX = float.MinValue; var maxY = float.MinValue;

        foreach (var layer in layers)
        {
            foreach (var cell in layer.GetUsedCells())
            {
                var world = layer.MapToGlobal(cell);
                if (world.X < minX) minX = world.X;
                if (world.Y < minY) minY = world.Y;
                if (world.X > maxX) maxX = world.X;
                if (world.Y > maxY) maxY = world.Y;
            }
        }

        if (minX == float.MaxValue) return;

        var halfTile = new Vector2(
            (layers[0].TileSet?.TileSize.X ?? 32) * 0.5f,
            (layers[0].TileSet?.TileSize.Y ?? 32) * 0.5f
        );
        var boundsMin = new Vector2(minX, minY) - halfTile;
        var boundsMax = new Vector2(maxX, maxY) + halfTile;
        var boundsSize = boundsMax - boundsMin;

        var zoomX = Viewport.Size.X / boundsSize.X;
        var zoomY = Viewport.Size.Y / boundsSize.Y;
        var zoom = Mathf.Min(zoomX, zoomY) * 0.9f;

        camera.Position = (boundsMin + boundsMax) * 0.5f;
        camera.Zoom = new Vector2(zoom, zoom);
    }

    public void Close()
    {
        if (_current != null)
        {
            _current.Visible = false;
            if (_originalParent != null && IsInstanceValid(_originalParent))
                _current.Reparent(_originalParent);
            _current = null;
            _originalParent = null;
        }
        Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
            Close();
    }
}
