namespace CitySim.Data;

public enum ActivityType
{
    Idle,
    Work,
    Eat,
    Sleep,
    Liesure,
    Social
}

public enum ActivityPriority
{
    Idle = int.MinValue,
    Sick = 1,
    Exhausted = 1,
    Starving = 1,
    Work = 2,
    Bored = 3,
    Lonely = 3,
    Default = 3
}