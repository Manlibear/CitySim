using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CitySim.ECS;

namespace CitySim.Data.StateEffects;

public static class AttachComponentEffect
{
    public static AttachComponentEffect<T> Create<T>(T instance) where T : class, IComponent, new() => new(instance);
}

public class AttachComponentEffect<T>(T? instance = null) : IStateEffect where T : class, IComponent, new()
{
    private static readonly PropertyInfo[] WritableProperties = [.. typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanWrite)];

    public void Apply(Entity entity, params object[] info)
    {
        instance ??= new T();

        foreach (var arg in info)
        {
            if (arg is not ITuple { Length: 2 } tuple || tuple[0] is not string name)
                continue;

            var property = WritableProperties.FirstOrDefault(p => p.Name == name);
            if (property is not null && property.PropertyType.IsInstanceOfType(tuple[1]))
                property.SetValue(instance, tuple[1]);
        }

        entity.Attach(instance);
    }
}
