using System;
using CitySim.ECS;

namespace CitySim.Components;

public class SocialInteractionComponent : IComponent
{
    public Guid TargetEntityID { get; set; }
    public SocialInteractionStatus Status { get; set; }
    public bool Positive { get; set; }
    public double Duration { get; set; }
}

public enum SocialInteractionStatus
{
    Pending,
    InProgress,
    Ticking
}
