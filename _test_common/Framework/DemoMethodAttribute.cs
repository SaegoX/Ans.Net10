namespace _test_common.Framework;

/// <summary>Помечает метод-демку. Имя/описание — для отображения в меню.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class DemoMethodAttribute : Attribute
{
    public string? Name { get; init; }
    public string? Description { get; init; }
}