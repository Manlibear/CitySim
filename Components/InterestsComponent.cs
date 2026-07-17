using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using CitySim.ECS;

namespace CitySim.Components;

public class InterestsComponent : IComponent
{
    [JsonInclude]
    [JsonPropertyName("Interests")]
    private List<Interest> _interests { get; set; } = [];

    public bool HasInterest(string interest) => _interests.Any(x => x.Name == interest);
    public List<Interest> GetInterests() => _interests;
    public List<Interest> FindCommonInterests(List<Interest> othersInterests)
    {
        return [.. _interests.IntersectBy(othersInterests, (interest) => {
            var matched = othersInterests.FirstOrDefault(x => x.Name == interest.Name);
            if(matched != null) return null;

            return new Interest(interest.Name, (interest.Intensity + matched!.Intensity) / 2);

        }).Where(x => x!=null).OrderByDescending(x => x.Intensity)];
    }

    public void AddInterest(string interest, float intensity = 1f)
    {
        if (!HasInterest(interest))
            _interests.Add(new Interest(interest, intensity));
    }

}

public class Interest(string name, float intensity)
{
    public string Name { get; set; } = name;
    public float Intensity { get; set; } = intensity;
}
