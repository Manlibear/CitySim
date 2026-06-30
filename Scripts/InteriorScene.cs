using System.Linq;
using Godot;
using CitySim.Data;
using CitySim.Registries;

namespace CitySim.Scripts;

// Attach to the root node of any building interior scene.
// Set MapID and OverworldDoorTile before adding to the scene tree
// (BuildingPresenter does this when it preloads the interior at bootstrap).
public partial class InteriorScene : Node2D
{
    public string MapID { get; set; } = "";
    public Vector2I OverworldDoorTile { get; set; }
    [Export] public Vector2I ExitTile { get; set; }

    public override void _Ready()
    {
        if (string.IsNullOrEmpty(MapID))
        {
            GD.PushError($"InteriorScene on {Name} has no MapID set. Was it instantiated via BuildingPresenter?");
            return;
        }

        var layers = GetChildren().OfType<TileMapLayer>().ToArray();
        MapRegistry.Register(MapID, layers);
        MapRegistry.AddWarp(
               new WorldPosition(MapID, ExitTile),
               new WorldPosition(MapRegistry.OverworldId, OverworldDoorTile)
           );
    }
}
