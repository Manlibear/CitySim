using System;
using CitySim.Data;
using Godot;

namespace CitySim.Helpers;

public static class Vector2IExtensions
{
    public static int ManhattanDistanceFrom(this Vector2I p1, Vector2I p2) => Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
}

public static class Vector2IHelper
{
    public static Vector2I FromFacingDirection(FacingDirection direction)
    {
        return direction switch
        {
            FacingDirection.South => Vector2I.Down,
            FacingDirection.North => Vector2I.Up,
            FacingDirection.East => Vector2I.Right,
            FacingDirection.West => Vector2I.Left,
            _ => throw new ArgumentException("Unhandled direction"),
        };
    }
}
