using System;
using Godot;

namespace CitySim.Helpers;

public static class Vector2IExtensions
{
    public static int ManhattanDistanceFrom(this Vector2I p1, Vector2I p2) => Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
}