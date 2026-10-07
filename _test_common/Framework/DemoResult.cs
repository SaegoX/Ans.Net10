namespace _test_common.Framework;

/// <summary>Результат прогона одного кейса.</summary>
public sealed class DemoResult
{
    public required string CaseLabel { get; init; }
    public required bool Success { get; init; }
    public object? Actual { get; init; }
    public object? Expected { get; init; }
    public bool ExpectException { get; init; }
    public bool ShouldVerify { get; init; }
    public Exception? Exception { get; init; }
    public TimeSpan Duration { get; init; }
}