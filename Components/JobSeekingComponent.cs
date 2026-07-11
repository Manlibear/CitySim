
using System.Collections.Generic;
using CitySim.ECS;

namespace CitySim.Components;

public class JobSeekingComponent : IComponent
{
    public List<string> AlreadyApplied {get;set;} = [];
}
