using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Data;

namespace CitySim.Registries;

public static class EmployerRegistry
{
    private readonly static Dictionary<string, Dictionary<string, JobPosition>> employers = [];

    public static void AddEmployer(string name)
    {
        if (employers.ContainsKey(name)) return;

        employers.Add(name, []);
    }

    public static void AddJob(string employer, string title, decimal wage, List<ScheduleEntry> schedule)
    {
        if (!employers.ContainsKey(employer)) throw new ArgumentException($"Unknown employer \"{employer}\"");

        employers[employer].Add(title, new JobPosition(false, wage, schedule));
    }

    public static void MarkJobFilled(string employer, string title, bool filled = true)
    {
        if (!employers.ContainsKey(employer)) throw new ArgumentException($"Unknown employer \"{employer}\"");
        if (!employers[employer].ContainsKey(title)) throw new ArgumentException($"Unknown job title \"{title}\" for \"{employer}\"");

        employers[employer][title].Filled = filled;
    }

    public static string[] GetVacanciesOfType(string employer, string title)
    {
        if (!employers.ContainsKey(employer)) throw new ArgumentException($"Unknown employer \"{employer}\"");
        // job titles are {title}_{LocationEntityID}, this prevents accidental hits
        title += "_";
        return [.. employers[employer].Where(x => x.Key.StartsWith(title) && !x.Value.Filled).Select(x => x.Key)];
    }

    public static (string Title, int Count, decimal Wage)[] GetVacancies(string employer)
    {
        if (!employers.ContainsKey(employer)) throw new ArgumentException($"Unknown employer \"{employer}\"");

        return [.. employers[employer].Where(x => !x.Value.Filled).Select(x => (x.Key.Split("_")[0], x.Value.Wage))
                                                                  .GroupBy(x => x.Item1)
                                                                  .Select(x => (x.First().Item1, x.Count(), x.First().Wage))];
    }

    public static decimal GetWage(string employer, string title)
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

public record JobPosition(bool filled, decimal wage, List<ScheduleEntry> schedule)
{
    public bool Filled { get; set; } = filled;
    public decimal Wage { get; set; } = wage;
    public List<ScheduleEntry> Schedule { get; set; } = schedule;
}
