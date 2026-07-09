using System.Collections.Generic;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Registries;

namespace CitySim.Components;

public class JobComponent : IComponent
{
    public required string Employer { get; set; }
    public required string Title { get; set; }
    private ScheduledTransaction? _wage { get; set; }
    private List<ScheduleEntry>? _schedule { get; set; }

    public ScheduledTransaction Wage
    {
        get
        {
            if (_wage == null)
                UpdateWage();

            return _wage!;
        }
    }

    public void UpdateWage()
    {
        _wage ??= new ScheduledTransaction();
        _wage.Amount = EmployerRegistry.GetWage(Employer, Title);
    }

    public List<ScheduleEntry> Schedule
    {
        get
        {
            if (_schedule == null)
                UpdateSchedule();

            return _schedule!;
        }
    }

    public void UpdateSchedule()
    {
        _schedule = EmployerRegistry.GetSchedule(Employer, Title);
    }
}
