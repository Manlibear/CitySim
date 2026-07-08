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
    public IReadOnlyList<ScheduleEntry> Entries => Schedule;
    public void AddEntry(ScheduleEntry entry) => Schedule.Add(entry);
    public void RemoveEntry(ScheduleEntry entry) => Schedule.Remove(entry);
}