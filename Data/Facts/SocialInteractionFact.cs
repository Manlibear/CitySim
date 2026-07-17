using System;

namespace CitySim.Data.Facts;

public class SocialInteractionFact : IFact
{
    public bool Positive { get; set; }
    public double Duration { get; set; }
    public Guid OtherPersonID {get;set;}
}
