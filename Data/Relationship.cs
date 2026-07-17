using System.Collections.Generic;

namespace CitySim.Data;

public class Relationship
{
    public float Score { get; set; }
    public RelationshipType Type { get; set; }
    public List<IMemory> Memories { get; set; } = [];

}

public enum RelationshipType
{
    Stranger,
    Brother,
    Sister,
    Son,
    Daughter,
    Mother,
    Father,
    Aunt,
    Uncle,
    Grandmother,
    Grandfather,
    Coworker,
    Acquaintance,
    Friend,
    BestFriend,
    Dating,
}
