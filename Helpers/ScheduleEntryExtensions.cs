using System.Collections.Generic;
using System.Linq;
using CitySim.Data;
using CitySim.Scripts;

namespace CitySim.Helpers;

public static class ScheduleEntryExtensions
{
    public static ScheduleEntry? GetNext(this List<ScheduleEntry> schedule)
    {
        if (!schedule.Any()) return null;

        var immediate = schedule.FirstOrDefault(x => x.IsImmediate);
        if (immediate != null) return immediate;

        var now = SimWorld.Instance.DateTime;
        var nowMins = (int)now.DayOfWeek * 1440 + now.Hour * 60 + now.Minute;

        return schedule
            .Where(x => x.Day != null && x.Time != null)
            .Select(e =>
            {
                var entryMins = (int)e.Day! * 1440 + e.Time!.Value.Hour * 60 + e.Time!.Value.Minute;
                var delta = entryMins - nowMins;
                if (delta <= 0) delta += 7 * 1440; // if the Day is before now, it's actuallly next week, bump it
                return (entry: e, delta);
            })
            .MinBy(x => x.delta)
            .entry;
    }
}
