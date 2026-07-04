using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CitySim.Data.StateEffects;
using CitySim.Registries;
using CitySim.Scripts;
using Godot;

namespace CitySim.Data;

public class ScheduleEntry
{
    public required DayOfWeek Day { get; set; }
    public required TimeOnly Time { get; set; }
    public bool Repeats { get; set; }
    public required string LocationPath { get; set; }
    [JsonIgnore] public WorldPosition? Position { get; set; } = null;
    [JsonIgnore] public Queue<WorldPosition>? CachedPath { get; set; } = null;
    [JsonIgnore] public DateTime? DispatchTime { get; set; } = null;
    public IStateEffect[]? OnArriveEffects { get; set; } = null;

    [JsonConstructor]
    public ScheduleEntry() { }

    public ScheduleEntry(string locationPath, DayOfWeek day, TimeOnly time, bool repeats = false)
    {
        Day = day;
        Time = time;
        LocationPath = locationPath;
        Repeats = repeats;
    }

    public ScheduleEntry(string locationPath, bool repeats = false)
    {
        var minutes = (SimWorld.Instance.DateTime.Minute / 5 + 1) * 5;
        var nextAvailableTime = new DateTime(SimWorld.Instance.DateTime.Year, SimWorld.Instance.DateTime.Month, SimWorld.Instance.DateTime.Day, SimWorld.Instance.DateTime.Hour, 0, 0).AddMinutes(minutes);
        Day = nextAvailableTime.DayOfWeek;
        Time = TimeOnly.FromDateTime(nextAvailableTime);
        LocationPath = locationPath;
        Repeats = repeats;
    }
}