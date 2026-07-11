using System.Collections.Generic;
using System.Text.Json.Serialization;
using CitySim.Data;
using CitySim.ECS;
using Godot;

namespace CitySim.Components;

public class SkillsComponent : IComponent
{
    [JsonInclude]
    private Dictionary<Skill, float> _skills = [];

    public Dictionary<Skill, float> GetSkills() => _skills;

    public float GetSkill(Skill skill)
    {
        if (_skills.ContainsKey(skill))
            return _skills[skill];

        return 0;
    }

    public void IncreaseSkill(Skill skill, float amount)
    {
        if (!_skills.ContainsKey(skill))
            _skills.Add(skill, amount);
        else
            _skills[skill] += amount;

        _skills[skill] = Mathf.Clamp(_skills[skill], 0, 10);
    }

    public SkillsComponent WithSkill(Skill skill, float amount)
    {
        _skills.Add(skill, amount);
        return this;
    }
}
