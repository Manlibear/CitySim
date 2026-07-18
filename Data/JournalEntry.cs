using System;

namespace CitySim.Data;

public record JournalEntry
{
    public DateTime Timestamp { get; set; }
    public required string Text { get; set; }
    public Guid? OtherPersonID { get; set; }
}
