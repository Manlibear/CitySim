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
         switch (direction)
        {
            case FacingDirection.South:
                return Vector2I.Down;

            case FacingDirection.North:
                return Vector2I.Up;

            case FacingDirection.East:
                return  Vector2I.Right;

            case FacingDirection.West:
                return Vector2I.Left;

            default:
                throw new ArgumentException("Unhandled direction");
        }
    }
}