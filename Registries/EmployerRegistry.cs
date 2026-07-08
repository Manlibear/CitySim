using System;
using System.Collections.Generic;
using CitySim.Data;

namespace CitySim.Registries;

public static class EmployerRegistry
{
    private static Dictionary<string, Dictionary<string, (double Wage, List<ScheduleEntry> Schedule)>> employers = [];

    public static void AddEmployer(string name)
    {
        if (employers.ContainsKey(name)) return;

        employers.Add(name, []);
    }

    public static void AddJob(string employer, string title, double wage, List<ScheduleEntry> schedule)
    {
        if (!employers.ContainsKey(employer)) throw new ArgumentException($"Unknown employer \"{employer}\"");

        employers[employer].Add(title, (wage, schedule));
    }

    public static double GetWage(string employer, string title)
    {
        if (!employers.ContainsKey(employer)) throw new ArgumentException($"Unknown employer \"{employer}\"");
        if (!employers[employer].ContainsKey(title)) throw new ArgumentException($"Unknown job title \"{title}\" for \"{employer}\"");

        return employers[employer][title].Wage;
    }

    public static List<ScheduleEntry> GetSchedule(string employer, string title)
    {
        if (!employers.ContainsKey(employer)) throw new ArgumentException($"Unknown employer \"{employer}\"");
        if (!employers[employer].ContainsKey(title)) throw new ArgumentException($"Unknown job title \"{title}\" for \"{employer}\"");

        return employers[employer][title].Schedule;
    }
}
