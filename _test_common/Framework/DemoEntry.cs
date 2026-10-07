using System.Reflection;

namespace _test_common.Framework;

/// <summary>Одна запись реестра: демка одного метода одного класса библиотеки.</summary>
public sealed class DemoEntry
{
    public required Type DemoType { get; init; }
    public required Type TargetType { get; init; }
    public required string Category { get; init; }
    public required MethodInfo Method { get; init; }
    public required string DisplayName { get; init; }
    public required string? Description { get; init; }
    public required IReadOnlyList<DemoCaseAttribute> Cases { get; init; }
}