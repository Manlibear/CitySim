using System.Collections.Generic;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.StateEffects;

namespace CitySim.Registries;
public static class StateEffectRegistry
{
    private static readonly Dictionary<ActivityType, List<IStateEffect>> _byState = [];
    private static readonly Dictionary<(ActivityType From, ActivityType To), List<IStateEffect>> _byEdge = [];

    public static void Register(ActivityType state, params IStateEffect[] effects) =>
        _byState[state] = [.. effects];

    public static void Register(ActivityType from, ActivityType to, params IStateEffect[] effects) =>
        _byEdge[(from, to)] = [.. effects];

    public static IEnumerable<IStateEffect> Get(ActivityType from, ActivityType to) =>
        _byEdge.TryGetValue((from, to), out var edge) ? edge
        : _byState.TryGetValue(to, out var state) ? state
        : [];

    public static void Initialize()
    {
        Register(ActivityType.Sleep, new AttachComponentEffect<SleepComponent>(), new PlayAnimationEffect("sleep"));
        Register(ActivityType.WakeUp, new DetachComponentEffect<SleepComponent>(), new PlayAnimationEffect("idle"));
    }
}