using System;
using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.Facts;
using CitySim.Data.StateEffects;
using CitySim.ECS;
using CitySim.Helpers;
using CitySim.Registries;
using Godot;

namespace CitySim.Systems;

public class JobSystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        // TODO: Actually check if they want a job, they may be freelance (if that's something we do), a child, or retired
        foreach (var entity in world.Entities.Without<JobSeekingComponent>().Without<JobComponent>().With<CitizenComponent>())
            entity.Attach(new JobSeekingComponent());

        foreach (var entity in world.Entities.With<JobSeekingComponent>().With<SkillsComponent>().Without<JobApplicantComponent>())
        {
            //TODO: Once again, handle homeless people
            if (entity.TryGet<HomeComponent>(out var homeComp))
            {
                var homeLocation = LocationRegistry.Get(homeComp!.MapID, MapRegistry.OverworldId) ?? throw new ArgumentException($"Unable to find home '{homeComp.MapID}'");
                var skillsComp = entity.Get<SkillsComponent>();
                var jobSeekingComp = entity.Get<JobSeekingComponent>();

                var employersWithJobs = EmployerRegistry.GetQualifiedVacancies(skillsComp.GetSkills(), .1f);

                if (employersWithJobs.Length == 0) continue;

                var bestScore = decimal.MinValue;
                (string Employer, string Job) preference = ("", "");

                //TODO: this will currently expire, do we want people to re-apply for old jobs?
                // or do we say yes, and then have a list of previous employees kept by the employer
                // and increase the chance for interview failure?
                var negativePreviousEmployers = entity.Get<MemoryComponent>().Memories.Where(x => x is JobMemory jm && jm.Satisfaction < 0).Select(x => (x as JobMemory)!.Employer);

                foreach (var (Employer, Jobs) in employersWithJobs)
                {
                    if(negativePreviousEmployers.Contains(Employer)) continue;

                    var jobLocation = LocationRegistry.Get(Employer, MapRegistry.OverworldId) ?? throw new ArgumentException($"Unable to find employer Location '{Employer}'");

                    foreach (var job in Jobs)
                    {
                        if (jobSeekingComp.AlreadyApplied.Contains(job.Key)) continue;

                        var distFromHome = homeLocation.Position.Tile.ManhattanDistanceFrom(jobLocation.Position.Tile);

                        // TODO: This should be take into account things like Skills.Endurance and Preferences
                        var score = job.Value.Wage - (int)(distFromHome * Globals.CommuterDistancePenaltyPerUnit);

                        if (score > bestScore)
                        {
                            bestScore = score;
                            preference = (Employer, job.Key);
                        }
                    }
                }

                if (bestScore > decimal.MinValue)
                {
                    entity.Attach(new JobApplicantComponent()
                    {
                        Employer = preference.Employer,
                        Job = preference.Job
                    });
                }
            }
        }

        foreach (var entity in world.Entities.With<JobApplicantComponent>().Without<PathfindingComponent>())
        {
            // Once dispatched, the applicant sits in ActivityType.Interview until it completes.
            // Without this guard, PathfindingComponent clearing on arrival would make this loop
            // redispatch them to the interview every tick, endlessly resetting the timer.
            if (entity.TryGet<ActivityTypeComponent>(out var activityComp) && activityComp!.Type == ActivityType.Interview) continue;

            var jobApplicantComp = entity.Get<JobApplicantComponent>();
            Location? location = null;
            if (jobApplicantComp.Job == "Manager")
            {
                // manager jobs are handled at home, as if it were a telephone/web interview with head office
                var homeComp = entity.Get<HomeComponent>();
                location = LocationRegistry.Resolve($"/{homeComp.MapID}/@admin");
            }
            else
            {
                location = LocationRegistry.Get("ManagerVisitor", jobApplicantComp.Employer) ?? throw new ArgumentException($"Unable to find ManagerVisitor location for '{jobApplicantComp.Employer}'");
            }

            if (location == null) throw new ArgumentException($"Unable to find interview location for {jobApplicantComp.Employer} {jobApplicantComp.Job}");

            entity.Attach(new PathfindingComponent()
            {
                Destination = location.Position,
                OnArriveEffects = [
                      new ActivityTypeEffect(ActivityType.Interview, ActivityPriority.Work, durationHours: .5){
                           OnCompleteEffects = [
                               new DetachComponentEffect<JobApplicantComponent>(),
                               AttachComponentEffect.Create(new JobApplicationResultComponent(){
                                    Employer = jobApplicantComp.Employer,
                                    Job = jobApplicantComp.Job
                               })
                           ]
                      }
                  ]
            });
        }

        foreach (var entity in world.Entities.With<JobApplicationResultComponent>().With<SkillsComponent>().ToList())
        {
            var jobApplicationResult = entity.Get<JobApplicationResultComponent>();
            var skillsComp = entity.Get<SkillsComponent>();
            var memoryComp = entity.Get<MemoryComponent>();
            var moodComp = entity.Get<MoodComponent>();
            var jobSkills = EmployerRegistry.GetJobSkills(jobApplicationResult.Employer, jobApplicationResult.Job);

            var negativeInterviewsCount = memoryComp.Memories.Count(x => x is ConfidenceMemory cm && cm.Type == ActivityType.Interview && cm.Satisfaction < 0);

            // Mood is 0-1, so centre it on .5f - a sour mood drags both bounds down, a good one lifts them.
            var moodModifier = (moodComp.Mood - .5f) * Globals.MoodInterviewModifier;

            // TODO: this is just quick math, refine these numbers
            var interviewRoll = new RandomNumberGenerator().RandfRange(.8f - (negativeInterviewsCount / 15f) + moodModifier, 1 + skillsComp.GetSkill(Skill.Charisma) / 30f + moodModifier);

            bool success = true;

            foreach (var skill in jobSkills)
            {
                if (skillsComp.GetSkill(skill.Key) * interviewRoll < skill.Value)
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                EmployerRegistry.MarkJobFilled(jobApplicationResult.Employer, jobApplicationResult.Job);
                entity.Detach<JobSeekingComponent>();
                entity.Attach(new JobComponent() { Employer = jobApplicationResult.Employer, Title = jobApplicationResult.Job });
            }
            else
            {
                entity.Get<JobSeekingComponent>().AlreadyApplied.Add(jobApplicationResult.Job);
            }

            entity.Detach<JobApplicationResultComponent>();
            entity.Get<FactComponent>().Add(new JobInterviewFact() { Success = success });
        }

        foreach (var entity in world.Entities.With<JobComponent>().ToList())
        {
            var jobComp = entity.Get<JobComponent>();

            if (jobComp.Performance < Globals.MinJobPerformance)
            {
                EmployerRegistry.MarkJobFilled(jobComp.Employer, jobComp.Title, filled: false);
                entity.Get<FactComponent>().Add(new FiredFromJobFact(){ Employer = jobComp.Employer });
                entity.Detach<JobComponent>();
            }
        }


    }
}
