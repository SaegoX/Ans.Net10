namespace _test_common.Framework;

/// <summary>Помечает класс-демку и говорит, какой тип из библиотеки он демонстрирует.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DemoClassAttribute : Attribute
{
    public Type Target { get; }
    public string Category { get; }

    public DemoClassAttribute(Type target, string? category = null)
    {
        Target = target;
        Category = category
            ?? target.Namespace?.Split('.').LastOrDefault()
            ?? "Прочее";
    }
}