using System.Collections.Generic;
using CitySim.Data.Facts;
using CitySim.ECS;

namespace CitySim.Components;


public class FactComponent : IComponent
{
    public Queue<IFact> Facts { get; set; } = [];

    public void Add(IFact fact) => Facts.Enqueue(fact);
}
