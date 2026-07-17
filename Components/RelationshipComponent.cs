
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CitySim.Data;
using CitySim.ECS;

namespace CitySim.Components;

public class RelationshipComponent : IComponent
{
    [JsonInclude]
    [JsonPropertyName("Relationships")]
    private Dictionary<Guid, Relationship> _relationships { get; set; } = [];

    public Relationship? GetRelationship(Guid person) => _relationships.ContainsKey(person) ? _relationships[person] : null;

    public void AddRelationship(Guid person)
    {
        if (!_relationships.ContainsKey(person))
            _relationships.Add(person, new Relationship());
    }

    public void UpdateRelationship(Guid person, float delta)
    {
        AddRelationship(person);

        _relationships[person].Score += delta;
    }

    public void AddMemory(Guid person, IMemory memory)
    {
        AddRelationship(person);

        _relationships[person].Memories.Add(memory);
    }

    public void SetType(Guid person, RelationshipType type)
    {
        AddRelationship(person);

        _relationships[person].Type = type;
    }

}
