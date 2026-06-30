using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CitySim.Registries;
using CitySim.Scripts;
using Godot;

namespace CitySim.Data;

public class ScheduleEntry
{
    public required DayOfWeek Day { get; set; }
    public required TimeOnly Time { get; set; }
    public required ActivityType Type { get; set; }
    public int Priority { get; set; } = 3;
    public bool Repeats { get; set; }
    public required string LocationPath { get; set; }
    public WorldPosition? Position { get; set; } = null;
    public Queue<WorldPosition>? CachedPath { get; set; } = null;
    public DateTime? DispatchTime { get; set; } = null;
    public StatePayload? StatePayload { get; set; } = null;

    [JsonConstructor]
    public ScheduleEntry() { }

    public ScheduleEntry(string locationPath, ActivityType type, DayOfWeek day, TimeOnly time, int priority = 3, bool repeats = false)
    {
        Day = day;
        Time = time;
        Priority = priority;
        LocationPath = locationPath;
        Type = type;
        Repeats = repeats;
    }

    public ScheduleEntry(string locationPath, ActivityType type, int priority = 3, bool repeats = false)
    {
        var minutes = (SimWorld.Instance.DateTime.Minute / 5 + 1) * 5;
        var nextAvailableTime = new DateTime(SimWorld.Instance.DateTime.Year, SimWorld.Instance.DateTime.Month, SimWorld.Instance.DateTime.Day, SimWorld.Instance.DateTime.Hour, 0, 0).AddMinutes(minutes);
        Day = nextAvailableTime.DayOfWeek;
        Time = TimeOnly.FromDateTime(nextAvailableTime);
        Priority = priority;
        LocationPath = locationPath;
        Type = type;
        Repeats = repeats;
    }
}