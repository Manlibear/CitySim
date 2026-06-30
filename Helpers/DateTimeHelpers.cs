using System;

namespace CitySim.Helpers;

public static class DateTimeHelpers
{
    public static string ToOrdinal(this DateTime dt)
    {
        var day = dt.Day;
        var suffix = day switch
        {
            11 or 12 or 13 => "th",
            _ when day % 10 == 1 => "st",
            _ when day % 10 == 2 => "nd",
            _ when day % 10 == 3 => "rd",
            _ => "th"
        };
        return $"{day}{suffix} {dt:MMMM yyyy HH:mm}";
    }
}