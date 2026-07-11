using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Data;
using CitySim.Helpers;

namespace CitySim.Registries;

public static class EmployerRegistry
{
    private readonly static Dictionary<string, Dictionary<string, JobPosition>> employers = [];

    public static void AddEmployer(string name)
    {
        if (employers.ContainsKey(name)) return;

        employers.Add(name, []);
    }

    public static void AddJob(string employer, string title, decimal wage, List<ScheduleEntry> schedule, Dictionary<Skill, float> requiredSkills)
    {
        if (!employers.ContainsKey(employer)) throw new ArgumentException($"Unknown employer \"{employer}\"");

        employers[employer].Add(title, new JobPosition() { Filled = false, RequiredSkills = requiredSkills, Schedule = schedule, Wage = wage });
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

    public static (string Employer, string[] JobTitles)[] GetVacanciesOfType(string title)
    {
        // job titles are {title}_{LocationEntityID}, this prevents accidental hits
        title += "_";
        return [..employers.Select(x => (Employer: x.Key, Jobs: x.Value ))
            .Where(x => x.Jobs.Any(j => j.Key.StartsWith(title) && !j.Value.Filled))
            .Select(x => (x.Employer, JobTitles: (string[])[..x.Jobs.Select(y => y.Key)] ))];
    }

    public static (string Employer, Dictionary<string, JobPosition> Jobs)[] GetQualifiedVacancies(Dictionary<Skill, float> skills, float margin = 0)
    {
        return [.. employers
            .Select(x => (Employer: x.Key, Jobs: x.Value
                .Where(j => !j.Value.Filled && j.Value.RequiredSkills.SatisfiedBy(skills, margin))
                .ToDictionary(j => j.Key, j => j.Value)))
            .Where(x => x.Jobs.Count > 0)];
    }

    public static Dictionary<Skill, float> GetJobSkills(string employer, string job)
    {
        if (!employers.ContainsKey(employer)) throw new ArgumentException($"Unknown employer \"{employer}\"");
        if (!employers[employer].ContainsKey(job)) throw new ArgumentException($"Unknown job \"{job}\" for employer \"{employer}\"");

        return employers[employer][job].RequiredSkills;
    }

    public static (string Title, int Count, decimal Wage)[] GetVacanciesByEmployer(string employer)
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

public record JobPosition
{
    public required bool Filled { get; set; }
    public required decimal Wage { get; set; }
    public required List<ScheduleEntry> Schedule { get; set; }
    public required Dictionary<Skill, float> RequiredSkills { get; set; }
}
