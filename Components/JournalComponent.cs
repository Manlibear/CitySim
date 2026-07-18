using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Scripts;

namespace CitySim.Components;

public class JournalComponent : IComponent
{
    [JsonInclude]
    private List<JournalEntry> Entries { get; set; } = [];

    public void AddEntry(string text, Guid? otherPerson)
    {
        Entries.Add(new JournalEntry()
        {
            OtherPersonID = otherPerson,
            Text = text,
            Timestamp = SimWorld.Instance.DateTime
        });
    }

    public IEnumerable<JournalEntry> GetAllEntries() => GetEntries(DateTime.MinValue, DateTime.MaxValue);
    public IEnumerable<JournalEntry> GetEntriesFrom(DateTime start) => GetEntries(start, DateTime.MaxValue);
    public IEnumerable<JournalEntry> GetEntriesUntil(DateTime end) => GetEntries(DateTime.MinValue, end);
    public IEnumerable<JournalEntry> GetEntries(DateTime start, DateTime end)
    {
        return Entries.Where(x => x.Timestamp >= start && x.Timestamp <= end);
    }
}
