using System;

namespace CitySim.Data;

public record MoodSample
{
    public required DateTime Timestamp { get; set; }
    public required float Mood { get; set; }
}
