using System;

namespace CitySim.Data.Facts;

public class BrowseResultFact(Guid entityID, ItemType itemType, string tag, bool success, float totalScore) : IFact
{
    public Guid EntityID { get; set; } = entityID;
    public float TotalScore { get; set; } = totalScore;
    public ItemType ItemType { get; set; } = itemType;
    public string Tag { get; set; } = tag;
    public bool Success { get; set; } = success;
}
