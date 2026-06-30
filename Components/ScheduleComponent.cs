using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Scripts;

namespace CitySim.Components;

public class ScheduleComponent : IComponent
{
    private List<ScheduleEntry> Schedule { get; set; } = [];

    public void AddEntry(ScheduleEntry entry) => Schedule.Add(entry);
    public void RemoveEntry(ScheduleEntry entry) => Schedule.Remove(entry);
    public ScheduleEntry? GetNext()
    {
        if (!Schedule.Any()) return null;

        var now = SimWorld.Instance.DateTime;
        var nowMins = (int)now.DayOfWeek * 1440 + now.Hour * 60 + now.Minute;

        return Schedule
            .Select(e =>
            {
                var entryMins = (int)e.Day * 1440 + e.Time.Hour * 60 + e.Time.Minute;
                var delta = entryMins - nowMins;
                if (delta <= 0) delta += 7 * 1440; // if the Day is before now, it's actuallly next week, bump it
                return (entry: e, delta);
            })
            .MinBy(x => x.delta)
            .entry;
    }
}