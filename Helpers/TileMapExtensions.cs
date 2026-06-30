using Godot;

namespace CitySim.Helpers;

public static class TileMapExtensions
{
    public static Vector2 MapToGlobal(this TileMapLayer mapLayer, Vector2I position)
    {
        return mapLayer.ToGlobal(mapLayer.MapToLocal(position));
    }
}