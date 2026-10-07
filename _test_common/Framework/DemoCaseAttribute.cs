namespace _test_common.Framework;

/// <summary>Один вариант входных параметров для демки.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class DemoCaseAttribute : Attribute
{
    public string Label { get; }
    public object?[] Args { get; }

    /// <summary>Ожидаемое значение. Если null и Verify=false — результат не проверяется.</summary>
    public object? Expected { get; init; }

    /// <summary>Проверять результат, даже если Expected == null (кейс «ожидаем null»).</summary>
    public bool Verify { get; init; }

    /// <summary>Ожидаем, что вызов бросит исключение.</summary>
    public bool ExpectException { get; init; }

    public DemoCaseAttribute(string label, params object?[] args)
    {
        Label = label;
        Args = args ?? [];
    }

    /// <summary>Надо ли сравнивать Actual с Expected.</summary>
    public bool ShouldVerify => Verify || Expected is not null;
} 