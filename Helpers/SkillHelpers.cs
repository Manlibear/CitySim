using System.Collections.Generic;
using CitySim.Data;

namespace CitySim.Helpers;

public static class SkillHelpers
{
    public static bool SatisfiedBy(this Dictionary<Skill, float> required, Dictionary<Skill, float> skills, float margin = 0)
    {
        foreach (var req in required)
        {
            if (!skills.ContainsKey(req.Key)) return false;
            if ((skills[req.Key] * (1 + margin)) < required[req.Key]) return false;
        }

        return true;

    }
}
