using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using Chickensoft.GoDotTest;
using CitySim.Components;
using CitySim.Data;
using CitySim.ECS;
using CitySim.Scripts;
using CitySim.Systems;
using Xunit;

namespace CitySim.Tests;

// Calibration harness, not a story-driven integration test: MoodSystem's two coefficients were
// picked with no real basis, so rather than asserting one fixed pair of numbers, we describe how
// a handful of scenarios *should* feel (as a mood band) and brute-force scan a coefficient grid
// looking for pairs that satisfy all of them at once. The printed table is the actual output —
// the assert is just a smoke check that a workable pair still exists after any tuning changes to
// the scenarios or the formula.
public class MoodCalibrationTest(Node testScene) : TestClass(testScene)
{
    private record MoodScenario(string Name, NeedsComponent Needs, List<IMemory> Memories, float MinMood, float MaxMood);

    private static readonly float[] NeedsModifiers = [.1f, .2f, .3f, .4f, .5f, .6f, .7f, .8f, .9f, 1f];
    private static readonly float[] MemoryMultipliers = [.001f, .002f, .005f, .01f, .02f, .05f, .1f];

    private List<MoodScenario> _scenarios = default!;

    [Setup]
    public void Setup()
    {
        SimWorld.Instance = new SimWorld
        {
            TimeSpeed = 1f,
            TimeMultiplier = 60f,
            DateTime = new DateTime(2026, 1, 1, 12, 0, 0),
        };

        _scenarios =
        [
            new MoodScenario(
                "Content, no memories",
                new NeedsComponent { Satiety = 1f, Energy = 1f, Social = 1f },
                [],
                MinMood: .8f, MaxMood: 1f),

            new MoodScenario(
                "Neglected needs, no memories",
                new NeedsComponent { Satiety = .4f, Energy = .4f, Social = .4f },
                [],
                MinMood: 0f, MaxMood: .2f),

            new MoodScenario(
                "Good needs, but grieving",
                new NeedsComponent { Satiety = 1f, Energy = 1f, Social = 1f },
                [new FinancialMemory { Satisfaction = -100f, IsPermanent = true }],
                MinMood: 0f, MaxMood: .3f),

            new MoodScenario(
                "Needs dipping, but recently thrilled",
                new NeedsComponent { Satiety = .6f, Energy = .6f, Social = .6f },
                [new FinancialMemory { Satisfaction = 50f, IsPermanent = true }],
                MinMood: .6f, MaxMood: 1f),

            new MoodScenario(
                "Right at the minimum thresholds, no memories",
                new NeedsComponent { Satiety = .3f, Energy = .3f, Social = .3f },
                [],
                MinMood: 0f, MaxMood: .3f),
        ];
    }

    [Test]
    public void ScanForCoefficientsSatisfyingAllScenarios()
    {
        var scores = new Dictionary<(float NeedsModifier, float MemoryMultiplier), int>();

        foreach (var needsModifier in NeedsModifiers)
        {
            foreach (var memoryMultiplier in MemoryMultipliers)
            {
                var passCount = _scenarios.Count(scenario => WithinBand(scenario, needsModifier, memoryMultiplier));
                scores[(needsModifier, memoryMultiplier)] = passCount;
            }
        }

        var table = new StringBuilder();
        table.AppendLine("-- MoodCalibrationTest: scenarios passed per (needsModifier, memoryMultiplier) --");
        table.Append("needs \\ memory".PadRight(16));
        foreach (var memoryMultiplier in MemoryMultipliers)
            table.Append(memoryMultiplier.ToString("0.000").PadLeft(8));
        table.AppendLine();

        foreach (var needsModifier in NeedsModifiers)
        {
            table.Append(needsModifier.ToString("0.0").PadRight(16));
            foreach (var memoryMultiplier in MemoryMultipliers)
                table.Append($"{scores[(needsModifier, memoryMultiplier)]}/{_scenarios.Count}".PadLeft(8));
            table.AppendLine();
        }
        GD.Print(table.ToString());

        var best = scores.OrderByDescending(x => x.Value).First();
        GD.Print($"= Best candidate: needsModifier={best.Key.NeedsModifier}, memoryMultiplier={best.Key.MemoryMultiplier} ({best.Value}/{_scenarios.Count} scenarios)");

        foreach (var scenario in _scenarios)
        {
            var mood = ComputeMood(scenario, best.Key.NeedsModifier, best.Key.MemoryMultiplier);
            GD.Print($"  - {scenario.Name}: mood={mood:F3} (expected [{scenario.MinMood:F2}, {scenario.MaxMood:F2}])");
        }

        Assert.True(best.Value == _scenarios.Count,
            $"No scanned coefficient pair satisfied every scenario band (best was {best.Value}/{_scenarios.Count}) — " +
            "either widen the grid or the scenario bands are mutually exclusive under the current linear formula.");
    }

    private static bool WithinBand(MoodScenario scenario, float needsModifier, float memoryMultiplier)
    {
        var mood = ComputeMood(scenario, needsModifier, memoryMultiplier);
        return mood >= scenario.MinMood && mood <= scenario.MaxMood;
    }

    private static float ComputeMood(MoodScenario scenario, float needsModifier, float memoryMultiplier)
    {
        var world = new World(1);
        var entity = world.CreateEntity();
        entity.Attach(scenario.Needs);
        entity.Attach(new MemoryComponent { Memories = scenario.Memories });
        entity.Attach(new MoodComponent());

        new MoodSystem(world, needsModifier, memoryMultiplier).Update(1.0);

        return entity.Get<MoodComponent>().Mood;
    }
}
