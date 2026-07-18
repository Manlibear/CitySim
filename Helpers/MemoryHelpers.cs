using System;
using System.Collections.Generic;
using System.Linq;
using CitySim.Data;

namespace CitySim.Helpers;

public static class MemoryHelpers
{
    public static IEnumerable<IMemory> OfOtherPerson(this List<IMemory> memories, Guid otherPersonID)
    {
        return memories.Where(x => (x is SocialInteractionMemory sim && sim.OtherPersonID == otherPersonID));
    }
}
