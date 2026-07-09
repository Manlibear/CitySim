using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CitySim.Data;

public enum Item
{
    WhiteBread,
}

public enum ItemType
{
    Food
}

// StandalonePhrase: used when there's no whole count, e.g. "Half a loaf of" -> "Half a loaf of white bread"
// CombinedFragment: bare word used when joined onto a whole count, e.g. "a half" -> "6 and a half loaves of white bread"
// CombinedJoinsUnit: true if the fragment describes a fraction of a loaf ("and a half loaves"),
//                     false if it describes a separate remainder ("loaves and a few slices")
public record PartialTier(float Threshold, string StandalonePhrase, string CombinedFragment, bool CombinedJoinsUnit);

public record ItemDefinition
{
    public required ItemType Type { get; set; }
    public required string Name { get; set; }
    public required string[] Tags { get; set; }
    public required string UnitSingular { get; set; }
    public required string UnitPlural { get; set; }
    public required string Description { get; set; }
    public required int SlotMax { get; set; }
    public required bool PartialUsage { get; set; }
    public float? PartialUsageStep { get; set; }
    public NeedsDelta? NeedsDelta { get; set; }
    public PartialTier[]? PartialTiers { get; set; }

}
