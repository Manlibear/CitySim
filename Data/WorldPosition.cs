using Godot;
using CitySim.Registries;
using CitySim.Helpers;

namespace CitySim.Data;

public readonly record struct WorldPosition(string MapID, Vector2I Tile)
{
    public static WorldPosition Overworld(Vector2I tile) => new(MapRegistry.OverworldId, tile);


    public Vector2 ToGlobalPosition()
    {
        var layer = MapRegistry.GetLayer(MapID);
        return layer!.MapToGlobal(Tile);
    }
}
