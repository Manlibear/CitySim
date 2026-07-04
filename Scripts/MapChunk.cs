using System.Linq;
using CitySim.Data;
using CitySim.Registries;
using Godot;

namespace CitySim.Scripts;

public partial class MapChunk : Node2D
{
    public void Bootstrap()
    {
        var mapLayers = GetChildren().OfType<TileMapLayer>().ToArray();
        if (mapLayers.Length > 0)
            MapRegistry.RegisterSubMap(MapRegistry.OverworldId, (Vector2I)Transform.Origin / Globals.TileSize, mapLayers);
    }
}