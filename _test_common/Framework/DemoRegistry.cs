using System.Reflection;

namespace _test_common.Framework;

/// <summary>Карта всех демок, найденных в сборке.</summary>
public sealed class DemoRegistry
{
    private readonly List<DemoEntry> _entries = [];
    public IReadOnlyList<DemoEntry> Entries => _entries;

    public static DemoRegistry Build(Assembly assembly)
    {
        var registry = new DemoRegistry();

        foreach (var type in assembly.GetTypes())
        {
            var classAttr = type.GetCustomAttribute<DemoClassAttribute>();
            if (classAttr is null) continue;

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var methodAttr = method.GetCustomAttribute<DemoMethodAttribute>();
                if (methodAttr is null) continue;

                var cases = method.GetCustomAttributes<DemoCaseAttribute>().ToList();
                if (cases.Count == 0) continue; // метод без кейсов игнорируем

                registry._entries.Add(new DemoEntry
                {
                    DemoType = type,
                    TargetType = classAttr.Target,
                    Category = classAttr.Category,
                    Method = method,
                    DisplayName = methodAttr.Name ?? method.Name,
                    Description = methodAttr.Description,
                    Cases = cases,
                });
            }
        }

        return registry;
    }

    /// <summary>Все публичные типы из указанной сборки, по которым нет демок.</summary>
    public IEnumerable<Type> UncoveredTypes(Assembly libraryAssembly)
    {
        var covered = _entries.Select(e => e.TargetType).ToHashSet();
        return libraryAssembly.GetExportedTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !covered.Contains(t))
            .OrderBy(t => t.FullName);
    }
}